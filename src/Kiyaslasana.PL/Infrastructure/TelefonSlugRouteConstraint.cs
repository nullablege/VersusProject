using System.Text.RegularExpressions;
using Kiyaslasana.EL.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Kiyaslasana.PL.Infrastructure;

public sealed class TelefonSlugRouteConstraint : IRouteConstraint
{
    private static readonly Regex SlugRegex = new(
        $"^[a-z0-9-]{{1,{TelefonConstraints.SlugMaxLength}}}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public bool Match(
        HttpContext? httpContext,
        IRouter? route,
        string routeKey,
        RouteValueDictionary values,
        RouteDirection routeDirection)
    {
        if (!values.TryGetValue(routeKey, out var rawValue))
        {
            return false;
        }

        var slug = Convert.ToString(rawValue);
        return slug is not null && SlugRegex.IsMatch(slug);
    }
}
