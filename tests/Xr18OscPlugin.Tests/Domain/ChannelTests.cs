namespace Loupedeck.Xr18OscPlugin.Tests.Domain;

using Loupedeck.Xr18OscPlugin.Domain;
using Moq;
using SharpOSC;

public class ChannelTests
{
    private readonly Mock<IOscClient> _oscClientMock = new();
    private IOscClient _oscClient => _oscClientMock.Object;

    [Fact]
    public void Channel_Key_FormatsWithLeadingZero()
    {
        var channel = new Channel(_oscClient, 3);
        Assert.Equal("Ch 03", channel.Key);
    }

    [Fact]
    public void Channel_Index_IsSet()
    {
        var channel = new Channel(_oscClient, 7);
        Assert.Equal(7, channel.Index);
    }

    [Fact]
    public void Channel_DefaultName_MatchesIndex()
    {
        var channel = new Channel(_oscClient, 1);
        Assert.Equal("Channel 01", channel.Name.Value);
    }

    [Fact]
    public void Channel_DefaultIsOn_IsTrue()
    {
        var channel = new Channel(_oscClient, 1);
        Assert.True(channel.IsOn.Value);
    }

    [Fact]
    public void Channel_DefaultFaderLevel_IsZero()
    {
        var channel = new Channel(_oscClient, 1);
        Assert.Equal(0.0f, channel.MainFaderLevel.Value);
    }

    [Fact]
    public void Channel_RegistersCorrectOscAddresses()
    {
        _ = new Channel(_oscClient, 5);

        // Name, IsOn, MainFaderLevel + 6 bus sends = 9 handlers
        _oscClientMock.Verify(x => x.RegisterHandler("/ch/05/config/name", It.IsAny<EventHandler<OscMessage>>()), Times.Once);
        _oscClientMock.Verify(x => x.RegisterHandler("/ch/05/mix/on", It.IsAny<EventHandler<OscMessage>>()), Times.Once);
        _oscClientMock.Verify(x => x.RegisterHandler("/ch/05/mix/fader", It.IsAny<EventHandler<OscMessage>>()), Times.Once);
    }

    [Fact]
    public void Channel_BusSendFaderLevels_HasSixEntries()
    {
        var channel = new Channel(_oscClient, 1);
        Assert.Equal(6, channel.BusSendFaderLevels.Length);
        Assert.All(channel.BusSendFaderLevels, sv => Assert.NotNull(sv));
    }

    [Fact]
    public void Channel_BusSend_RegistersCorrectAddresses()
    {
        _ = new Channel(_oscClient, 2);

        for (var bus = 1; bus <= 6; bus++)
        {
            _oscClientMock.Verify(x => x.RegisterHandler($"/ch/02/mix/{bus:00}/level", It.IsAny<EventHandler<OscMessage>>()), Times.Once);
        }
    }

    [Fact]
    public void Channel_RegistersMeterHandler()
    {
        _ = new Channel(_oscClient, 1);
        _oscClientMock.Verify(x => x.RegisterHandler("/meters/1", It.IsAny<EventHandler<OscMessage>>()), Times.Once);
    }

    [Fact]
    public void Channel_MeterValueUpdated_FiresOnMeterMessage()
    {
        EventHandler<OscMessage>? meterHandler = null;
        _oscClientMock.Setup(x => x.RegisterHandler("/meters/1", It.IsAny<EventHandler<OscMessage>>()))
            .Callback<string, EventHandler<OscMessage>>((_, h) => meterHandler = h);

        var channel = new Channel(_oscClient, 1); // meterIndex = 0

        float receivedValue = 0;
        channel.MeterValueUpdated += (_, v) => receivedValue = v;

        // Build a blob with Int16 little-endian values
        var blob = new byte[36]; // 18 channels * 2 bytes each
        short rawValue = 512; // 512 / 256.0 = 2.0
        BitConverter.GetBytes(rawValue).CopyTo(blob, 0); // index 0 for channel 1

        var message = new OscMessage("/meters/1", [blob]);
        meterHandler!.Invoke(null, message);

        Assert.Equal(2.0f, receivedValue);
        Assert.Equal(2.0f, channel.MeterValue);
    }

    [Fact]
    public void Channel_MeterValue_UsesCorrectIndex()
    {
        EventHandler<OscMessage>? meterHandler = null;
        _oscClientMock.Setup(x => x.RegisterHandler("/meters/1", It.IsAny<EventHandler<OscMessage>>()))
            .Callback<string, EventHandler<OscMessage>>((_, h) => meterHandler = h);

        var channel = new Channel(_oscClient, 5); // meterIndex = 4

        float receivedValue = -1;
        channel.MeterValueUpdated += (_, v) => receivedValue = v;

        var blob = new byte[36];
        short rawValue = -128; // -128 / 256.0 = -0.5
        BitConverter.GetBytes(rawValue).CopyTo(blob, 8); // index 4 * 2 = byte offset 8

        var message = new OscMessage("/meters/1", [blob]);
        meterHandler!.Invoke(null, message);

        Assert.Equal(-0.5f, receivedValue);
    }

    [Fact]
    public void Channel_MeterValue_IgnoresBlobTooSmall()
    {
        EventHandler<OscMessage>? meterHandler = null;
        _oscClientMock.Setup(x => x.RegisterHandler("/meters/1", It.IsAny<EventHandler<OscMessage>>()))
            .Callback<string, EventHandler<OscMessage>>((_, h) => meterHandler = h);

        var channel = new Channel(_oscClient, 10); // meterIndex = 9, needs byte offset 18+

        bool eventFired = false;
        channel.MeterValueUpdated += (_, _) => eventFired = true;

        var blob = new byte[4]; // too small for index 9
        var message = new OscMessage("/meters/1", [blob]);
        meterHandler!.Invoke(null, message);

        Assert.False(eventFired);
        Assert.Equal(0.0f, channel.MeterValue);
    }

    [Fact]
    public void Channel_MeterValue_IgnoresNonBlobArgument()
    {
        EventHandler<OscMessage>? meterHandler = null;
        _oscClientMock.Setup(x => x.RegisterHandler("/meters/1", It.IsAny<EventHandler<OscMessage>>()))
            .Callback<string, EventHandler<OscMessage>>((_, h) => meterHandler = h);

        var channel = new Channel(_oscClient, 1);

        bool eventFired = false;
        channel.MeterValueUpdated += (_, _) => eventFired = true;

        var message = new OscMessage("/meters/1", [42]);
        meterHandler!.Invoke(null, message);

        Assert.False(eventFired);
    }
}
