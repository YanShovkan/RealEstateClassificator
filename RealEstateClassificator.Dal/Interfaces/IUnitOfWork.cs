namespace RealEstateClassificator.Dal.Interfaces;

/// <summary>
/// Единица работы. 
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    ///  Сохранение изменений.
    /// </summary>
    /// <returns></returns>
    public Task CommitAsync(CancellationToken cancellationToken = default);
}

