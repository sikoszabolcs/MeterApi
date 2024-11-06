using MeterApi.Entities;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext()
    {
        
    }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    
    public virtual DbSet<MeterReading> Readings { get; set; }
    
    public virtual DbSet<Account> Accounts { get; set; }
}

public class Reading
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public DateTime MeterReadingDateTime { get; set; }
    public int MeterReadValue { get; set; }
}