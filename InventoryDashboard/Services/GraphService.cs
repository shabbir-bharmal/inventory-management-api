using Azure.Storage.Blobs;
using ClosedXML.Excel;

namespace InventoryDashboard.Services
{
    public class GraphService
    {

        private readonly IConfiguration _configuration;

        public GraphService(IConfiguration configuration)
        {

            _configuration = configuration;
        }

        public Dictionary<string, List<List<string>>> GetDataFromExcel()
        {
            string blobStorageConnectionString = _configuration["BlobStorage:ConnectionString"];
            string blobStorageContainerName = _configuration["BlobStorage:ContainerName"];
            string clientId = _configuration["BlobStorage:ClientId"];
            var containerClient = new BlobContainerClient(blobStorageConnectionString, blobStorageContainerName);
            var blobClient = containerClient.GetBlobClient(clientId);
            var result = new Dictionary<string, List<List<string>>>();

            if (!blobClient.Exists())
                return result;

            using var stream = new MemoryStream();
            blobClient.DownloadToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);

            foreach (var worksheet in workbook.Worksheets)
            {
                var sheetData = new List<List<string>>();

                foreach (var row in worksheet.RowsUsed())
                {
                    var rowData = new List<string>();
                    foreach (var cell in row.CellsUsed())
                    {
                        rowData.Add(cell.GetValue<string>());
                    }
                    sheetData.Add(rowData);
                }

                result[worksheet.Name] = sheetData;
            }

            return result;
        }
    }
}
