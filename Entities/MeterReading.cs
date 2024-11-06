using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration.Attributes;

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
