using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Kiyaslasana.PL.Infrastructure;

public sealed class CspNonceViewDataFilter : IAsyncResultFilter
{
    public const string CspNonceItemKey = "CspNonce";

    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ViewResult viewResult &&
            context.HttpContext.Items.TryGetValue(CspNonceItemKey, out var nonceValue) &&
            nonceValue is string nonce &&
            !string.IsNullOrWhiteSpace(nonce))
        {
            viewResult.ViewData[CspNonceItemKey] = nonce;
        }

        return next();
    }
}
