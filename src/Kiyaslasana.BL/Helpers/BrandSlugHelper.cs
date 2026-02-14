using System.Globalization;
using System.Text;

namespace Kiyaslasana.BL.Helpers;

public static class BrandSlugHelper
{
    public static string ToSlug(string? brand)
    {
        if (string.IsNullOrWhiteSpace(brand))
        {
            return string.Empty;
        }

        var normalizedInput = ReplaceTurkishChars(brand.Trim().ToLowerInvariant())
            .Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder(normalizedInput.Length);
        var previousDash = false;

        foreach (var ch in normalizedInput)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(ch);
                previousDash = false;
                continue;
            }

            if (previousDash)
            {
                continue;
            }

            builder.Append('-');
            previousDash = true;
        }

        return builder
            .ToString()
            .Trim('-')
            .ToLowerInvariant();
    }

    private static string ReplaceTurkishChars(string input)
    {
        return input
            .Replace('ı', 'i')
            .Replace('ğ', 'g')
            .Replace('ü', 'u')
            .Replace('ş', 's')
            .Replace('ö', 'o')
            .Replace('ç', 'c');
    }
}
