using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.BL.SeoFilters;

public static class SeoFilterRegistry
{
    private static readonly IReadOnlyDictionary<string, SeoFilterDefinition> Filters =
        new Dictionary<string, SeoFilterDefinition>(StringComparer.Ordinal)
        {
            ["5g-telefonlar"] = new SeoFilterDefinition(
                Slug: "5g-telefonlar",
                Title: "5G Telefonlar",
                MetaDescription: "5G destekli telefon modellerini sayfali listede kesfedin.",
                Predicate: x =>
                    x.NetworkTeknolojisi != null
                    && (x.NetworkTeknolojisi.Contains("5G")
                        || x.NetworkTeknolojisi.Contains("5g")
                        || x.NetworkTeknolojisi.Contains("NR"))),

            ["8gb-ram-telefonlar"] = new SeoFilterDefinition(
                Slug: "8gb-ram-telefonlar",
                Title: "8GB RAM Telefonlar",
                MetaDescription: "8GB RAM sunan telefon modellerini tek bir SEO uyumlu sayfada inceleyin.",
                Predicate: x =>
                    x.DahiliHafiza != null
                    && (x.DahiliHafiza.Contains("8GB")
                        || x.DahiliHafiza.Contains("8 GB")
                        || x.DahiliHafiza.Contains("8gb")
                        || x.DahiliHafiza.Contains("8 gb"))),

            ["5000mah-batarya-telefonlar"] = new SeoFilterDefinition(
                Slug: "5000mah-batarya-telefonlar",
                Title: "5000mAh Batarya Telefonlar",
                MetaDescription: "5000mAh batarya kapasitesine sahip telefonlari listeleyin ve karsilastirin.",
                Predicate: x =>
                    (x.BataryaTipi != null
                     && (x.BataryaTipi.Contains("5000mAh")
                         || x.BataryaTipi.Contains("5000 mAh")
                         || x.BataryaTipi.Contains("5000mah")
                         || x.BataryaTipi.Contains("5000 mah")))
                    || (x.BataryaDiger != null
                        && (x.BataryaDiger.Contains("5000mAh")
                            || x.BataryaDiger.Contains("5000 mAh")
                            || x.BataryaDiger.Contains("5000mah")
                            || x.BataryaDiger.Contains("5000 mah")))),

            ["120hz-ekran-telefonlar"] = new SeoFilterDefinition(
                Slug: "120hz-ekran-telefonlar",
                Title: "120Hz Ekran Telefonlar",
                MetaDescription: "120Hz ekran tazeleme hizina sahip telefon modellerini kesfedin.",
                Predicate: x =>
                    (x.EkranDigerOzellikler != null
                     && (x.EkranDigerOzellikler.Contains("120Hz")
                         || x.EkranDigerOzellikler.Contains("120 Hz")
                         || x.EkranDigerOzellikler.Contains("120hz")
                         || x.EkranDigerOzellikler.Contains("120 hz")))
                    || (x.EkranTipi != null
                        && (x.EkranTipi.Contains("120Hz")
                            || x.EkranTipi.Contains("120 Hz")
                            || x.EkranTipi.Contains("120hz")
                            || x.EkranTipi.Contains("120 hz"))))
        };

    public static bool TryGet(string? slug, out SeoFilterDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            definition = default!;
            return false;
        }

        return Filters.TryGetValue(slug.Trim().ToLowerInvariant(), out definition!);
    }

    public static IReadOnlyList<SeoFilterDefinition> GetAll()
    {
        return Filters.Values.ToArray();
    }
}
