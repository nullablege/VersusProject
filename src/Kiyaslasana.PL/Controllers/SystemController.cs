using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Kiyaslasana.PL.Controllers;

[ApiController]
public sealed class SystemController : ControllerBase
{
    private readonly IAppInfoProvider _appInfoProvider;

    public SystemController(IAppInfoProvider appInfoProvider)
    {
        _appInfoProvider = appInfoProvider;
    }

    [HttpGet("/system/info")]
    public ActionResult<SystemInfoDto> Info()
    {
        return Ok(_appInfoProvider.GetInfo());
    }
}
