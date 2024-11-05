using MeterApi.Controllers;

namespace MeterApiTests;


public class CsvFileParserTests
{
    [Fact]
    public async Task ParseEmptyFilePath()
    {
        var csvFileParser = new CsvFileParser();
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await csvFileParser.ParseCsvFile(string.Empty));
        Assert.Equal($"The file path is empty.", exception.Message);
    }

    [Fact]
    public async Task ParseNullFilePath()
    {
        var csvFileParser = new CsvFileParser();
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await csvFileParser.ParseCsvFile(null!));
        Assert.Equal($"The file path is empty.", exception.Message);
    }

    [Fact]
    public async Task ParseInexistentFilePath()
    {
        var csvFileParser = new CsvFileParser();
        var csvFilePath = "I_dont_exist.csv";
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await csvFileParser.ParseCsvFile(csvFilePath));
        Assert.Equal($"Error parsing CSV file {csvFilePath}! The file was not found.", exception.Message);
    }

    [Fact]
    public async Task ParseTooLongFile()
    {
        var maxFileLengthBytes = 1u;
        var csvFileParser = new CsvFileParser(maxFileLengthBytes);
        var csvFilePath = "TestData/Readings.csv";
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await csvFileParser.ParseCsvFile(csvFilePath));
        Assert.Equal($"Sorry, file too long. Max file size is {maxFileLengthBytes} bytes.", exception.Message);
    }
}