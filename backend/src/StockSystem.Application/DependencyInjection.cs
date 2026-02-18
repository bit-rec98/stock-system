using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StockSystem.Application.Mappings;

namespace StockSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfile));

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<MappingProfile>();

        return services;
    }
}
