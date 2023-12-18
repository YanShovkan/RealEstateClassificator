using Microsoft.EntityFrameworkCore;

namespace RealEstateClassificator.Dal;

public class RealEstateClassificatorContext : DbContext
{
    public RealEstateClassificatorContext(DbContextOptions dbContextOptions) : base(dbContextOptions) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
