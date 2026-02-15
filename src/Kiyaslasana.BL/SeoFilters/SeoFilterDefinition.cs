using System.Linq.Expressions;
using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.BL.SeoFilters;

// TODO: Expression predicates currently couple this model to EF translation rules.
// Consider a provider-agnostic filter model if data providers are expanded later.
public sealed record SeoFilterDefinition(
    string Slug,
    string Title,
    string MetaDescription,
    Expression<Func<Telefon, bool>> Predicate);
