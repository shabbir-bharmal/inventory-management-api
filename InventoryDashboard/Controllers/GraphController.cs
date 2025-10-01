using InventoryDashboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryDashboard.Controllers
{
    public class GraphController : Controller
    {
        private readonly GraphService _graphService;

        public GraphController(GraphService graphService)
        {
            _graphService = graphService;
        }

        [HttpGet("read")]
        public async Task<IActionResult> ReadExcel()
        {
            var filePath = "YOUR_FILE_ID"; // 🔸 adjust path
            var sheetName = "Sheet1";

            var values = await _graphService.ReadExcelAsync(filePath, sheetName);
            return Ok(values);
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadExcel()
        {

            string blobStorageConnectionString = "{BLOB_STORAGE_CONNECTION_STRING}";
            string blobStorageContainerName = "{BLOB_STORAGE_CONTAINER_NAME}";

            var containerClient = new BlobContainerClient(blobStorageConnectionString, blobStorageContainerName);
            var blobClient = containerClient.GetBlobClient("{FILE_NAME}");

            if (!await blobClient.ExistsAsync())
            {
                return NotFound("File not found in blob storage.");
            }

            var stream = new MemoryStream();
            await blobClient.DownloadToAsync(stream);
            stream.Position = 0;

            return File(
                stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "report.xlsx"
            );
        }

    }
}
