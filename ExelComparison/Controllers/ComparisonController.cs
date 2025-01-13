using Microsoft.AspNetCore.Mvc;
using Aspose.Cells;
using System.Text;
using ExelComparison.Services.Comparison;
using ExelComparison.Models;
using ExelComparison.Services.Conversion;

namespace ExcelComparison.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ComparisonController : ControllerBase
    {
        private readonly ILogger<ComparisonController> _logger;
        private readonly IComparisonService _comparisonService;
        private readonly IConversionService _conversionService;

        public ComparisonController(ILogger<ComparisonController> logger, IComparisonService comparisonService, IConversionService conversionService)
        {
            _comparisonService = comparisonService;
            _conversionService = conversionService;
            _logger = logger;
        }

        [HttpPost("compare")]
        [ProducesResponseType(typeof(ComparisonResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ComparisonResult>> CompareFiles(IFormFile file1, IFormFile file2)
        {
            try
            {
                return await _comparisonService.CompareFiles(file1, file2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing Excel files");

                return StatusCode(500, new { error = "Error processing Excel files" });
            }
        }
    }
}