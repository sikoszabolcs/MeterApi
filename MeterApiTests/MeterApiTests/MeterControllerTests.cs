using System.Diagnostics.Metrics;
using MeterApi.Controllers;
using MeterApi.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;

namespace MeterApiTests;

public class MeterControllerTests
{
    [Fact]
    public async Task PostMeterReadingUploads_WhenCalledWithValidReading_ShouldReturnOk()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MeterController>>();
        
        var readingsInCsv = new List<MeterReading>
        {
            new()
            {
                AccountId = 1,
                Instant = DateTime.UtcNow,
                Value = 2345
            }
        };
        var csvParser = new Mock<CsvParser>();
        csvParser.Setup(x => x.MaxFileLengthBytes).Returns(1024);
        csvParser.Setup(x => x.ParseCsvFile(It.IsAny<Stream>())).Returns(readingsInCsv.ToAsyncEnumerable());
        
        
        var fakeAccounts = new List<Account>
        {
            new()
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe"
            }
        };
        var appDbContextMock = new Mock<AppDbContext>();
        appDbContextMock.Setup<DbSet<Account>>(x => x.Accounts)
            .ReturnsDbSet(fakeAccounts);
        appDbContextMock.Setup<Task<int>>(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(readingsInCsv.Count));

        var readingsInDb = new List<MeterReading>();
        appDbContextMock.Setup<EntityEntry<MeterReading>>(x => x.Readings.Add(It.IsAny<MeterReading>()))
            .Callback<MeterReading>(x => readingsInDb.Add(x));
        appDbContextMock.Setup<DbSet<MeterReading>>(x => x.Readings)
            .ReturnsDbSet(readingsInDb);
        
        var accountsCacheMock = new Mock<AccountsCache>();
        accountsCacheMock.Setup(x => x.Contains(It.IsAny<int>())).Returns(true);
        
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(x => x.FileName).Returns("readings.csv");
        fileMock.Setup(x => x.Length).Returns(512);
        
        
        //Act
        MeterController meterController = new(appDbContextMock.Object, csvParser.Object, accountsCacheMock.Object, loggerMock.Object);
        var result = await meterController.PostMeterReadingUploads(fileMock.Object);
        var okResult = result as OkObjectResult;
        
        //Assert
        Assert.NotNull(okResult);
        Assert.Equal("1/0", okResult.Value);
    }
}