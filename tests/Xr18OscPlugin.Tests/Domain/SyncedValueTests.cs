namespace Loupedeck.Xr18OscPlugin.Tests.Domain;

using Loupedeck.Xr18OscPlugin.Domain;
using Moq;
using SharpOSC;
using global::Xr18OscPlugin.Domain;

public class SyncedValueTests
{
    private readonly Mock<IOscClient> _oscClientMock = new();
    private EventHandler<OscMessage>? _capturedHandler;

    public SyncedValueTests()
    {
        // Capture the handler passed to RegisterHandler
        _oscClientMock
            .Setup(x => x.RegisterHandler(It.IsAny<string>(), It.IsAny<EventHandler<OscMessage>>()))
            .Callback<string, EventHandler<OscMessage>>((_, handler) => _capturedHandler = handler);
    }

    [Fact]
    public void Constructor_SetsDefaultValue()
    {
        var synced = new SyncedValue<float>(_oscClientMock.Object, "/test/fader", 0.5f);
        Assert.Equal(0.5f, synced.Value);
    }

    [Fact]
    public void Constructor_RegistersHandler()
    {
        _ = new SyncedValue<string>(_oscClientMock.Object, "/test/name", "default");
        _oscClientMock.Verify(x => x.RegisterHandler("/test/name", It.IsAny<EventHandler<OscMessage>>()), Times.Once);
    }

    [Fact]
    public void Set_Float_SendsOscMessage()
    {
        var synced = new SyncedValue<float>(_oscClientMock.Object, "/ch/01/mix/fader", 0.0f);
        synced.Set(0.75f);
        _oscClientMock.Verify(x => x.Send("/ch/01/mix/fader", 0.75f), Times.Once);
    }

    [Fact]
    public void Set_Bool_True_SendsInt1()
    {
        var synced = new SyncedValue<bool>(_oscClientMock.Object, "/ch/01/mix/on", false);
        synced.Set(true);
        _oscClientMock.Verify(x => x.Send("/ch/01/mix/on", (object)1), Times.Once);
    }

    [Fact]
    public void Set_Bool_False_SendsInt0()
    {
        var synced = new SyncedValue<bool>(_oscClientMock.Object, "/ch/01/mix/on", true);
        synced.Set(false);
        _oscClientMock.Verify(x => x.Send("/ch/01/mix/on", (object)0), Times.Once);
    }

    [Fact]
    public void OnValueChanged_Float_UpdatesValue()
    {
        var synced = new SyncedValue<float>(_oscClientMock.Object, "/test/fader", 0.0f);
        _capturedHandler!.Invoke(null, new OscMessage("/test/fader", 0.85f));
        Assert.Equal(0.85f, synced.Value);
    }

    [Fact]
    public void OnValueChanged_Float_RaisesEvent()
    {
        var synced = new SyncedValue<float>(_oscClientMock.Object, "/test/fader", 0.0f);

        float? received = null;
        synced.ValueChanged += (_, v) => received = v;

        _capturedHandler!.Invoke(null, new OscMessage("/test/fader", 0.5f));
        Assert.Equal(0.5f, received);
    }

    [Fact]
    public void OnValueChanged_Bool_FromInt()
    {
        var synced = new SyncedValue<bool>(_oscClientMock.Object, "/test/on", false);

        _capturedHandler!.Invoke(null, new OscMessage("/test/on", 1));
        Assert.True(synced.Value);

        _capturedHandler!.Invoke(null, new OscMessage("/test/on", 0));
        Assert.False(synced.Value);
    }

    [Fact]
    public void OnValueChanged_String_UpdatesValue()
    {
        var synced = new SyncedValue<string>(_oscClientMock.Object, "/test/name", "Default");
        _capturedHandler!.Invoke(null, new OscMessage("/test/name", "Vocals"));
        Assert.Equal("Vocals", synced.Value);
    }

    [Fact]
    public void OnValueChanged_String_EmptyFallsBackToDefault()
    {
        var synced = new SyncedValue<string>(_oscClientMock.Object, "/test/name", "Default Name");
        _capturedHandler!.Invoke(null, new OscMessage("/test/name", ""));
        Assert.Equal("Default Name", synced.Value);
    }
}
