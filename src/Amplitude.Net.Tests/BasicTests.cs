using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bogus;
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
        var counters = new Counters();
        var httpClient = new HttpClient(new MockHttpMessageHandler(counters));
        var httpFactory = Substitute.For<IHttpClientFactory>();
        httpFactory.CreateClient().Returns(_ => httpClient);

        var sender = new AsyncAmplitudeSender(httpFactory, _optionsWrapper, Substitute.For<ILogger<AsyncAmplitudeSender>>());
        var amplitude = new Amplitude(Substitute.For<ILogger<Amplitude>>(), sender);

        var userId = Guid.NewGuid().ToString("N");
        await amplitude.ForUserId(userId)
            .Identify(new Dictionary<string, object>
            {
                {"first_name", "Billy Bob"}
            });

        await Task.Delay(TimeSpan.FromMilliseconds(1000));
        
        counters.Calls.ShouldBe(1);
        counters.Successes.ShouldBe(1);
    }
    
    [Fact]
    public async Task AssertEventRequestSent()
    {
        var counters = new Counters();
        var httpClient = new HttpClient(new MockHttpMessageHandler(counters));
        var httpFactory = Substitute.For<IHttpClientFactory>();
        httpFactory.CreateClient().Returns(_ => httpClient);

        var sender = new AsyncAmplitudeSender(httpFactory, _optionsWrapper, Substitute.For<ILogger<AsyncAmplitudeSender>>());
        var amplitude = new Amplitude(Substitute.For<ILogger<Amplitude>>(), sender);

        var userId = Guid.NewGuid().ToString("N");
        await amplitude.ForUserId(userId)
            .Event("touched_sky", DateTime.UtcNow, new Dictionary<string, object>
            {
                {"first_name", "Billy Bob"}
            });

        await Task.Delay(TimeSpan.FromMilliseconds(1000));
        
        counters.Calls.ShouldBe(1);
        counters.Successes.ShouldBe(1);
    }
    
    [Fact]
    public async Task AssertEventRequestSentUnderLoad()
    {
        var counters = new Counters();
        var httpFactory = Substitute.For<IHttpClientFactory>();
        httpFactory.CreateClient().Returns(_ => new HttpClient(new MockHttpMessageHandler(counters)));

        var sender = new AsyncAmplitudeSender(httpFactory, _optionsWrapper, Substitute.For<ILogger<AsyncAmplitudeSender>>());
       
        await Parallel.ForEachAsync(Enumerable.Range(0, 100), async (i, token) =>
        {
            var amplitude = new Amplitude(Substitute.For<ILogger<Amplitude>>(), sender);
            var faker = new Faker();
            var userId = Guid.NewGuid().ToString("N");
            await amplitude.ForUserId(userId)
                .Event("touched_sky",  DateTime.UtcNow, new Dictionary<string, object>
                {
                    {"color", "blue"}
                });
        });

        await Task.Delay(TimeSpan.FromMilliseconds(5000));
        
        //5-6 ?? Theres some variance that happens with timing. Test is subject to the fluctuations of IO.
        counters.Calls.ShouldBeInRange(5, 6);
        counters.Successes.ShouldBeInRange(5, 6);
    }
    
    [Fact]
    public async Task AssertIdentifyRequestSentUnderLoad()
    {
        var counters = new Counters();
        var httpFactory = Substitute.For<IHttpClientFactory>();
        httpFactory.CreateClient().Returns(_ => new HttpClient(new MockHttpMessageHandler(counters)));

        var sender = new AsyncAmplitudeSender(httpFactory, _optionsWrapper, Substitute.For<ILogger<AsyncAmplitudeSender>>());

        await Parallel.ForEachAsync(Enumerable.Range(0, 100), async (i, token) =>
        {
            var amplitude = new Amplitude(Substitute.For<ILogger<Amplitude>>(), sender);
            var faker = new Faker();
            var userId = Guid.NewGuid().ToString("N");
            await amplitude.ForUserId(userId)
                .Identify(new Dictionary<string, object>
                {
                    {"first_name", faker.Person.FirstName},
                    {"last_name", faker.Person.LastName},
                });
        });

        await Task.Delay(TimeSpan.FromMilliseconds(5000));
        
        //we have 100 requests and we deliver at most 20 per batch
        counters.Calls.ShouldBeInRange(5, 6);
        counters.Successes.ShouldBeInRange(5, 6);
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