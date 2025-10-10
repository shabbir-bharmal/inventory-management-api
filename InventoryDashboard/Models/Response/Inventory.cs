namespace InventoryDashboard.Models.Response
{
    public class Inventory
    {
        public string WarehouseID { get; set; } = string.Empty;
        public string Warehouse { get; set; } = string.Empty;
        public string ProductID { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
    }
}
