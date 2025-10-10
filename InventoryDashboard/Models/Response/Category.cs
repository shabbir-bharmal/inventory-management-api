namespace InventoryDashboard.Models.Response
{
    public class CategoryStock
    {
        public string Category { get; set; } = string.Empty;
        public int TotalStock { get; set; }
    }
}
