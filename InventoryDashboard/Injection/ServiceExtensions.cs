using InventoryDashboard.Services;

namespace InventoryDashboard.DependencyInjection
{
    public static class ServiceExtensions
    {
        // Single method to register all services
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IDashboardService, DashboardService>();

            return services;
        }
    }
}
