using ExelComparison.Models;

namespace ExelComparison.Services.Comparison
{
    public interface IComparisonService
    {
        Task<ComparisonResult> CompareFiles(IFormFile file1, IFormFile file2);
    }
}
