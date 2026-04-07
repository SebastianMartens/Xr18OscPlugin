namespace Loupedeck.Xr18OscPlugin.Tests.Domain;

using Loupedeck.Xr18OscPlugin.Domain;
using NSubstitute;
using SharpOSC;
using global::Xr18OscPlugin.Domain;

public class SyncedValueTests
{
    private readonly IOscClient _oscClient = Substitute.For<IOscClient>();

    [Fact]
    public void Constructor_SetsDefaultValue()
    {
        var synced = new SyncedValue<float>(_oscClient, "/test/fader", 0.5f);
        Assert.Equal(0.5f, synced.Value);
    }

    [Fact]
    public void Constructor_RegistersHandler()
    {
        _ = new SyncedValue<string>(_oscClient, "/test/name", "default");
        _oscClient.Received(1).RegisterHandler("/test/name", Arg.Any<EventHandler<OscMessage>>());
    }

    [Fact]
    public void Set_Float_SendsOscMessage()
    {
        var synced = new SyncedValue<float>(_oscClient, "/ch/01/mix/fader", 0.0f);
        synced.Set(0.75f);
        _oscClient.Received(1).Send("/ch/01/mix/fader", 0.75f);
    }

    [Fact]
    public void Set_Bool_True_SendsInt1()
    {
        var synced = new SyncedValue<bool>(_oscClient, "/ch/01/mix/on", false);
        synced.Set(true);
        _oscClient.Received(1).Send("/ch/01/mix/on", 1);
    }

    [Fact]
    public void Set_Bool_False_SendsInt0()
    {
        var synced = new SyncedValue<bool>(_oscClient, "/ch/01/mix/on", true);
        synced.Set(false);
        _oscClient.Received(1).Send("/ch/01/mix/on", 0);
    }

    [Fact]
    public void OnValueChanged_Float_UpdatesValue()
    {
        var synced = new SyncedValue<float>(_oscClient, "/test/fader", 0.0f);
        var handler = CaptureHandler("/test/fader");

        handler.Invoke(null, new OscMessage("/test/fader", 0.85f));

        Assert.Equal(0.85f, synced.Value);
    }

    [Fact]
    public void OnValueChanged_Float_RaisesEvent()
    {
        var synced = new SyncedValue<float>(_oscClient, "/test/fader", 0.0f);
        var handler = CaptureHandler("/test/fader");

        float? received = null;
        synced.ValueChanged += (_, v) => received = v;

        handler.Invoke(null, new OscMessage("/test/fader", 0.5f));

        Assert.Equal(0.5f, received);
    }

    [Fact]
    public void OnValueChanged_Bool_FromInt()
    {
        var synced = new SyncedValue<bool>(_oscClient, "/test/on", false);
        var handler = CaptureHandler("/test/on");

        handler.Invoke(null, new OscMessage("/test/on", 1));
        Assert.True(synced.Value);

        handler.Invoke(null, new OscMessage("/test/on", 0));
        Assert.False(synced.Value);
    }

    [Fact]
    public void OnValueChanged_String_UpdatesValue()
    {
        var synced = new SyncedValue<string>(_oscClient, "/test/name", "Default");
        var handler = CaptureHandler("/test/name");

        handler.Invoke(null, new OscMessage("/test/name", "Vocals"));

        Assert.Equal("Vocals", synced.Value);
    }

    [Fact]
    public void OnValueChanged_String_EmptyFallsBackToDefault()
    {
        var synced = new SyncedValue<string>(_oscClient, "/test/name", "Default Name");
        var handler = CaptureHandler("/test/name");

        handler.Invoke(null, new OscMessage("/test/name", ""));

        Assert.Equal("Default Name", synced.Value);
    }

    private EventHandler<OscMessage> CaptureHandler(string address)
    {
        var handler = default(EventHandler<OscMessage>);
        _oscClient.When(x => x.RegisterHandler(address, Arg.Any<EventHandler<OscMessage>>()))
            .Do(ci => handler = ci.ArgAt<EventHandler<OscMessage>>(1));

        // Re-create to capture the handler (constructor calls RegisterHandler)
        // We already have one from the test setup, so we need to get it.
        // Actually, let's just grab it from the received calls:
        var calls = _oscClient.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(IOscClient.RegisterHandler))
            .Where(c => (string)c.GetArguments()[0]! == address)
            .ToList();

        return (EventHandler<OscMessage>)calls.Last().GetArguments()[1]!;
    }
}
