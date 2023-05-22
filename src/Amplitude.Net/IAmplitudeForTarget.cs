namespace Amplitude.Net;

#if (NETSTANDARD2_0)
using ReturnType = Task;
#else
using ReturnType = ValueTask;
#endif

public interface IAmplitudeForTarget
{
    ReturnType Identify(IDictionary<string, object>? userProperties = default, Optional<string?> appVersion = default, Optional<string?> language = default, Optional<string?> paying = default, Optional<string?> startVersion = default, DeviceInfo? deviceInfo = default, LocationInfo? locationInfo = default);

    ReturnType Event(string eventType, Optional<DateTime> time = default, IDictionary<string, object>? eventProperties = default, IDictionary<string, object>? userProperties = default, Optional<bool> skipUserPropertiesSync = default, Optional<string> appVersion = default,
        DeviceInfo? deviceInfo = default, LocationInfo? locationInfo = default, TransactionInfo? transactionInfo = default,
        Optional<string> ip = default, Optional<decimal> locationLatitude= default, Optional<decimal> locationLongitude= default, AdvertiserIds? advertiserIds = default, Optional<int> eventId = default, Optional<long> sessionId = default, Optional<string> insertId = default, 
        PlanInfo? planInfo = default);
}