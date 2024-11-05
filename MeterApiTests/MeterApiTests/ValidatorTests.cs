using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace MeterApiTests;

public class ValidatorTests
{
    [Fact]
    public async void LoadSameEntry_Twice()
    {
        var fileStream = File.Open("TestData/Readings.csv", FileMode.Open);
        int errors = 0;
        int valid = 0;
        using (var reader = new StreamReader(fileStream))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
               {
                   HasHeaderRecord = true,
                   ReadingExceptionOccurred = args =>
                   {
                       errors += 1;
                       return false;
                   }
               }))
        {
            try
            {
                await foreach (var record in csv.GetRecordsAsync<MeterReading>())
                {
                    valid += 1;
                    Console.Out.WriteLine(record);
                }
            }
            catch (Exception e)
            {
                //errors += 1;
                
                //throw;
            }
            /*while (await csv.ReadAsync())
            {
                var record = new MeterReading
                {
                    AccountId = csv.GetField<int>("AccountId"),
                    Instant = csv.GetField<int>("Age"),
                    Value = csv.GetField<string>("Value")
                };
            }*/
        }
        
        Assert.Equal(4, errors);
        Assert.Equal(31, valid);
    }

    [Fact]
    public void MeterReadingMustHaveAssociatedAccountId()
    {
        
    }

    [Fact]
    public void ReadingValuesFormat_ShouldBe_NNNNN()
    {
        
    }
}