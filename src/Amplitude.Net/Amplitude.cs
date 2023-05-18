using Microsoft.Extensions.Logging;

namespace Amplitude.Net;

/// <summary>
/// Handle for sending requests to Amplitude. Should be a scoped service.
/// </summary>
public class Amplitude : IAmplitude
{
    private readonly ILogger<Amplitude> _logger;
    private readonly IAmplitudeSender _sender;

    public Amplitude(ILogger<Amplitude> logger, IAmplitudeSender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    public Task IdentifyByUserId(string userId, IDictionary<string, object> userProperties = default, string appVersion = IAmplitude.UNSET,
        string language = IAmplitude.UNSET, string paying = IAmplitude.UNSET, string startVersion = IAmplitude.UNSET, DeviceInfo? deviceInfo = default,
        LocationInfo locationInfo = default)
    {
        var payload = new Dictionary<string, object>();
        payload.Add("user_id", userId);
        
        return Identify(payload, userProperties, appVersion, language, paying, startVersion, deviceInfo, locationInfo);
    }

    public Task IdentifyByDeviceId(string deviceId, IDictionary<string, object> userProperties = default, string appVersion = IAmplitude.UNSET,
        string language = IAmplitude.UNSET, string paying = IAmplitude.UNSET, string startVersion = IAmplitude.UNSET, DeviceInfo? deviceInfo = default,
        LocationInfo locationInfo = default)
    {
        var payload = new Dictionary<string, object>();
        payload.Add("device_id", deviceId);

        return Identify(payload, userProperties, appVersion, language, paying, startVersion, deviceInfo, locationInfo);
    }

    private async Task Identify(IDictionary<string, object> payload, IDictionary<string, object> userProperties = default,
        string appVersion = IAmplitude.UNSET,
        string language = IAmplitude.UNSET, string paying = IAmplitude.UNSET, string startVersion = IAmplitude.UNSET, DeviceInfo? deviceInfo = default,
        LocationInfo? locationInfo = default)
    {
        if (userProperties != null)
        {
            payload.Add("user_properties", userProperties);
        }

        if (appVersion != IAmplitude.UNSET)
        {
            payload.Add("app_version", appVersion);
        }
        
        if (language != IAmplitude.UNSET)
        {
            payload.Add("language", language);
        }
        
        if (paying != IAmplitude.UNSET)
        {
            payload.Add("paying", paying);
        }
        
        if (startVersion != IAmplitude.UNSET)
        {
            payload.Add("start_version", startVersion);
        }

        if (deviceInfo != null)
        {
            payload.Add("os_name", deviceInfo.OsName);
            payload.Add("os_version", deviceInfo.OsVersion);
            payload.Add("device_brand", deviceInfo.DeviceBrand);
            payload.Add("device_manufacturer", deviceInfo.DeviceManufacturer);
            payload.Add("device_model", deviceInfo.DeviceModel);
            payload.Add("carrier", deviceInfo.Carrier);
            payload.Add("platform", deviceInfo.Platform);
        }

        if (locationInfo != null)
        {
            payload.Add("country", locationInfo.Country);
            payload.Add("city", locationInfo.City);
            payload.Add("region", locationInfo.Region);
            payload.Add("dma", locationInfo.DMA);
        }
        
        await _sender.Identify(payload, _logger);
    }
}