namespace Loupedeck.Xr18OscPlugin.Tests.Domain;

using Loupedeck.Xr18OscPlugin.Domain;
using Moq;
using SharpOSC;

public class AuxBusTests
{
    private readonly Mock<IOscClient> _oscClientMock = new();

    [Fact]
    public void AuxBus_Key_FormatsCorrectly()
    {
        var bus = new AuxBus(_oscClientMock.Object, 3);
        Assert.Equal("Aux3", bus.Key);
    }

    [Fact]
    public void AuxBus_Index_IsSet()
    {
        var bus = new AuxBus(_oscClientMock.Object, 5);
        Assert.Equal(5, bus.Index);
    }

    [Fact]
    public void AuxBus_DefaultName()
    {
        var bus = new AuxBus(_oscClientMock.Object, 4);
        Assert.Equal("Bus 4", bus.Name.Value);
    }

    [Fact]
    public void AuxBus_RegistersNameHandler()
    {
        _ = new AuxBus(_oscClientMock.Object, 2);
        _oscClientMock.Verify(x => x.RegisterHandler("/bus/2/config/name", It.IsAny<EventHandler<OscMessage>>()), Times.Once);
    }
}
