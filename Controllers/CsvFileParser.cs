using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace MeterApi.Controllers;

public class CsvFileParser
{
    public CsvFileParser(uint maxFileLengthBytes = 1024)
    {
        MaxFileLengthBytes = maxFileLengthBytes;
    }

    public async IAsyncEnumerable<MeterReading> ParseCsvFile(Stream csvFileStream)
    {
        LastFailureCount = 0;
        using var reader = new StreamReader(csvFileStream);
        using var csv = new CsvReader(
            reader,
            new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                ReadingExceptionOccurred = _ =>
                {
                    LastFailureCount += 1;
                    return false;
                }
            });
        await foreach (var record in csv.GetRecordsAsync<MeterReading>())
        {
            //LastSuccessCount += 1;
            yield return record;
        }
    }

    public int LastFailureCount { get; private set; }
    
    //public int LastSuccessCount { get; private set; }
    
    public uint MaxFileLengthBytes { get; init; }
}