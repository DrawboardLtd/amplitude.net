# Amplitude.Net

This a dotnet library for making api calls to Amplitude (the analytics service). Currently it supports sending Identify requests only.

## Delivery 
Instances of `IAmplitude` can be injected.

## Configuration

During program initialisation call `services.AddAmplitude()`.

Add to your configuration a section called `Amplitude` and configure the api key. Eg:
``` json
{
    "Amplitude": {
        "ApiKey": "somekey"
    }
}
```

## Usage

``` csharp
public MyService(IAmplitude amplitude)
{
    _amplitude = amplitude
}

public async Task Identify(string userId)
{
    var userProperties = new Dictionary<string, object> {
        { "favourite", "blue" }
    };

    await _amplitude.ForUserId(userId).Identify(userProperties);
}
```