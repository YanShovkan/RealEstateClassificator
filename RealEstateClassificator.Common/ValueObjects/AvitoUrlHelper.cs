using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace RealEstateClassificator.Common.ValueObjects;
public static class AvitoUrlHelper
{
    private static readonly byte[] CommonHeaderSuffix = { 0x01, 0x45, 0xc6, 0x9a, 0x0c };

    public static string GenerateUrl(PriceRange range, string startUrl)
    {
        var (from, to) = range;

        var filter = $"{{\"from\":{from},\"to\":{to}}}";
        var array = new byte[] { 0x01, 0x28, 0x01, 0x01, 0x01, 0x02, 0x01, 0x44, 0x92, 0x03, 0xc6, 0x10, 0x01, 0x40, 0xe6, 0x07, 0x14, 0x8c, 0x52 }
            .Concat(CommonHeaderSuffix)
            .Concat(new[] { (byte)filter.Length })
            .Concat(Encoding.Default.GetBytes(filter))
            .ToArray();
        return startUrl + "&f=" + Convert.ToBase64String(array).TrimEnd('=').Replace('/', '~');
    }

    public static PriceRange GetRangeFromUrl(string url)
    {
        var query = new Uri(url).Query;
        var filterBase64 = PadBase64(HttpUtility.ParseQueryString(query).Get("f")!).Replace('~', '/');

        var filter = Encoding.Default.GetString(Convert.FromBase64String(filterBase64));

        var pattern = @"\{""from"":(\d+),""to"":(\d+)\}";
        var matches = Regex.Matches(filter, pattern);

        var from = long.Parse(matches.First().Groups[1].Value);
        var to = long.Parse(matches.First().Groups[2].Value);

        return new(from, to);
    }

    private static string PadBase64(string value)
    {
        static int TotalLength(int l)
        {
            return l + ((4 - (l % 4)) % 4);
        }

        return value.PadRight(TotalLength(value.Length), '=');
    }
}