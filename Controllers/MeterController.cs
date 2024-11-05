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
                    _dbContext.Readings.Add(record);

                    try
                    {
                        var updatedRows = await _dbContext.SaveChangesAsync();
                        successCount += updatedRows;
                    }
                    catch (DbUpdateException ex)
                    {
                        failCount++;
                    }
                }
                else
                {
                    failCount++;
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