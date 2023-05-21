using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Amplitude.Net.Tests;

public class BasicTests
{
    private readonly IOptions<AmplitudeOptions> _optionsWrapper;

    public BasicTests()
    {
        ConfigurationBuilder configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("appsettings.json", optional: true);
        configBuilder.AddEnvironmentVariables("tests_");
        var configuration = configBuilder.Build();
        
        var ampConfigSection = configuration.GetSection(AmplitudeOptions.Section);
        _optionsWrapper = Substitute.For<IOptions<AmplitudeOptions>>();
        _optionsWrapper.Value.Returns(ampConfigSection.Get<AmplitudeOptions>());
    }

    [Fact]
    public async Task AssertIdentifyRequestSent()
    {
        var httpClient = new HttpClient();
        var httpFactory = Substitute.For<IHttpClientFactory>();
        httpFactory.CreateClient().Returns(_ => httpClient);

        var sender = new AsyncAmplitudeSender(httpFactory, _optionsWrapper);
        var amplitude = new Amplitude(Substitute.For<ILogger<Amplitude>>(), sender);

        var userId = Guid.NewGuid().ToString("N");
        await amplitude.ForUserId(userId)
            .Identify(new Dictionary<string, object>
            {
                {"first_name", "Billy Bob"}
            });

        await Task.Delay(TimeSpan.FromMilliseconds(1000));
        
        httpFactory.Received().CreateClient();
    }

    [Fact]
    public void AssertOptionalAsMethodArgDefaultBehaves()
    {
        void Test(Optional<bool> arg = default)
        {
            arg.HasValue.ShouldBeFalse();
        }

        Test();
    }
}
