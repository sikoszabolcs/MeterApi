namespace MeterApi.Controllers;

public class CsvParserOptions
{
    public const string CsvParser = "CsvParser";
    public int MaxFileLengthBytes { get; set; }
}