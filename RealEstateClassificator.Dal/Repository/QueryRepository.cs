using Microsoft.EntityFrameworkCore;
using RealEstateClassificator.Dal.Interfaces;

namespace RealEstateClassificator.Dal.Repository;

/// <summary>
/// <inheritdoc/>
/// </summary>
/// <typeparam name="T"></typeparam>
public class QueryRepository<T> : ReadRepository<T>, IQueryRepository<T> where T : class, IEntity
{
    protected override IQueryable<T> Query { get; set; }

    /// <summary>
    /// Создание экземпляра объекта типа <see cref="QueryRepository{T}"/>
    /// </summary>
    /// <param name="context"></param>
    public QueryRepository(RealEstateClassificatorContext context) : base(context)
    {
        Query = context.Set<T>().AsNoTracking().IgnoreAutoIncludes();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public IQueryable<T> GetQuery()
        => Query;
}

