using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MeterApi.Controllers;

[ApiController]
[Route("meter-reading-uploads")]
public class MeterController(
    AppDbContext dbContext,
    CsvParser parser,
    AccountsCache accountsCache,
    ILogger<MeterController> logger) : Controller
{
    private readonly AppDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly ILogger<MeterController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly AccountsCache _accountsCache = accountsCache?? throw new ArgumentNullException(nameof(accountsCache));
    private readonly CsvParser _csvParser = parser ?? throw new ArgumentNullException(nameof(parser));

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

        if (file.Length > _csvParser.MaxFileLengthBytes)
        {
            return BadRequest($"File is too big. Max supported file size is {_csvParser.MaxFileLengthBytes} bytes.");
        }

        try
        {
            var successCount = 0;
            var insertErrorCount = 0;

            await foreach (var record in _csvParser.ParseCsvFile(file.OpenReadStream()))
            {
                if (record == null)
                {
                    _logger.LogError("Record is null");
                    continue;
                }
                
                record.Instant = DateTime.SpecifyKind(record.Instant, DateTimeKind.Utc);
                // var lastReading =
                //     _dbContext.Readings
                //         .Where(reading => reading.AccountId == record.AccountId)
                //         .OrderByDescending(reading => reading.Id)
                //         .FirstOrDefault();
                
                var lastReading =
                    _dbContext.Readings.Local
                        .Where(reading => reading.AccountId == record.AccountId)
                        .OrderByDescending(reading => reading.Id)
                        .FirstOrDefault();

                var checksOk = true;
                if (lastReading != null)
                {
                    if (lastReading.Instant > record.Instant)
                    {
                        checksOk = false;
                        _logger.LogWarning(
                            $"Tried to log a historic entry for record with AccountId {record.AccountId}, Instant {record.Instant.ToString(CultureInfo.InvariantCulture)}, Value {record.Value}. New entries must be older than {lastReading?.Instant.ToString(CultureInfo.InvariantCulture)} ");
                    }
                }

                if (record.Value is < 0 or > 99999)
                {
                    checksOk = false;
                    _logger.LogWarning(
                        $"Value out of range for record with AccountId {record.AccountId}, Instant {record.Instant.ToString(CultureInfo.InvariantCulture)}, Value {record.Value}");
                }
                
                if (!_accountsCache.Contains(record.AccountId))
                {
                    checksOk = false;
                    _logger.LogWarning(
                        $"Invalid account id {record.AccountId} for record with AccountId {record.AccountId}, Instant {record.Instant.ToString(CultureInfo.InvariantCulture)}, Value {record.Value}");
                }

                if (checksOk)
                {
                    _dbContext.Readings.Add(record);
                    continue;
                }

                insertErrorCount++;
            }

            try
            {
                successCount = await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error updating DB: {ex.Message}");
                return BadRequest(ex.Message);
            }

            return Ok($"{successCount}/{_csvParser.LastErrorCount + insertErrorCount}");
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
    }
}