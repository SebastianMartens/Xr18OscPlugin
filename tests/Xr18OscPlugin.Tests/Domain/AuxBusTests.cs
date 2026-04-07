namespace Loupedeck.Xr18OscPlugin.Tests.Domain;

using Loupedeck.Xr18OscPlugin.Domain;
using NSubstitute;
using SharpOSC;

public class AuxBusTests
{
    private readonly IOscClient _oscClient = Substitute.For<IOscClient>();

    [Fact]
    public void AuxBus_Key_FormatsCorrectly()
    {
        var bus = new AuxBus(_oscClient, 3);
        Assert.Equal("Aux3", bus.Key);
    }

    [Fact]
    public void AuxBus_Index_IsSet()
    {
        var bus = new AuxBus(_oscClient, 5);
        Assert.Equal(5, bus.Index);
    }

    [Fact]
    public void AuxBus_DefaultName()
    {
        var bus = new AuxBus(_oscClient, 4);
        Assert.Equal("Bus 4", bus.Name.Value);
    }

    [Fact]
    public void AuxBus_RegistersNameHandler()
    {
        _ = new AuxBus(_oscClient, 2);
        _oscClient.Received().RegisterHandler("/bus/2/config/name", Arg.Any<EventHandler<OscMessage>>());
    }
}
