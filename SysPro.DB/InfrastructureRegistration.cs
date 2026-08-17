using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SysPro.Application.Interfaces;
using SysPro.DB.Persistence;
using SysPro.DB.Repositories;

namespace SysPro.DB;

public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        bool enableSensitiveDataLogging = false)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
            if (enableSensitiveDataLogging)
                options.EnableSensitiveDataLogging();
        });

        services.AddScoped<IOrdersRepository, OrdersRepository>();
        services.AddScoped<IAppRepository, AppRepository>();

        return services;
    }
}