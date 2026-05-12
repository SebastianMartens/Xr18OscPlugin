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
    public void Channel_DefaultColor_IsZero()
    {
        var channel = new Channel(_oscClient, 1);
        Assert.Equal(0, channel.Color.Value);
    }

    [Fact]
    public void Channel_Color_RegistersCorrectOscAddress()
    {
        _ = new Channel(_oscClient, 5);
        _oscClientMock.Verify(x => x.RegisterHandler("/ch/05/config/color", It.IsAny<EventHandler<OscMessage>>()), Times.Once);
    }
}
