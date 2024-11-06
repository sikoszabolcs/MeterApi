using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;

namespace MeterApi.Controllers;

public class CsvParser
{
    private readonly CsvParserOptions _options;

    public CsvParser()
    {
        
    }
    public CsvParser(IOptions<CsvParserOptions> options)
    {
        _options = options.Value;
    }

    public virtual async IAsyncEnumerable<MeterReading> ParseCsvFile(Stream csvStream)
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

    public virtual int MaxFileLengthBytes => _options.MaxFileLengthBytes;
}