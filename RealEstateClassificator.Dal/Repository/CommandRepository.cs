using Microsoft.EntityFrameworkCore;
using RealEstateClassificator.Dal.Interfaces;

namespace RealEstateClassificator.Dal.Repository;

/// <summary>
/// <inheritdoc/>
/// </summary>
/// <typeparam name="T"></typeparam>
public class CommandRepository<T> : ReadRepository<T>, ICommandRepository<T> where T : class, IEntity
{
    private readonly DbSet<T> _dbSet;

    /// <summary>
    /// Создание экземпляра объекта типа <see cref="CommandRepository{T}"/>
    /// </summary>
    /// <param name="context"></param>
    public CommandRepository(RealEstateClassificatorContext context) : base(context)
    {
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="models"><inheritdoc/></param>
    /// <param name="cancellationToken"><inheritdoc/></param>
    /// <returns></returns>
    public async Task CreateAsync(T model, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(model, cancellationToken);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="models"><inheritdoc/></param>
    /// <param name="cancellationToken"><inheritdoc/></param>
    /// <returns></returns>
    public Task CreateManyAsync(IEnumerable<T> models, CancellationToken cancellationToken = default)
        => _dbSet.AddRangeAsync(models, cancellationToken);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="model"></param>
    public void Update(T model)
        => _dbSet.Update(model);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="models"></param>
    public void UpdateMany(IEnumerable<T> models)
        => _dbSet.UpdateRange(models);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="model"></param>
    public void Delete(T model)
        => _dbSet.Remove(model);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="models"></param>
    public void DeleteMany(IEnumerable<T> models)
        => _dbSet.RemoveRange(models);
}


