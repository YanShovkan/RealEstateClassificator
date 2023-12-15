using RealEstateClassificator.Dal.Entities;

namespace RealEstateClassificator.Core.Services.Interfaces;

public interface IPageParserService
{
    void GetRealEstates(IEnumerable<Card> cards);

    IEnumerable<IEnumerable<Card>> GetCardsFromNextPage();
}
