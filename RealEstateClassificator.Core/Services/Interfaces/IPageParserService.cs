using RealEstateClassificator.Dal.Entities;

namespace RealEstateClassificator.Core.Services.Interfaces;

public interface IPageParserService
{
    IEnumerable<IEnumerable<Card>> GetCardsFromNextPage();
}
