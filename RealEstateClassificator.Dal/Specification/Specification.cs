using System.Linq.Expressions;

namespace RealEstateClassificator.Dal.Specification;
public abstract class Specification<T>
{
    public Expression<Func<T, bool>> Predicate { get; }

    public Specification(Expression<Func<T, bool>> predicate)
    {
        Predicate = predicate;
    }
}

