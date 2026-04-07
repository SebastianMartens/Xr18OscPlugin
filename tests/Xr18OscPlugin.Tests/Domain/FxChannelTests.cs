namespace Loupedeck.Xr18OscPlugin.Tests.Domain;

using Loupedeck.Xr18OscPlugin.Domain;
using NSubstitute;
using SharpOSC;

public class FxChannelTests
{
    private readonly IOscClient _oscClient = Substitute.For<IOscClient>();

    [Fact]
    public void FxChannel_Key_FormatsCorrectly()
    {
        var fx = new FxChannel(_oscClient, 2);
        Assert.Equal("Fx Return 2", fx.Key);
    }

    [Fact]
    public void FxChannel_DefaultName()
    {
        var fx = new FxChannel(_oscClient, 3);
        Assert.Equal("FX 3", fx.Name.Value);
    }

    [Fact]
    public void FxChannel_DefaultIsOn_IsTrue()
    {
        var fx = new FxChannel(_oscClient, 1);
        Assert.True(fx.IsOn.Value);
    }

    [Fact]
    public void FxChannel_RegistersCorrectOscAddresses()
    {
        _ = new FxChannel(_oscClient, 2);

        _oscClient.Received().RegisterHandler("/rtn/2/config/name", Arg.Any<EventHandler<OscMessage>>());
        _oscClient.Received().RegisterHandler("/rtn/2/mix/on", Arg.Any<EventHandler<OscMessage>>());
        _oscClient.Received().RegisterHandler("/rtn/2/mix/fader", Arg.Any<EventHandler<OscMessage>>());
    }

    [Fact]
    public void FxChannel_BusSendFaderLevels_HasSixEntries()
    {
        var fx = new FxChannel(_oscClient, 1);
        Assert.Equal(Mixer.BusCount, fx.BusSendFaderLevels.Length);
        Assert.All(fx.BusSendFaderLevels, sv => Assert.NotNull(sv));
    }

    [Fact]
    public void FxChannel_BusSend_RegistersCorrectAddresses()
    {
        _ = new FxChannel(_oscClient, 1);

        for (var bus = 1; bus <= 6; bus++)
        {
            _oscClient.Received().RegisterHandler($"/rtn/1/mix/{bus:00}/level", Arg.Any<EventHandler<OscMessage>>());
        }
    }
}
