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