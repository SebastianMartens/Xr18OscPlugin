namespace Loupedeck.Xr18OscPlugin.Tests.Domain;

using Loupedeck.Xr18OscPlugin.Domain;
using Moq;
using SharpOSC;

public class MainLrBusTests
{
    private readonly Mock<IOscClient> _oscClientMock = new();

    [Fact]
    public void MainLrBus_DefaultName()
    {
        var main = new MainLrBus(_oscClientMock.Object);
        Assert.Equal("Main LR", main.Name.Value);
    }

    [Fact]
    public void MainLrBus_DefaultIsOn_IsTrue()
    {
        var main = new MainLrBus(_oscClientMock.Object);
        Assert.True(main.IsOn.Value);
    }

    [Fact]
    public void MainLrBus_DefaultFaderLevel_IsZero()
    {
        var main = new MainLrBus(_oscClientMock.Object);
        Assert.Equal(0.0f, main.MainFaderLevel.Value);
    }

    [Fact]
    public void MainLrBus_DefaultPan_IsZero()
    {
        var main = new MainLrBus(_oscClientMock.Object);
        Assert.Equal(0.0f, main.Pan.Value);
    }

    [Fact]
    public void MainLrBus_RegistersCorrectOscAddresses()
    {
        _ = new MainLrBus(_oscClientMock.Object);

        _oscClientMock.Verify(x => x.RegisterHandler("/lr/config/name", It.IsAny<EventHandler<OscMessage>>()), Times.Once);
        _oscClientMock.Verify(x => x.RegisterHandler("/lr/mix/on", It.IsAny<EventHandler<OscMessage>>()), Times.Once);
        _oscClientMock.Verify(x => x.RegisterHandler("/lr/mix/fader", It.IsAny<EventHandler<OscMessage>>()), Times.Once);
        _oscClientMock.Verify(x => x.RegisterHandler("/lr/mix/pan", It.IsAny<EventHandler<OscMessage>>()), Times.Once);
    }
}
