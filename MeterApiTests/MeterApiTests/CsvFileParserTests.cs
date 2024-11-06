
using CsvHelper;
using MeterApi.CsvOps;
using Microsoft.Extensions.Options;
using Moq;
using CsvParser = MeterApi.CsvOps.CsvParser;

namespace MeterApiTests;

public class CsvFileParserTests
{
    [Fact]
    public async Task ParseNullFilePath()
    {
        var optionsMock = new Mock<IOptions<CsvParserOptions>>();
        optionsMock.Setup(x => x.Value).Returns(new CsvParserOptions()
        {
            MaxFileLengthBytes = 1024
        });
        var csvFileParser = new CsvParser(optionsMock.Object);
        
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await csvFileParser.ParseCsvFile(null!).ToListAsync());
        Assert.Equal($"Value cannot be null. (Parameter 'csvStream')", exception.Message);
    }

    [Fact]
    public async Task ParseInvalidFile()
    {
        var optionsMock = new Mock<IOptions<CsvParserOptions>>();
        optionsMock.Setup(x => x.Value).Returns(new CsvParserOptions()
        {
            MaxFileLengthBytes = 1024
        });
        var csvFileParser = new CsvParser(optionsMock.Object);

        var fileStream = File.Open("./TestData/InvalidCsv.csv", FileMode.Open);
        var records = csvFileParser.ParseCsvFile(fileStream);
        await Assert.ThrowsAsync<HeaderValidationException>(
            async () =>
            {
                await foreach (var record in records)
                {
                }
            });
    }
    
    [Fact]
    public async Task ParseTooLongFile()
    {
        var optionsMock = new Mock<IOptions<CsvParserOptions>>();
        optionsMock.Setup(x => x.Value).Returns(new CsvParserOptions()
        {
            MaxFileLengthBytes = 1
        });
        var csvFileParser = new CsvParser(optionsMock.Object);

        var fileStream = File.Open("./TestData/Readings.csv", FileMode.Open);
        var records = csvFileParser.ParseCsvFile(fileStream);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
            {
                await foreach (var record in records)
                {
                }
            });
        Assert.Equal($"Sorry, file too long. Max file size is 1 bytes.", exception.Message);
    }
}