using System.Runtime.InteropServices.JavaScript;

namespace Amplitude.Net;

public record DeviceInfo(string Platform, string OsName, string OsVersion, string DeviceBrand, string DeviceManufacturer, string DeviceModel, string Carrier);