using Kiyaslasana.BL.Helpers;

namespace Kiyaslasana.Tests.Helpers;

public class BrandSlugHelperTests
{
    [Theory]
    [InlineData("  Samsung   Galaxy  ", "samsung-galaxy")]
    [InlineData("Sony/PlayStation", "sony-playstation")]
    [InlineData("Xiaomi@@@Pro", "xiaomi-pro")]
    public void ToSlug_NormalizesSpacesAndSymbols(string input, string expected)
    {
        var slug = BrandSlugHelper.ToSlug(input);

        Assert.Equal(expected, slug);
    }

    [Theory]
    [InlineData("İşık", "isik")]
    [InlineData("Çığ Özü Şölen", "cig-ozu-solen")]
    [InlineData("  Türkçe Marka  ", "turkce-marka")]
    public void ToSlug_NormalizesTurkishCharacters(string input, string expected)
    {
        var slug = BrandSlugHelper.ToSlug(input);

        Assert.Equal(expected, slug);
    }
}
