namespace SharpOSC;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using Loupedeck.Xr18OscPlugin;

/// <summary>
/// Class created by Sebastian Martens
/// (having two different classes for sending and listening was not working well with XAir products who 
/// send back to sender).
/// </summary>
/// <param name="remoteAddress"></param>
/// <param name="remotePort"></param>
/// <param name="localPort"></param>
public class UdpOscConnection(string remoteAddress, int remotePort, int localPort = 0) : IDisposable
{
    private readonly UdpClient _udpClient = new(localPort);
    private readonly IPEndPoint _remoteEndPoint = new(IPAddress.Parse(remoteAddress), remotePort);
    
    private bool isListening;

    public event Action<IOscPacket>? MessageReceived;

    public void Send(OscMessage message)
    {        
        var data = message.Serialize();        
        try
        {
            _udpClient.Send(data, data.Length, _remoteEndPoint);            
        }
        catch (SocketException)
        {
            PluginLog.Error($"SocketException occurred while sending OSC data. Data could not be sent to {message.Address}");
        }
        
    }

    public void StartReceiving()
    {
        if (isListening) 
            return;
            
        isListening = true;
        Task.Run(async () =>
        {
            while (isListening)
            {
                try
                {
                    var result = await _udpClient.ReceiveAsync().ConfigureAwait(false);
                    var packet = IOscPacket.Deserialize(result.Buffer);
                    MessageReceived?.Invoke(packet);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException)
                {
                    PluginLog.Error($"SocketException occurred while receiving OSC data.");
                }
                catch (Exception ex)
                {
                    PluginLog.Error($"Unexpected error in OSC receive loop: {ex.Message}");
                }
            }
        });
    }

    public void StopReceiving() => isListening = false;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        isListening = false;
        _udpClient?.Close();
        _udpClient?.Dispose();        
    }
    
}