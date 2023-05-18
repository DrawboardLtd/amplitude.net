namespace Amplitude.Net;

public interface IAmplitude
{
    const string UNSET = "UNSET";
    
    ValueTask IdentifyByUserId(string userId, IDictionary<string, object>? userProperties = default, string? appVersion = UNSET, string? language = UNSET, string? paying = UNSET, string? startVersion = UNSET, DeviceInfo? deviceInfo = default, LocationInfo? locationInfo = default);
    ValueTask IdentifyByDeviceId(string deviceId, IDictionary<string, object>? userProperties = default, string? appVersion = UNSET, string? language = UNSET, string? paying = UNSET, string? startVersion = UNSET, DeviceInfo? deviceInfo = default, LocationInfo? locationInfo = default);
}