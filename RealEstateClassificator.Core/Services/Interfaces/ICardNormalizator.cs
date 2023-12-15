using RealEstateClassificator.Dal.Entities;

namespace RealEstateClassificator.Core.Services.Interfaces;

public interface ICardNormalizator
{
    void Normalize(Card card);
}
