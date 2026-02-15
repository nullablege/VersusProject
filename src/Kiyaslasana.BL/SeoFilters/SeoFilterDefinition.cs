using System.Linq.Expressions;
using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.BL.SeoFilters;

public sealed record SeoFilterDefinition(
    string Slug,
    string Title,
    string MetaDescription,
    Expression<Func<Telefon, bool>> Predicate);
