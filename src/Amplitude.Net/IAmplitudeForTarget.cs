namespace Amplitude.Net;

public interface IAmplitudeForTarget
{
    const string UNSET = "UNSET";
    ValueTask Identify(IDictionary<string, object>? userProperties = default, string? appVersion = UNSET, string? language = UNSET, string? paying = UNSET, string? startVersion = UNSET, DeviceInfo? deviceInfo = default, LocationInfo? locationInfo = default);

    ValueTask Event(string eventType, Optional<DateTime> time = default, IDictionary<string, object>? eventProperties = default, IDictionary<string, object>? userProperties = default, Optional<bool> skipUserPropertiesSync = default, Optional<string> appVersion = default,
        DeviceInfo? deviceInfo = default, LocationInfo? locationInfo = default, TransactionInfo? transactionInfo = default,
        Optional<string> ip = default, Optional<decimal> locationLatitude= default, Optional<decimal> locationLongitude= default, AdvertiserIds? advertiserIds = default, Optional<int> eventId = default, Optional<long> sessionId = default, Optional<string> insertId = default, 
        PlanInfo? planInfo = default);
}