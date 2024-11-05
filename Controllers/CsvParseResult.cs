namespace MeterApi.Controllers;

public class CsvParseResult
{
    public string FileName { get; set; } = string.Empty;
    public int SuccessCount { get; set; }
    public int FailCount { get; set; }
}