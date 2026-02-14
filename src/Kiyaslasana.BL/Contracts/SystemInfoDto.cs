namespace Kiyaslasana.BL.Contracts;

public sealed record SystemInfoDto(string Name, string Version, DateTimeOffset StartedAtUtc);
