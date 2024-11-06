using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;

namespace MeterApi.CsvOps;

public interface ICsvParser
{
    public IAsyncEnumerable<MeterReading> ParseCsvFile(Stream csvStream);

    public int LastErrorCount { get; }

    public int MaxFileLengthBytes { get; }
}

public class CsvParser : ICsvParser
{
    private readonly CsvParserOptions _options;
    
    public CsvParser(IOptions<CsvParserOptions> options)
    {
        _options = options.Value;
    }

    public async IAsyncEnumerable<MeterReading> ParseCsvFile(Stream csvStream)
    {
        if (csvStream == null)
        {
            throw new ArgumentNullException(nameof(csvStream));
        }

        if (csvStream.Length > MaxFileLengthBytes)
        {
            throw new InvalidOperationException($"Sorry, file too long. Max file size is {MaxFileLengthBytes} bytes.");
        }
        
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