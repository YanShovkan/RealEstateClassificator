using RealEstateClassificator.Common.Specification;

namespace RealEstateClassificator.Dal.Interfaces;

/// <summary>
/// Репозиторий для получения запросов к данным из БД.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IReadRepository<T> where T : class, IEntity
{
    /// <summary>
    /// Получить список всех сущностей.
    /// </summary>
    /// <returns></returns>
    public Task<IReadOnlyCollection<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить сущность с определнноым Id.
    /// </summary>
    /// <param name="id">Идентификатор сущности</param>
    /// <returns></returns>
    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить запрос для сущности по определнной спецификации.
    /// </summary>
    /// <param name="specification">Спецификация</param>
    /// <returns></returns>
    public Task<IReadOnlyCollection<T>> GetBySpecification(Specification<T> specification, CancellationToken cancellationToken = default);
}

