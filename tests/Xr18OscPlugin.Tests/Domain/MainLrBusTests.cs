namespace Loupedeck.Xr18OscPlugin.Tests.Domain;

using Loupedeck.Xr18OscPlugin.Domain;
using NSubstitute;
using SharpOSC;

public class MainLrBusTests
{
    private readonly IOscClient _oscClient = Substitute.For<IOscClient>();

    [Fact]
    public void MainLrBus_DefaultName()
    {
        var main = new MainLrBus(_oscClient);
        Assert.Equal("Main LR", main.Name.Value);
    }

    [Fact]
    public void MainLrBus_DefaultIsOn_IsTrue()
    {
        var main = new MainLrBus(_oscClient);
        Assert.True(main.IsOn.Value);
    }

    [Fact]
    public void MainLrBus_DefaultFaderLevel_IsZero()
    {
        var main = new MainLrBus(_oscClient);
        Assert.Equal(0.0f, main.MainFaderLevel.Value);
    }

    [Fact]
    public void MainLrBus_DefaultPan_IsZero()
    {
        var main = new MainLrBus(_oscClient);
        Assert.Equal(0.0f, main.Pan.Value);
    }

    [Fact]
    public void MainLrBus_RegistersCorrectOscAddresses()
    {
        _ = new MainLrBus(_oscClient);

        _oscClient.Received().RegisterHandler("/lr/config/name", Arg.Any<EventHandler<OscMessage>>());
        _oscClient.Received().RegisterHandler("/lr/mix/on", Arg.Any<EventHandler<OscMessage>>());
        _oscClient.Received().RegisterHandler("/lr/mix/fader", Arg.Any<EventHandler<OscMessage>>());
        _oscClient.Received().RegisterHandler("/lr/mix/pan", Arg.Any<EventHandler<OscMessage>>());
    }
}
