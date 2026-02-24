namespace Loupedeck.Xr18OscPlugin.Domain;

using SharpOSC;

/// <summary>
/// Our Mixer class currently has two roles (no SRP ;-)):
/// - It represents the mixer itself and holds the channels, busses, etc.
/// - It also is the main entry point for sending OSC messages to the mixer and receiving OSC messages from the mixer (via the IOscClient).
/// This interface helps keeping responsibilites separated mainly for cleaner usage.
/// </summary>
public interface IOscClient
{
    void RegisterHandler(string address, EventHandler<OscMessage> messageHandler);

    void Send(string address, object? value = null);

    bool IsConnected { get; }

    event EventHandler<bool> IsConnectedChanged;
}