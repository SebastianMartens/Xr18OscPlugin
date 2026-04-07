namespace Loupedeck.Xr18OscPlugin.Tests.Domain;

using Loupedeck.Xr18OscPlugin.Domain;
using Moq;
using SharpOSC;

public class MainLrBusTests
{
    private readonly Mock<IOscClient> _oscClientMock = new();

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
