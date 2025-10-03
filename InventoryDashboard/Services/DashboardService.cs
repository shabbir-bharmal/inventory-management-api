using Azure.Storage.Blobs;
using ClosedXML.Excel;
using InventoryDashboard.Models.Response;

namespace InventoryDashboard.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IConfiguration _configuration;

        public DashboardService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private async Task<List<Dictionary<string, string>>> GetSheetDataAsync(string sheetName)
        {
            string connectionString = _configuration["BlobStorage:ConnectionString"] ?? string.Empty;
            string containerName = _configuration["BlobStorage:ContainerName"] ?? string.Empty;
            string blobFileName = _configuration["BlobStorage:FileName"] ?? string.Empty;

            var containerClient = new BlobContainerClient(connectionString, containerName);
            var blobClient = containerClient.GetBlobClient(blobFileName);

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
                Capacity = int.TryParse(r["Capacity"], out var cap) ? cap : 0
            }).ToList();
        }

        public async Task<List<Inventory>> GetInventoryAsync()
        {
            var rows = await GetSheetDataAsync("Inventory");
            return rows.Select(r => new Inventory
            {
                ProductID = r["ProductID"],
                WarehouseID = r["WarehouseID"],
                Quantity = int.TryParse(r["Quantity"], out var qty) ? qty : 0
            }).ToList();
        }
    }
}
