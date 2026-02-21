namespace Loupedeck.Xr18OscPlugin.Domain;

using SharpOSC;

public interface IOscClient
{
    void RegisterHandler(string address, EventHandler<OscMessage> messageHandler);

    void Send(string address, object? value = null);

    bool IsConnected { get; }

    event EventHandler<bool> IsConnectedChanged;
}