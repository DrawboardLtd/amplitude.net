using Microsoft.Extensions.Logging;

namespace Amplitude.Net;

public abstract class AmplitudeFor: IAmplitudeForTarget
{
    private readonly ILogger _logger;
    private readonly IAmplitudeSender _amplitudeSender;
    private readonly KeyValuePair<string, object?> _identifier;

    protected AmplitudeFor(ILogger logger, IAmplitudeSender amplitudeSender, KeyValuePair<string, object> identifier)
    {
        _logger = logger;
        _amplitudeSender = amplitudeSender;
        _identifier = identifier!;
    }
    
    public async ValueTask Identify(IDictionary<string, object>? userProperties = default,
        string? appVersion = IAmplitude.UNSET,
        string? language = IAmplitude.UNSET, string? paying = IAmplitude.UNSET, string? startVersion = IAmplitude.UNSET, DeviceInfo? deviceInfo = default,
        LocationInfo? locationInfo = default)
    {
        var payload = new Dictionary<string, object?>(new [] {_identifier});
        
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
            if (string.IsNullOrEmpty(paying) || paying == "none")
            {
                payload.Add("paying", paying);
            }
            else
            {
                throw new InvalidDataException($"The value {paying} provided for 'paying' is invalid");
            }
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
        
        await _amplitudeSender.Identify(payload, _logger);
    }
}