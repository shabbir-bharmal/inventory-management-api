namespace InventoryDashboard.Models.Response
{
    public class Inventory
    {
        public string ProductID { get; init; } = string.Empty;
        public string WarehouseID { get; init; } = string.Empty;
        public int Quantity { get; init; }
    }
}
