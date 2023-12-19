using RealEstateClassificator.Dal.Entities;

namespace RealEstateClassificator.Dal.Specification;

public class CardUrlSpecification : Specification<Card>
{
    public CardUrlSpecification(string url) : base(_ => _.Url.Equals(url)) { }
}
