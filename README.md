# Amplitude.Net

This a dotnet library for making api calls to Amplitude (the analytics service). Currently it supports sending Identify requests and Events.

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

## Maintained by Drawboard

[Drawboard PDF](https://www.drawboard.com/pdf/pdf) is a [feature-rich](https://www.drawboard.com/pdf/product-tour) PDF annotation and editing software that is designed to enhance productivity and collaboration.
Users can annotate on shared PDFs in real time, making it ideal for team projects or remote collaboration.

With Markup tools that can cater to the everyday user, as well as highly technical teams, [Drawboard PDF](https://www.drawboard.com/pdf/pdf)’s intuitive interface is easy to learn and turns beginners into experts.