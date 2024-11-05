using System.Text.Json;
using System.Text.Json.Nodes;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;

public class MeterReading
{
    [Ignore]
    public int Id { get; set; }
    
    [Name("AccountId")]
    public int AccountId { get; set; }
    
    [Name("MeterReadingDateTime")]
    public DateTime Instant { get; set; }
    
    [Name("MeterReadValue")]
    //[TypeConverter(typeof(MeterReadingConverter))]
    public int Value { get; set; }

    public override string ToString()
    {
        return $"{AccountId}, {Instant}, {Value}";
    }
}

// public class MeterReadingConverter : DefaultTypeConverter
// {
//     public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
//     {
//         return 
//     }
// }