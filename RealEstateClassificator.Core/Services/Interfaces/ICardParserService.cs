using RealEstateClassificator.Dal.Entities;

namespace RealEstateClassificator.Core.Services.Interfaces;

public interface ICardParserService
{
    void ParseRealEstates(IEnumerable<Card> cards);

}
