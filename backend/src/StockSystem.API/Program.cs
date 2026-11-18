using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using StockSystem.API;
using StockSystem.API.Filters;
using StockSystem.Application;
using StockSystem.Domain.Common;
using StockSystem.Domain.Entities;
using StockSystem.Infrastructure;
using StockSystem.Infrastructure.Data;

const int MinJwtKeyLength = 32;

// Load a .env file if one exists in the working directory or any parent (repo root in local development).
// In containers/hosting platforms the variables come from the real environment instead.
try
{
    Env.NoClobber().TraversePath().Load();
}
catch (FileNotFoundException)
{
    // No .env file: rely on process environment variables.
}

var isDevelopment = string.Equals(
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase);

var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Is(isDevelopment ? LogEventLevel.Debug : LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

if (isDevelopment)
{
    // Containers/PaaS have ephemeral disks: log files are a local-development convenience only.
    loggerConfiguration.WriteTo.File(
        path: "logs/stocksystem-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
}

Log.Logger = loggerConfiguration.CreateLogger();

try
{
    Log.Information("Starting Stock System API");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // Map the documented environment variable names onto configuration keys.
    string? FromEnv(string name) => Environment.GetEnvironmentVariable(name);

    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["ConnectionStrings:DefaultConnection"] = FromEnv("DB_CONNECTION_STRING") ?? builder.Configuration.GetConnectionString("DefaultConnection"),
        ["Jwt:Key"] = FromEnv("JWT_KEY") ?? builder.Configuration["Jwt:Key"],
        ["Jwt:Issuer"] = FromEnv("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"],
        ["Jwt:Audience"] = FromEnv("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"],
        ["Jwt:ExpirationHours"] = FromEnv("JWT_EXPIRATION_HOURS") ?? builder.Configuration["Jwt:ExpirationHours"],
        ["Seed:AdminEmail"] = FromEnv("ADMIN_EMAIL") ?? builder.Configuration["Seed:AdminEmail"],
        ["Seed:AdminPassword"] = FromEnv("ADMIN_PASSWORD") ?? builder.Configuration["Seed:AdminPassword"],
        ["Seed:SampleData"] = FromEnv("SEED_SAMPLE_DATA") ?? builder.Configuration["Seed:SampleData"],
    });

    // Fail fast on missing or weak security configuration instead of failing later or running insecurely.
    var jwtKey = builder.Configuration["Jwt:Key"];
    var jwtIssuer = builder.Configuration["Jwt:Issuer"];
    var jwtAudience = builder.Configuration["Jwt:Audience"];

    if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("DefaultConnection")))
        throw new InvalidOperationException("DB_CONNECTION_STRING is not configured.");
    if (string.IsNullOrEmpty(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < MinJwtKeyLength)
        throw new InvalidOperationException($"JWT_KEY must be set and at least {MinJwtKeyLength} bytes long.");
    if (string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
        throw new InvalidOperationException("JWT_ISSUER and JWT_AUDIENCE must be set.");

    // CORS: every origin listed in CORS_ORIGINS (comma separated), else the configuration file, else the Vite dev server.
    var corsOrigins = (FromEnv("CORS_ORIGINS") ?? string.Empty)
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    if (corsOrigins.Length == 0)
        corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    if (corsOrigins.Length == 0)
        corsOrigins = new[] { "http://localhost:5173" };

    builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>())
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // The API usually runs behind a TLS-terminating proxy: honor X-Forwarded-* so client IPs
    // (rate limiting, logs) and the request scheme are correct.
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(Roles.Admin));
        options.AddPolicy(Policies.StaffOrAdmin, policy => policy.RequireRole(Roles.Admin, Roles.Staff));
    });

    // Throttle credential endpoints per client IP (complements per-account lockout).
    var authPermitPerMinute = builder.Configuration.GetValue("RateLimit:AuthPermitPerMinute", 10);
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy(RateLimitPolicies.Auth, context =>
            RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = authPermitPerMinute,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(corsOrigins)
                  .WithHeaders("Authorization", "Content-Type")
                  .WithMethods("GET", "POST", "PUT", "DELETE");
        });
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Stock System API",
            Version = "v1",
            Description = "Hardware Store Inventory Management System API"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Paste the token only; 'Bearer ' is added for you.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // Apply migrations and seed. A failure here is fatal: serving requests against an unusable database is worse than not starting.
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var seedOptions = new SeedOptions(
            app.Configuration["Seed:AdminEmail"],
            app.Configuration["Seed:AdminPassword"],
            bool.TryParse(app.Configuration["Seed:SampleData"], out var sample) && sample);

        await DbInitializer.SeedAsync(
            services.GetRequiredService<ApplicationDbContext>(),
            services.GetRequiredService<UserManager<ApplicationUser>>(),
            services.GetRequiredService<RoleManager<IdentityRole>>(),
            seedOptions);

        if (string.IsNullOrWhiteSpace(seedOptions.AdminEmail))
            Log.Warning("ADMIN_EMAIL/ADMIN_PASSWORD are not set: no initial admin user is created.");
        Log.Information("Database migrated and seeded");
    }

    app.UseForwardedHeaders();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseHsts();
        app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
        {
            var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            Log.Error(exception, "Unhandled exception for request {Method} {Path}",
                context.Request.Method, context.Request.Path);

            // Never expose exception details to clients.
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred."
            });
        }));
    }

    // Security headers (the API serves JSON only; Swagger UI in Development needs inline scripts, so no CSP there).
    app.Use(async (context, next) =>
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        if (!app.Environment.IsDevelopment())
            headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
        await next();
    });

    app.UseCors("AllowFrontend");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

    Log.Information("Stock System API is ready");
    app.Run();
}
catch (Exception ex) when (ex.GetType().Name is not "HostAbortedException" and not "StopTheHostException")
{
    // The filter lets design-time tooling (dotnet ef) and the test host stop the app without a fatal log.
    Log.Fatal(ex, "Application terminated unexpectedly");
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}

// Exposed for WebApplicationFactory in integration tests.
public partial class Program;
