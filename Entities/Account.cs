using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeterApi.Entities;

[Table("accounts")]
public class Account
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;
    
    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;
}