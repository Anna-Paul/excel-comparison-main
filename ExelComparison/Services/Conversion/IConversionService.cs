using Aspose.Cells;
using ExelComparison.Models;

namespace ExelComparison.Services.Conversion
{
    public interface IConversionService
    {
        string ConvertWorkbookToHtml(Workbook excelFile);

        string ReplaceTableCells(string htmlContent, IEnumerable<CellDifference> differences);
    }
}
