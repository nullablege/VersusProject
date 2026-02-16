using System.Globalization;
using System.Text;

namespace Kiyaslasana.BL.Helpers;

public static class SlugHelper
{
    public static string ToSlug(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var normalizedInput = ReplaceTurkishChars(input.Trim().ToLowerInvariant())
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

        return builder.ToString().Trim('-');
    }

    private static string ReplaceTurkishChars(string input)
    {
        return input
            .Replace('\u0131', 'i')
            .Replace('\u011f', 'g')
            .Replace('\u00fc', 'u')
            .Replace('\u015f', 's')
            .Replace('\u00f6', 'o')
            .Replace('\u00e7', 'c')
            .Replace('\u0130', 'i')
            .Replace('\u011e', 'g')
            .Replace('\u00dc', 'u')
            .Replace('\u015e', 's')
            .Replace('\u00d6', 'o')
            .Replace('\u00c7', 'c');
    }
}
