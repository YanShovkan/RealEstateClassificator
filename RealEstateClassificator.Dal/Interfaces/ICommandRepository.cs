namespace RealEstateClassificator.Dal.Interfaces;

/// <summary>
/// Репозиторий для работы с данными в БД.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ICommandRepository<T> : IReadRepository<T> where T : class, IEntity
{
    /// <summary>
    /// Создание сущности в БД на основе модели.
    /// </summary>
    /// <param name="model">Модель данных</param>
    public Task CreateAsync(T model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Создание нескольких сущностей в БД на основе моделей данных.
    /// </summary>
    /// <param name="models">Список моделей данных</param>
    public Task CreateManyAsync(IEnumerable<T> models, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновление сущности в БД на основе модели данных.
    /// </summary>
    /// <param name="model">Модель данных</param>
    public void Update(T model);

    /// <summary>
    /// Обновление нескольких сущностей в БД на основе моделей данных.
    /// </summary>
    /// <param name="models">Список моделей данных</param>
    public void UpdateMany(IEnumerable<T> models);

    /// <summary>
    /// Удаление сущности в БД на основе модели данных.
    /// </summary>
    /// <param name="model">Модель данных</param>
    public void Delete(T model);

    /// <summary>
    /// Удаление ескольких сущностей в БД на основе моделей данных.
    /// </summary>
    /// <param name="models">Список моделей данных</param>
    public void DeleteMany(IEnumerable<T> models);
}


