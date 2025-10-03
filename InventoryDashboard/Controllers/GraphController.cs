using Azure.Storage.Blobs;
using ClosedXML.Excel;
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


        [HttpGet("GetDataFromExcel")]
        public IActionResult GetDataFromExcel()
        {
            Dictionary<string, List<List<string>>> result = _graphService.GetDataFromExcel();
            return Ok(result);
        }

    }
}
