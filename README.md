# Amplitude.Net

This a dotnet library for making api calls to Amplitude (the analytics service). Currently it supports sending Identify requests only.

## Delivery 
Instances of `IAmplitude` can be injected and IdentifyByUserId/IdentifyByDeviceId called. Despite the Task response requests are dumped to a singleton queue and the method call returns immediately.
This is to stop your code being slowed down by analytics calls.

## Configuration

During program initialisation called `services.AddAmplitude()`.

Add to your configuration a section called `Amplitude` and configure the api key. Eg:
```
{
    "Amplitude": {
        "ApiKey": "somekey"
    }
}
```