using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Features.Devices.Extensions;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.Devices;

public class DeviceConnectionStateTests
{
    [Fact]
    public void Should_RemainOnline_UntilOneAndHalfSampleRate()
    {
        var now = DateTimeOffset.UtcNow;

        var state = DeviceExtensions.GetConnectionState(
            DeviceConnectionProtocol.HTTP,
            sampleRateSeconds: 10,
            mqttConnected: true,
            lastSeen: now.AddSeconds(-15),
            nowUtc: now
        );

        state.Should().Be(DeviceConnectionState.Online);
    }

    [Fact]
    public void Should_BeDegraded_AfterOneAndHalfSampleRate()
    {
        var now = DateTimeOffset.UtcNow;

        var state = DeviceExtensions.GetConnectionState(
            DeviceConnectionProtocol.HTTP,
            sampleRateSeconds: 10,
            mqttConnected: true,
            lastSeen: now.AddSeconds(-15.1),
            nowUtc: now
        );

        state.Should().Be(DeviceConnectionState.Degraded);
    }

    [Fact]
    public void Should_RemainDegraded_UntilTwoTimesSampleRate()
    {
        var now = DateTimeOffset.UtcNow;

        var state = DeviceExtensions.GetConnectionState(
            DeviceConnectionProtocol.HTTP,
            sampleRateSeconds: 10,
            mqttConnected: true,
            lastSeen: now.AddSeconds(-20),
            nowUtc: now
        );

        state.Should().Be(DeviceConnectionState.Degraded);
    }

    [Fact]
    public void Should_BeOffline_AfterTwoTimesSampleRate()
    {
        var now = DateTimeOffset.UtcNow;

        var state = DeviceExtensions.GetConnectionState(
            DeviceConnectionProtocol.HTTP,
            sampleRateSeconds: 10,
            mqttConnected: true,
            lastSeen: now.AddSeconds(-20.1),
            nowUtc: now
        );

        state.Should().Be(DeviceConnectionState.Offline);
    }

    [Fact]
    public void Should_NotTreatLatestDataPointAsStale_AtOfflineWindowBoundary()
    {
        var now = DateTimeOffset.UtcNow;

        var isStale = DeviceExtensions.IsLatestDataPointStale(
            DeviceConnectionProtocol.HTTP,
            sampleRateSeconds: 10,
            timestamp: now.AddSeconds(-20),
            nowUtc: now
        );

        isStale.Should().BeFalse();
    }

    [Fact]
    public void Should_TreatLatestDataPointAsStale_AfterOfflineWindow()
    {
        var now = DateTimeOffset.UtcNow;

        var isStale = DeviceExtensions.IsLatestDataPointStale(
            DeviceConnectionProtocol.HTTP,
            sampleRateSeconds: 10,
            timestamp: now.AddSeconds(-20.1),
            nowUtc: now
        );

        isStale.Should().BeTrue();
    }
}
