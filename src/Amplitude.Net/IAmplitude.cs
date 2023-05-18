namespace Amplitude.Net;

public interface IAmplitude
{
    const string UNSET = "UNSET";
    
    Task IdentifyByUserId(string userId, IDictionary<string, object> userProperties = default, string appVersion = UNSET, string language = UNSET, string paying = UNSET, string startVersion = UNSET, DeviceInfo? deviceInfo = default, LocationInfo locationInfo = default);
    Task IdentifyByDeviceId(string deviceId, IDictionary<string, object> userProperties = default, string appVersion = UNSET, string language = UNSET, string paying = UNSET, string startVersion = UNSET, DeviceInfo? deviceInfo = default, LocationInfo locationInfo = default);
}