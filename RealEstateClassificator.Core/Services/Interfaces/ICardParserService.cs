using RealEstateClassificator.Dal.Entities;

namespace RealEstateClassificator.Core.Services.Interfaces;

public interface ICardParserService
{
    Task ParseRealEstatesAsync(IEnumerable<Card> cards, CancellationToken cancellationToken = default);
}
