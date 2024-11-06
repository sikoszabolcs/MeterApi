using EntityFrameworkCore.Testing.Moq;
using MeterApi.Controllers;
using MeterApi.CsvOps;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeterApiTests;

public class MeterControllerTests
{
    [Fact]
    public async Task PostMeterReadingUploads_WhenCalledWithValidReading_ShouldReturnOk()
    {
        var readingsInCsv = new List<MeterReading>
        {
            new()
            {
                AccountId = 1,
                Instant = DateTime.UtcNow,
                Value = 2345
            }
        };
        
        var mockedDbContext = Create.MockedDbContextFor<AppDbContext>();
        var loggerMock = new Mock<ILogger<MeterController>>();
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(x => x.FileName).Returns("readings.csv");
        fileMock.Setup(x => x.Length).Returns(512);
        var accountsCacheMock = new Mock<AccountsCache>();
        accountsCacheMock.Setup(x => x.Contains(It.IsAny<int>())).Returns(true);
        var csvParser = new Mock<CsvParser>();
        csvParser.Setup(x => x.MaxFileLengthBytes).Returns(1024);
        csvParser.Setup(x => x.ParseCsvFile(It.IsAny<Stream>())).Returns(readingsInCsv.ToAsyncEnumerable());
        
        MeterController meterController = new(mockedDbContext, csvParser.Object, accountsCacheMock.Object, loggerMock.Object);
        var result = await meterController.PostMeterReadingUploads(fileMock.Object);
        var okResult = result as OkObjectResult;
        
        Assert.NotNull(okResult);
        Assert.Equal("1/0", okResult.Value);
    }
    
    [Fact]
    public async Task PostMeterReadingUploads_WhenCalledWithOutOfRangeValue_ShouldReturnOkWithFailureCount()
    {
        var readingsInCsv = new List<MeterReading>
        {
            new()
            {
                AccountId = 1,
                Instant = DateTime.UtcNow,
                Value = 123456
            }
        };
        
        var mockedDbContext = Create.MockedDbContextFor<AppDbContext>();
        var loggerMock = new Mock<ILogger<MeterController>>();
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(x => x.FileName).Returns("readings.csv");
        fileMock.Setup(x => x.Length).Returns(512);
        var accountsCacheMock = new Mock<AccountsCache>();
        accountsCacheMock.Setup(x => x.Contains(It.IsAny<int>())).Returns(true);
        var csvParser = new Mock<CsvParser>();
        csvParser.Setup(x => x.MaxFileLengthBytes).Returns(1024);
        csvParser.Setup(x => x.ParseCsvFile(It.IsAny<Stream>())).Returns(readingsInCsv.ToAsyncEnumerable());
        
        MeterController meterController = new(mockedDbContext, csvParser.Object, accountsCacheMock.Object, loggerMock.Object);
        var result = await meterController.PostMeterReadingUploads(fileMock.Object);
        var okResult = result as OkObjectResult;
        
        Assert.NotNull(okResult);
        Assert.Equal("0/1", okResult.Value);
    }
    
    [Fact]
    public async Task PostMeterReadingUploads_WhenCalledWithHistoricRecord_ShouldReturnOkWithFailureCount()
    {
        var timestamp = DateTime.UtcNow;
        var readingsInCsv = new List<MeterReading>
        {
            new()
            {
                AccountId = 1,
                Instant = timestamp.AddMinutes(1),
                Value = 100
            },
            new()
            {
                AccountId = 1,
                Instant = timestamp,
                Value = 200
            }
        };
        
        var mockedDbContext = Create.MockedDbContextFor<AppDbContext>();
        var loggerMock = new Mock<ILogger<MeterController>>();
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(x => x.FileName).Returns("readings.csv");
        fileMock.Setup(x => x.Length).Returns(512);
        var accountsCacheMock = new Mock<AccountsCache>();
        accountsCacheMock.Setup(x => x.Contains(It.IsAny<int>())).Returns(true);
        var csvParser = new Mock<CsvParser>();
        csvParser.Setup(x => x.MaxFileLengthBytes).Returns(1024);
        csvParser.Setup(x => x.ParseCsvFile(It.IsAny<Stream>())).Returns(readingsInCsv.ToAsyncEnumerable());
        
        MeterController meterController = new(mockedDbContext, csvParser.Object, accountsCacheMock.Object, loggerMock.Object);
        var result = await meterController.PostMeterReadingUploads(fileMock.Object);
        var okResult = result as OkObjectResult;
        
        Assert.NotNull(okResult);
        Assert.Equal("1/1", okResult.Value);
    }
}