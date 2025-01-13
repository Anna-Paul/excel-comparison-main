using Aspose.Cells;
using ExelComparison.Models;
using ExelComparison.Services.Conversion;

namespace ExelComparison.Services.Comparison
{
    public class ComparisonService : IComparisonService
    {
        private readonly IConversionService _conversionService;
        private readonly ILogger<ComparisonService> _logger;
        private readonly string _tempPath;

        public ComparisonService(ILogger<ComparisonService> logger, IConversionService conversionService) 
        {
            _conversionService = conversionService;
            _logger = logger;
            _tempPath = Path.GetTempPath();
        }

        public async Task<ComparisonResult> CompareFiles(IFormFile file1, IFormFile file2)
        {
            var tempFile1 = Path.Combine(_tempPath, $"comp1_{Guid.NewGuid()}.xlsx");
            var tempFile2 = Path.Combine(_tempPath, $"comp2_{Guid.NewGuid()}.xlsx");

            try
            {
                using (var stream1 = new FileStream(tempFile1, FileMode.Create))
                    await file1.CopyToAsync(stream1);
                using (var stream2 = new FileStream(tempFile2, FileMode.Create))
                    await file2.CopyToAsync(stream2);

                using var workbook1 = new Workbook(tempFile1);
                using var workbook2 = new Workbook(tempFile2);

                var differences = CompareWorkbooks(workbook1, workbook2);

                var html1 = _conversionService.ConvertWorkbookToHtml(workbook1);
                var html2 = _conversionService.ConvertWorkbookToHtml(workbook2);

                html2 = _conversionService.ReplaceTableCells(html2, differences);

                return new ComparisonResult
                {
                    Differences = differences,
                    Html1 = html1,
                    Html2 = html2
                };
            }
            finally
            {
                CleanupTempFiles(tempFile1, tempFile2);
            }
        }

        private List<CellDifference> CompareWorkbooks(Workbook workbook1, Workbook workbook2)
        {
            var differences = new List<CellDifference>();

            for (int sheetIndex = 0; sheetIndex < workbook1.Worksheets.Count; sheetIndex++)
            {
                if (sheetIndex >= workbook2.Worksheets.Count) break;

                var sheet1 = workbook1.Worksheets[sheetIndex];
                var sheet2 = workbook2.Worksheets[sheetIndex];

                for (int row = 0; row <= Math.Max(sheet1.Cells.MaxRow, sheet2.Cells.MaxRow); row++)
                {
                    for (int col = 0; col <= Math.Max(sheet1.Cells.MaxColumn, sheet2.Cells.MaxColumn); col++)
                    {
                        var cell1 = sheet1.Cells[row, col];
                        var cell2 = sheet2.Cells[row, col];

                        if (!AreCellsEqual(cell1, cell2))
                        {
                            differences.Add(new CellDifference
                            {
                                SheetName = sheet1.Name,
                                CellAddress = String.Concat(ExcelColumnIndexToName(col), row + 1),
                                Row = row,
                                Column = col,
                                Value1 = GetCellValue(cell1),
                                Value2 = GetCellValue(cell2)
                            });
                        }
                    }
                }
            }

            return differences;
        }

        private string ExcelColumnIndexToName(int Index)
        {
            string range = string.Empty;
            if (Index < 0) return range;
            int a = 26;
            int x = (int)Math.Floor(Math.Log((Index) * (a - 1) / a + 1, a));
            Index -= (int)(Math.Pow(a, x) - 1) * a / (a - 1);
            for (int i = x + 1; Index + i > 0; i--)
            {
                range = ((char)(65 + Index % a)).ToString() + range;
                Index /= a;
            }
            return range;
        }

        private static bool AreCellsEqual(Cell cell1, Cell cell2)
        {
            var value1 = GetCellValue(cell1);
            var value2 = GetCellValue(cell2);

            return value1 == value2;
        }

        private static string GetCellValue(Cell cell)
        {
            if (cell == null || cell.Value == null)
                return string.Empty;

            switch (cell.Type)
            {
                case CellValueType.IsDateTime:
                    return cell.DateTimeValue.ToString("yyyy-MM-dd HH:mm:ss");
                case CellValueType.IsNumeric:
                    return cell.DoubleValue.ToString("G");
                default:
                    return cell.Value.ToString();
            }
        }

        private void CleanupTempFiles(string tempFile1, string tempFile2)
        {
            var filesToDelete = new[] { tempFile1, tempFile2 };

            foreach (var file in filesToDelete)
            {
                try
                {
                    if (System.IO.File.Exists(file))
                    {
                        System.IO.File.Delete(file);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete temporary file: {File}", file);
                }
            }
        }
    }
}
