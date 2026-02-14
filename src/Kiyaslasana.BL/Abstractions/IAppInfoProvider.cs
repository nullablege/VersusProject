using Kiyaslasana.BL.Contracts;

namespace Kiyaslasana.BL.Abstractions;

public interface IAppInfoProvider
{
    SystemInfoDto GetInfo();
}
