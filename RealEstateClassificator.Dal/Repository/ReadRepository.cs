using Microsoft.EntityFrameworkCore;
using RealEstateClassificator.Common.Specification;
using RealEstateClassificator.Dal.Interfaces;

namespace RealEstateClassificator.Dal.Repository;

public class ReadRepository<T> : IReadRepository<T> where T : class, IEntity
{
    protected virtual IQueryable<T> Query { get; set; }

    /// <summary>
    /// Создание экземпляра объекта типа <see cref="ReadRepository{T}"/>
    /// </summary>
    /// <param name="context"></param>
    protected ReadRepository(RealEstateClassificatorContext context)
    {
        Query = context.Set<T>();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public async Task<IReadOnlyCollection<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await Query.ToArrayAsync(cancellationToken);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="id"><inheritdoc/></param>
    /// <returns></returns>
    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Query.FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="specification"><inheritdoc/></param>
    /// <returns></returns>
    public async Task<IReadOnlyCollection<T>> GetBySpecification(Specification<T> specification, CancellationToken cancellationToken = default)
        => await Query.Where(specification.Predicate).ToArrayAsync(cancellationToken);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="specification"><inheritdoc/></param>
    /// <returns></returns>
    public async Task<bool> FindBySpecification(Specification<T> specification, CancellationToken cancellationToken = default)
        => await Query.FirstOrDefaultAsync(specification.Predicate, cancellationToken) is null ? false : true;
}
