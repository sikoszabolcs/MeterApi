using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;

namespace MeterApi.Controllers;

public class CsvParserOptions
{
    public const string CsvParser = "CsvParser";
    public int MaxFileLengthBytes { get; set; }
}

public class CsvParser
{
    private readonly CsvParserOptions _options;
    public CsvParser(IOptions<CsvParserOptions> options)
    {
        _options = options.Value;
    }

    public async IAsyncEnumerable<MeterReading> ParseCsvFile(Stream csvStream)
    {
        LastErrorCount = 0;
        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(
            reader,
            new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                ReadingExceptionOccurred = _ =>
                {
                    LastErrorCount += 1;
                    return false;
                }
            });
        await foreach (var record in csv.GetRecordsAsync<MeterReading>())
        {
            yield return record;
        }
    }

    public int LastErrorCount { get; private set; }

    public int MaxFileLengthBytes => _options.MaxFileLengthBytes;
}