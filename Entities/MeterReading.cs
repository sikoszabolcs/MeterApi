using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;

[Table("readings")]
public class MeterReading
{
    [Ignore]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }
    
    [Name("AccountId")]
    [Column("account_id")]
    public int AccountId { get; set; }
    
    [Name("MeterReadingDateTime")]
    //[TypeConverter(typeof(TestConverter))]
    [Column("meter_reading_date_time")]
    public DateTime Instant { get; set; }
    
    [Name("MeterReadValue")]
    [Column("meter_read_value")]
    public int Value { get; set; }

    public override string ToString()
    {
        return $"{AccountId}, {Instant}, {Value}";
    }
}

// /// <summary>
// /// 
// /// </summary>
// public class TestConverter : DateTimeConverter
// {
//     public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
//     {
//         Convert.ToDateTime()
//     }
// }
