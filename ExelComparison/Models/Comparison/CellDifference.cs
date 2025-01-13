namespace ExelComparison.Models
{
    public class CellDifference
    {
        public string SheetName { get; set; }
        public string CellAddress { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
    }
}
