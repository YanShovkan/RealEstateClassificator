using RealEstateClassificator.Dal.Interfaces;

namespace RealEstateClassificator.Dal.Services;

public class UnitOfWork : IUnitOfWork
{
    private RealEstateClassificatorContext _context;

    /// <summary>
    /// Ctor <see cref="UnitOfWork"/>
    /// </summary>
    /// <param name="context"></param>
    public UnitOfWork(RealEstateClassificatorContext context)
    {
        _context = context;
    }

    public async Task Commit()
    {
        await _context.SaveChangesAsync();
    }
}

