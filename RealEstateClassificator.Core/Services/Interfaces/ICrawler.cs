using RealEstateClassificator.Dal.Entities;

namespace RealEstateClassificator.Core.Services.Interfaces;

public interface ICrawler
{
    IAsyncEnumerable<IEnumerable<Card>> GetCardsFromNextPageOrUrls();
}
