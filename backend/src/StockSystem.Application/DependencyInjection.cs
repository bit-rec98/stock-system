using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StockSystem.Application.Validators;

namespace StockSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<RegisterValidator>();

        return services;
    }
}
