using InventoryDashboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryDashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        [HttpGet("summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDashboardSummaryAsync()
        {
            try
            {
                var summary = await _dashboardService.GetDashboardSummaryAsync();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard summary");
                return StatusCode(500, "Internal server error");
            }
        }



        [HttpGet("Products")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductsAsync()
        {
            try
            {
                var products = await _dashboardService.GetProductsAsync();

                if (!products.Any())
                    return NotFound("No products found.");

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("Warehouses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWarehousesAsync()
        {
            try
            {
                var warehouses = await _dashboardService.GetWarehousesAsync();

                if (!warehouses.Any())
                    return NotFound("No warehouses found.");

                return Ok(warehouses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching warehouses");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("Inventory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInventoryAsync()
        {
            try
            {
                var inventory = await _dashboardService.GetInventoryAsync();

                if (!inventory.Any())
                    return NotFound("No inventory found.");

                return Ok(inventory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching inventory");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
