using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Contracts;
using Kiyaslasana.PL.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Kiyaslasana.Tests.Controllers;

public class SystemControllerTests
{
    [Fact]
    public void Info_Returns_Ok_WithSystemInfo()
    {
        var expected = new SystemInfoDto("Kiyaslasana", "1.0.0", DateTimeOffset.UtcNow);
        var controller = new SystemController(new StubAppInfoProvider(expected));

        var result = controller.Info().Result;

        var ok = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<SystemInfoDto>(ok.Value);
        Assert.Equal(expected, value);
    }

    private sealed class StubAppInfoProvider : IAppInfoProvider
    {
        private readonly SystemInfoDto _info;

        public StubAppInfoProvider(SystemInfoDto info)
        {
            _info = info;
        }

        public SystemInfoDto GetInfo()
        {
            return _info;
        }
    }
}
