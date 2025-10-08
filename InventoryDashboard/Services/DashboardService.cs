using Azure.Storage.Blobs;
using ClosedXML.Excel;
using InventoryDashboard.Models.Response;
using Microsoft.Extensions.Options;

namespace InventoryDashboard.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IConfiguration _configuration;
        private readonly string _blobConnectionString;
        private readonly string _blobContainerName;
        private readonly string _blobFileName;
        private readonly int _reorderThreshold;

        public DashboardService(IConfiguration configuration)
        {
            _configuration = configuration;
            _blobConnectionString = configuration["BlobStorage:ConnectionString"]
               ?? throw new InvalidOperationException("Missing BlobStorage:ConnectionString");

            _blobContainerName = configuration["BlobStorage:ContainerName"]
                ?? throw new InvalidOperationException("Missing BlobStorage:ContainerName");

            _blobFileName = configuration["BlobStorage:FileName"]
                ?? throw new InvalidOperationException("Missing BlobStorage:FileName");

            var reorderValue = configuration["InventorySettings:ReorderThreshold"];
            if (!int.TryParse(reorderValue, out _reorderThreshold))
                throw new InvalidOperationException("Invalid or missing InventorySettings:ReorderThreshold");
        }

        private async Task<List<Dictionary<string, string>>> GetSheetDataAsync(string sheetName)
        {
            var containerClient = new BlobContainerClient(_blobConnectionString, _blobContainerName);
            var blobClient = containerClient.GetBlobClient(_blobFileName);

            if (!await blobClient.ExistsAsync())
                return new List<Dictionary<string, string>>();

            using var stream = new MemoryStream();
            await blobClient.DownloadToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var sheet = workbook.Worksheets.FirstOrDefault(ws =>
                ws.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase));

            var result = new List<Dictionary<string, string>>();
            if (sheet == null) return result;

            var headerRow = sheet.Row(1).Cells().Select(c => c.GetValue<string>()).ToList();

            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                var dict = new Dictionary<string, string>();
                for (int i = 0; i < headerRow.Count; i++)
                {
                    dict[headerRow[i]] = row.Cell(i + 1).GetValue<string>();
                }
                result.Add(dict);
            }

            return result;
        }

        public async Task<DashboardSummary> GetDashboardSummaryAsync()
        {
            try
            {
                var productRows = await GetSheetDataAsync("Product");
                var warehouseRows = await GetSheetDataAsync("Warehouse");
                var inventoryRows = await GetSheetDataAsync("Inventory");

                var products = productRows.Select(r => new Product
                {
                    ProductID = r["ProductID"],
                    ProductName = r["ProductName"],
                    Category = r.ContainsKey("Category") ? r["Category"] : "Unknown"
                }).ToList();

                var warehouses = warehouseRows.Select(r => new Warehouse
                {
                    WarehouseID = r["WarehouseID"],
                    Location = r.ContainsKey("Location") ? r["Location"] : r["WarehouseID"]
                }).ToList();

                var inventories = inventoryRows.Select(r => new Inventory
                {
                    ProductID = r["ProductID"],
                    WarehouseID = r["WarehouseID"],
                    Quantity = int.TryParse(r["Quantity"], out var qty) ? qty : 0
                }).ToList();

                // Total stock of all products
                int TotalProductStock = inventories.Sum(p => p.Quantity);

                // Total stock per warehouse
                var stockPerWarehouse = inventories
                    .GroupBy(i => i.WarehouseID)
                    .Select(g =>
                    {
                        var warehouse = warehouses.FirstOrDefault(w => w.WarehouseID == g.Key);
                        return new Warehouse
                        {
                            WarehouseID = g.Key,
                            WarehouseName = warehouse?.Location ?? g.Key,
                            TotalStock = g.Sum(x => x.Quantity)
                        };
                    })
                    .ToList();

                var stockPerCategory = inventories
                    .GroupBy(i => products.FirstOrDefault(p => p.ProductID == i.ProductID)?.Category ?? "Unknown")
                    .Select(g => new CategoryStock
                    {
                        Category = g.Key,
                        TotalStock = g.Sum(x => x.Quantity)
                    })
                    .ToList();

                return new DashboardSummary
                {
                    TotalWarehouses = warehouses.Count,
                    TotalProducts = products.Count,
                    TotalProductStock = TotalProductStock,
                    StockPerWarehouse = stockPerWarehouse,
                    StockPerCategory = stockPerCategory
                };
            }
            catch (Exception ex)
            {
                // Optional: log exception or rethrow with context
                throw new ApplicationException("Error generating dashboard summary.", ex);
            }
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            var rows = await GetSheetDataAsync("Product");
            return rows.Select(r => new Product
            {
                ProductID = r["ProductID"],
                ProductName = r["ProductName"],
                Category = r["Category"],
                Price = decimal.TryParse(r["Price"], out var price) ? price : 0
            }).ToList();
        }

        public async Task<List<Warehouse>> GetWarehousesAsync()
        {
            var rows = await GetSheetDataAsync("Warehouse");
            return rows.Select(r => new Warehouse
            {
                WarehouseID = r["WarehouseID"],
                Location = r["Location"],
                WarehouseName = (r["WarehouseName"])
            }).ToList();
        }

        public async Task<List<Inventory>> GetInventoryAsync()
        {
            string connectionString = _configuration["BlobStorage:ConnectionString"] ?? string.Empty;
            string containerName = _configuration["BlobStorage:ContainerName"] ?? string.Empty;
            string blobFileName = _configuration["BlobStorage:FileName"] ?? string.Empty;

            var containerClient = new BlobContainerClient(connectionString, containerName);
            var blobClient = containerClient.GetBlobClient(blobFileName);

            if (!await blobClient.ExistsAsync())
                return new List<Inventory>();

            using var stream = new MemoryStream();
            await blobClient.DownloadToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);

            var productSheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals("Product", StringComparison.OrdinalIgnoreCase));
            var warehouseSheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals("Warehouse", StringComparison.OrdinalIgnoreCase));
            var inventorySheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals("Inventory", StringComparison.OrdinalIgnoreCase));

            if (productSheet == null || warehouseSheet == null || inventorySheet == null)
                return new List<Inventory>();

            var products = productSheet.RowsUsed().Skip(1).Select(row => new Product
            {
                ProductID = row.Cell(1).GetValue<string>(),
                ProductName = row.Cell(2).GetValue<string>()
            }).ToDictionary(p => p.ProductID);

            var warehouses = warehouseSheet.RowsUsed().Skip(1).Select(row => new Warehouse
            {
                WarehouseID = row.Cell(1).GetValue<string>(),
                WarehouseName = row.Cell(2).GetValue<string>(),
                Location = row.Cell(3).GetValue<string>()
            }).ToDictionary(w => w.WarehouseID);

            var inventoryDetails = new List<Inventory>();

            foreach (var row in inventorySheet.RowsUsed().Skip(1))
            {
                var productId = row.Cell(1).GetValue<string>();
                var warehouseId = row.Cell(2).GetValue<string>();
                var qty = int.TryParse(row.Cell(3).GetValue<string>(), out var q) ? q : 0;
                var reOrderLevel = int.TryParse(row.Cell(4).GetValue<string>(), out var rol) ? rol : 0;

                if (products.TryGetValue(productId, out var product) &&
                    warehouses.TryGetValue(warehouseId, out var warehouse))
                {
                    inventoryDetails.Add(new Inventory
                    {
                        Warehouse = warehouse.Location,
                        WarehouseID = warehouseId,
                        ProductID = productId,
                        Product = product.ProductName,
                        Quantity = qty,
                        ReorderLevel = reOrderLevel
                    });
                }
            }
            return inventoryDetails;
        }

    }
}
