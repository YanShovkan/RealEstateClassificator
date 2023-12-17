namespace RealEstateClassificator.Common.ValueObjects;

public static class RangeSplitHelper
{
    public static IEnumerable<PriceRange> StepRanges(PriceRange range, long rangeCount, long? pick = default) =>
        Ranges(Points(range.From, range.To, rangeCount, pick).ToArray());

    private static IEnumerable<long> Points(long start, long end, long rangeCount, long? pick = default)
    {
        long next = start;

        if (pick == null)
        {
            var linearStep = ((end - start) / rangeCount) + 1;
            do
            {
                yield return next;
                next += linearStep;
            }
            while (next < end);

            yield return end;
            yield break;
        }

        var rangeCountHalf = rangeCount / 2;
        var multiplier = Math.Pow(pick.Value - start, 1d / rangeCountHalf);

        for (int i = 1; i <= rangeCountHalf; i++)
        {
            yield return next;
            next = pick.Value - (long)Math.Pow(multiplier, rangeCountHalf - i);
        }

        next = pick.Value;
        var step = ((end - pick.Value) / (rangeCount - rangeCountHalf)) + 1;

        do
        {
            yield return next;
            next += step;
        }
        while (next < end);

        yield return end;
    }

    private static IEnumerable<PriceRange> Ranges(IEnumerable<long> points)
    {
        return points.Zip(points.Skip(1)).Select(_ => new PriceRange(_.First, _.Second));
    }
}

