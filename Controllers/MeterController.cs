using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MeterApi.Controllers;

[ApiController]
[Route("meter-reading-uploads")]
public class MeterController(AppDbContext dbContext, ILogger<MeterController> logger) : Controller
{
    private readonly ILogger<MeterController> _logger = logger;
    private readonly AppDbContext _dbContext = dbContext;
    private const int MaxFileLengthBytes = 1024;
    private readonly CsvFileParser _csvFileParser = new(MaxFileLengthBytes);
    
    [HttpPost(Name = "PostMeterReadingUploads")]
    public async Task<IActionResult> PostMeterReadingUploads(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is invalid");
        }

        if (!file.FileName.ToLower().EndsWith(".csv"))
        {
            return BadRequest("File is not .csv");
        }

        if (file.Length > MaxFileLengthBytes)
        {
            return BadRequest($"File is too big. Max supported file size is {MaxFileLengthBytes} bytes.");
        }

        try
        {
            var successCount = 0;
            var failCount = 0;
            
            var accountIds = _dbContext.Accounts.Select(a => a.Id).ToHashSet();

            await using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                await foreach (var record in _csvFileParser.ParseCsvFile(file.OpenReadStream()))
                {
                    record.Instant = DateTime.SpecifyKind(record.Instant, DateTimeKind.Utc);
                    var lastReading =
                        _dbContext.Readings
                            .Where(reading => reading.AccountId == record.AccountId)
                            .OrderByDescending(reading => reading.Id)
                            .FirstOrDefault();

                    if (lastReading?.Value != record.Value)
                    {
                        if (record.Value is >= 0 and <= 99999)
                        {
                            if (accountIds.Contains(record.AccountId))
                            {
                                _dbContext.Readings.Add(record);
                                continue;
                            }

                            _logger.LogWarning($"Invalid account id {record.AccountId} for record with AccountId {record.AccountId}, Instant {record.Instant.ToString(CultureInfo.InvariantCulture)}, Value {record.Value}");
                        }
                        else
                        {
                            _logger.LogWarning($"Value out of range for record with AccountId {record.AccountId}, Instant {record.Instant.ToString(CultureInfo.InvariantCulture)}, Value {record.Value}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Value already logged at {lastReading?.Instant.ToString(CultureInfo.InvariantCulture)} for record with A|ccountId {record.AccountId}, Instant {record.Instant.ToString(CultureInfo.InvariantCulture)}, Value {record.Value}");
                    }
                    
                    failCount++;
                }
                
                try
                {
                    successCount = await _dbContext.SaveChangesAsync();
                    // Commit the transaction
                    await transaction.CommitAsync();
                }
                catch (DbUpdateException ex)
                {
                    
                }
            }

            return Ok($"{successCount}/{_csvFileParser.LastFailureCount + failCount}");
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
    }
}