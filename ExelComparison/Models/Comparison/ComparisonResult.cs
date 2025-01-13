using ExcelComparison.Controllers;

namespace ExelComparison.Models
{
    public class ComparisonResult
    {
        public List<CellDifference> Differences { get; set; }
        public string Html1 { get; set; }
        public string Html2 { get; set; }
    }
}
