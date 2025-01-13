using Aspose.Cells;
using ExelComparison.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace ExelComparison.Services.Conversion
{
    public class ConversionService : IConversionService
    {
        public string ConvertWorkbookToHtml(Workbook workbook)
        {
            HtmlSaveOptions options = new HtmlSaveOptions
            {
                ExportImagesAsBase64 = true, 
                ExportWorksheetCSSSeparately = false,
                ExportRowColumnHeadings = true,
                ExportCellCoordinate = true,
                ExportFormula = true,
                ExportDocumentProperties = true,
                ExportGridLines = true,
            };

            using (var htmlStream = new MemoryStream())
            {
                workbook.Save(htmlStream, options);

                string htmlContent = Encoding.UTF8.GetString(htmlStream.ToArray());

                return htmlContent;
            }
        }

        public string ReplaceTableCells(string htmlContent, IEnumerable<CellDifference> differences)
        {
            var changedCells = differences.Select(c => c.CellAddress);

            string tooltipStyles = @"
    <style>
        .yellow-background {
            background:#FFFF00;
        }
    </style>";

            htmlContent = htmlContent.Replace("</head>", tooltipStyles + "</head>");

            return Regex.Replace(htmlContent, @"<td([^>]*)>(.*?)</td>", match =>
            {
                string existingAttributes = match.Groups[1].Value;
                string cellContent = match.Groups[2].Value;

                string pattern = @"class='[^']*'";

                string attributesWithoutOldClass = Regex.Replace(existingAttributes, pattern, "");

                if (changedCells.Any(coord => existingAttributes.Contains($"excelCoordinate='{coord}'")))
                {
                    return $"<td{attributesWithoutOldClass} class=\"yellow-background {GetExistingClasses(existingAttributes)}\">{cellContent}</td>";
                }

                return $"<td{attributesWithoutOldClass} class=\"{GetExistingClasses(existingAttributes)}\">{cellContent}</td>";
            });
        }

        private string GetExistingClasses(string attributes)
        {
            var classMatch = Regex.Match(attributes, @"class\s*=\s*[""']([^""']*)[""']");
            return classMatch.Success ? classMatch.Groups[1].Value : "";
        }
    }
}
