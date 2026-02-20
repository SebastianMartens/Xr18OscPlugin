namespace Loupedeck.Xr18OscPlugin.Domain;

using System.Threading.Tasks;

using SharpOSC;

public interface IOscClient
{
    void RegisterHandler(string address, EventHandler<OscMessage> messageHandler);

    Task Send(string address, object? value = null);
}