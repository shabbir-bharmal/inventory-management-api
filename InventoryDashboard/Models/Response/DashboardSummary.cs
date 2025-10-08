namespace InventoryDashboard.Models.Response
{
    public class DashboardSummary
    {
        public int TotalWarehouses { get; set; }
        public int TotalProducts { get; set; }
        public int TotalProductStock { get; set; }
        public List<Warehouse> StockPerWarehouse { get; set; } = new();
        public List<CategoryStock> StockPerCategory { get; set; } = new();
    }
}