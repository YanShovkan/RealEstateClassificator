using Microsoft.EntityFrameworkCore;

namespace RealEstateClassificator.Dal;

public class RealEstateClassificatorContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured == false)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=valutnikDB;Username=postgres;Password=123");
        }
        base.OnConfiguring(optionsBuilder);
    }
}
