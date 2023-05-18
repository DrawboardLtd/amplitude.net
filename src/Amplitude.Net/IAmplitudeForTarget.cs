namespace Amplitude.Net;

public interface IAmplitudeForTarget
{
    const string UNSET = "UNSET";
    ValueTask Identify(IDictionary<string, object>? userProperties = default, string? appVersion = UNSET, string? language = UNSET, string? paying = UNSET, string? startVersion = UNSET, DeviceInfo? deviceInfo = default, LocationInfo? locationInfo = default);
}