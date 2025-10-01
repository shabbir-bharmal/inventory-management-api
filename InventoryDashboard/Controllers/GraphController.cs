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

    }
}
