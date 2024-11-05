using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace MeterApi.Controllers;

public class CsvParseResult
{
    public string FileName { get; set; } = string.Empty;
    public int SuccessCount { get; set; }
    public int FailCount { get; set; }
}

public class CsvFileParser
{
    public CsvFileParser(uint maxFileLengthBytes = 1024)
    {
        MaxFileLengthBytes = maxFileLengthBytes;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="csvFilePath"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<CsvParseResult> ParseCsvFile(string csvFilePath)
    {
        if (string.IsNullOrEmpty(csvFilePath))
        {
            throw new InvalidOperationException("The file path is empty.");
        }
        
        try
        {
            var csvFileStream = File.Open(csvFilePath, FileMode.Open);
            if (csvFileStream.Length > MaxFileLengthBytes)
            {
                throw new InvalidOperationException($"Sorry, file too long. Max file size is {MaxFileLengthBytes} bytes.");
            }
            await ParseCsvFile(csvFileStream);

            return new CsvParseResult
            {
                FileName = csvFilePath,
                SuccessCount = LastSuccessCount,
                FailCount = LastFailureCount
            };
        }
        catch (FileNotFoundException e)
        {
            throw new InvalidOperationException($"Error parsing CSV file {csvFilePath}! The file was not found.");
        }
    }

    public async Task<CsvParseResult> ParseCsvFile(Stream csvFileStream)
    {
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
        await foreach (var _ in csv.GetRecordsAsync<MeterReading>())
        {
            LastSuccessCount += 1;
        }

        return new CsvParseResult
        {
            SuccessCount = LastSuccessCount,
            FailCount = LastFailureCount
        };
    }

    public int LastFailureCount { get; private set; }
    
    public int LastSuccessCount { get; private set; }
    
    
    public uint MaxFileLengthBytes { get; init; }
}