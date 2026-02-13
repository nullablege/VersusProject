using Versus.Services;

namespace Versus.Tests.Services
{
    public class AppInfoProviderTests
    {
        [Fact]
        public void GetInfo_Returns_NonEmptyNameAndVersion()
        {
            var provider = new AppInfoProvider();

            var info = provider.GetInfo();

            Assert.False(string.IsNullOrWhiteSpace(info.Name));
            Assert.False(string.IsNullOrWhiteSpace(info.Version));
            Assert.NotEqual(default, info.StartedAtUtc);
        }
    }
}
