namespace Amplitude.Net;

public record DeviceInfo(Optional<string?> Platform = default, Optional<string?> OsName = default, Optional<string?> OsVersion = default, Optional<string?> DeviceBrand = default, Optional<string?> DeviceManufacturer = default, Optional<string?> DeviceModel = default, Optional<string?> Carrier = default);