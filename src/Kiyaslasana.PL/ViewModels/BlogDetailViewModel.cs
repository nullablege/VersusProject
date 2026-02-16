using Kiyaslasana.BL.Contracts;
using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.PL.ViewModels;

public sealed class BlogDetailViewModel
{
    public required BlogPost Post { get; init; }

    public required string BlogPostingJsonLd { get; init; }

    public required IReadOnlyList<BlogInternalLink> InternalLinks { get; init; }
}
