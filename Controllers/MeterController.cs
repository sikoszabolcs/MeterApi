using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace MeterApi.Controllers;

[ApiController]
[Route("meter-reading-uploads")]
public class MeterController(ILogger<MeterController> logger) : Controller
{
    private readonly ILogger<MeterController> _logger = logger;
    private const int MaxFileLengthBytes = 1024;
    private CsvFileParser _csvFileParser = new(MaxFileLengthBytes);
    
    [HttpPost(Name = "PostMeterReadingUploads")]
    public async Task<IActionResult> PostMeterReadingUploads(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is invalid");
        }

        if (!file.FileName.EndsWith(".csv"))
        {
            return BadRequest("File is not .csv");
        }

        if (file.Length > MaxFileLengthBytes)
        {
            return BadRequest("File is too big");
        }

        try
        {
            var result = await _csvFileParser.ParseCsvFile(file.OpenReadStream());
            
            return Ok($"{result.SuccessCount}/{result.FailCount}");
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
        
        
        // 1. Validate uploaded files to prevent malicious uploads.
        // 2. Implement file size restrictions to control the size of uploaded files.
        // 3. Use secure file storage mechanisms to store uploaded files.
        // 4. Consider implementing file type restrictions to allow only specific file types.
        
        
    }
}