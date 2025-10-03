namespace InventoryDashboard.Models.Response
{
    public class Product
    {
        public string ProductID { get; init; } = string.Empty;
        public string ProductName { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public decimal Price { get; init; }
    }
}
