namespace InventoryDashboard.Models.Response
{
    public class Warehouse
    {
        public string WarehouseID { get; init; } = string.Empty;
        public string WarehouseName { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty ;
        public int TotalStock { get; set; } = 0 ;

    }
}
