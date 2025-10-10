using InventoryDashboard.Models.Response;

namespace InventoryDashboard.Services
{
    public interface IDashboardService
    {
        Task<List<Product>> GetProductsAsync();
        Task<List<Warehouse>> GetWarehousesAsync();
        Task<List<Inventory>> GetInventoryAsync();
        Task<DashboardSummary> GetDashboardSummaryAsync();
    }
}
