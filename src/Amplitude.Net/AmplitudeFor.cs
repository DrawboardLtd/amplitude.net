using Microsoft.Extensions.Logging;

namespace Amplitude.Net;

#if (NETSTANDARD2_0)
using ReturnType = Task;
#else
using ReturnType = ValueTask;
#endif

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
    
    public async ReturnType Identify(IDictionary<string, object>? userProperties = default,
        Optional<string?> appVersion = default,
        Optional<string?> language = default, Optional<string?> paying = default, Optional<string?> startVersion = default, DeviceInfo? deviceInfo = default,
        LocationInfo? locationInfo = default)
    {
        var payload = new Dictionary<string, object?>
        {
            {_identifier.Key, _identifier.Value}
        };
        
        void WriteOptional<T>(Optional<T> value, string key)
        {
            if (value.HasValue)
            {
                payload.Add(key, value.Value);
            }
        }
        
        if (userProperties != null)
        {
            payload.Add("user_properties", userProperties);
        }
        
        WriteOptional(appVersion, "app_version");
        WriteOptional(language, "language");
        

        if (paying.HasValue)
        {
            if (string.IsNullOrEmpty(paying.Value) || paying.Value == "none")
            {
                payload.Add("paying", paying.Value);
            }
            else
            {
                throw new InvalidDataException($"The value {paying.Value} provided for 'paying' is invalid");
            }
        }
        
        WriteOptional(startVersion, "start_version");
        
        if (deviceInfo != null)
        {
            payload.Add("os_name", deviceInfo.OsName.Value);
            payload.Add("os_version", deviceInfo.OsVersion.Value);
            payload.Add("device_brand", deviceInfo.DeviceBrand.Value);
            payload.Add("device_manufacturer", deviceInfo.DeviceManufacturer.Value);
            payload.Add("device_model", deviceInfo.DeviceModel.Value);
            payload.Add("carrier", deviceInfo.Carrier.Value);
            payload.Add("platform", deviceInfo.Platform.Value);
        }

        if (locationInfo != null)
        {
            payload.Add("country", locationInfo.Country.Value);
            payload.Add("city", locationInfo.City.Value);
            payload.Add("region", locationInfo.Region.Value);
            payload.Add("dma", locationInfo.DMA.Value);
        }
        
        await _amplitudeSender.Identify(payload, _logger);
    }

    public async ReturnType Event(string eventType, Optional<DateTime> time = default, IDictionary<string, object>? eventProperties = default,
        IDictionary<string, object>? userProperties = default, Optional<bool> skipUserPropertiesSync = default, Optional<string> appVersion = default,
        DeviceInfo? deviceInfo = default, LocationInfo? locationInfo = default, TransactionInfo? transactionInfo = default,
        Optional<string> ip = default, Optional<decimal> locationLatitude = default, Optional<decimal> locationLongitude = default, 
        AdvertiserIds? advertiserIds = default, Optional<int> eventId = default, Optional<long> sessionId = default, Optional<string> insertId = default, 
        PlanInfo? planInfo = default)
    {
        var payload = new Dictionary<string, object?>
        {
            {_identifier.Key, _identifier.Value},
            {"event_type", eventType}
        };
        
        void WriteOptional<T>(Optional<T> value, string key)
        {
            if (value.HasValue)
            {
                payload.Add(key, value.Value);
            }
        }
        
        WriteOptional(skipUserPropertiesSync, "$skip_user_properties_sync");
        WriteOptional(appVersion, "app_version");
        WriteOptional(locationLatitude, "location_lat");
        WriteOptional(locationLongitude, "location_lng");
        WriteOptional(eventId, "event_id");
        WriteOptional(sessionId, "session_id");
        WriteOptional(insertId, "insert_id");
        
        //TODO: generate event id and insert id if not provided?
        
        if (time.HasValue)
        {
            payload.Add("time", time.Value.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
        }
        
        if (eventProperties != null)
        {
            payload.Add("event_properties", eventProperties);
        }
        
        if (userProperties != null)
        {
            payload.Add("user_properties", userProperties);
        }

        if (deviceInfo != null)
        {
            WriteOptional(deviceInfo.DeviceBrand, "device_brand");
            WriteOptional(deviceInfo.DeviceManufacturer, "device_manufacturer");
            WriteOptional(deviceInfo.DeviceModel, "device_model");
            WriteOptional(deviceInfo.Carrier, "carrier");
            WriteOptional(deviceInfo.Platform, "platform");
            WriteOptional(deviceInfo.OsName, "os_name");
            WriteOptional(deviceInfo.OsVersion, "os_version");
        }

        if (locationInfo != null)
        {
            WriteOptional(locationInfo.City, "city");
            WriteOptional(locationInfo.Country, "country");
            WriteOptional(locationInfo.Region, "region");
            WriteOptional(locationInfo.DMA, "dma");
        }

        if (transactionInfo != null)
        {
            WriteOptional(transactionInfo.Price, "price");
            WriteOptional(transactionInfo.Quantity, "quantity");
            WriteOptional(transactionInfo.Revenue, "revenue");
            WriteOptional(transactionInfo.ProductId, "productId");
            WriteOptional(transactionInfo.Revenue, "revenueType");
        }
        
        if (advertiserIds != null)
        {
            WriteOptional(advertiserIds.AdId, "adid");
            WriteOptional(advertiserIds.AndroidId, "android_id");
            WriteOptional(advertiserIds.IdFa, "idfa");
            WriteOptional(advertiserIds.IdFv, "idfv");
        }

        if (planInfo != null)
        {
            payload.Add("plan",
                new
                {
                    branch = planInfo.Branch.ValueOrDefault, 
                    source = planInfo.Source.ValueOrDefault,
                    version = planInfo.Version.ValueOrDefault
                });
        }
        
        await _amplitudeSender.Event(payload, _logger);
    }
}