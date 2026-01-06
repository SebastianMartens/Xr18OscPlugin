namespace Xr18OscPlugin.Domain;

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using Loupedeck;
using Loupedeck.Xr18OscPlugin;

using SharpOSC;

/// <summary>
/// Represents a Behringer XR18 digital mixer.
/// 
/// - it knows about all sub-components like channels and buses (those reflect the behavior of the mixer)
/// - has only as few own logic as necessary
/// - handles communication with the mixer via OSC (Open Sound Control)
/// </summary>
public class Mixer
{
    /// <summary>
    /// Name of mixer as reported by the mixer itself.
    /// (Can't be changed by us yet)
    /// </summary>
    public string Name { get; private set; } = "Unknown Mixer";

    public string Model { get; private set; } = "Unknown Model";

    public string FirmwareVersion { get; private set; } = "Unknown Version";

    public MixerChannels MixerChannels { get; }

    public Mixer()
    {
        PluginLog.Info("Initializing Mixer domain object...");
        MixerChannels = new MixerChannels(this);
        
        // Impl. of buses is pending
        //InitBuses();
    }

    // /// <summary>
    // /// Busses are used for submixes like monitors or IEMs.
    // /// </summary>
    // public List<MixerBus> Buses { get; set; } = [];

    // private void InitBuses()
    // {
    //     Buses = [
    //         new(this) { BusNumber = 1 },
    //         new(this) { BusNumber = 2 },
    //         new(this) { BusNumber = 3 },
    //         new(this) { BusNumber = 4 },
    //         new(this) { BusNumber = 5 },
    //         new(this) { BusNumber = 6 }
    //     ];
    // }
    

    #region connection Plugin <=> Mixer

    public UdpOscConnection? UdpOscConnection { get; private set; }

    /// <summary>
    /// Timer used to send keep-alive pings to the mixer each few seconds.
    /// </summary>
    private Timer? timer;

    /// <summary>
    /// IP address of the mixer.
    /// We avoid to store a fixed IP address here and try to discover the mixer
    /// by sending out a UDP braodcast message (see below).
    /// But if needed, you can use the "Connect" action to set a fixed IP address.
    /// </summary>
    public string OscRemoteIpAddress { get; set; } = string.Empty;

    /// <summary>
    /// OSC port used for communication.
    /// Behringer XAir prodcuts per default listen on port 10024.
    /// Behringer X32 Series listen on port 10023.
    /// </summary>
    private int OscRemotePort { get; } = 10024;
    
    /// <summary>
    /// Reconnects this plugin to the mixer via OSC (Open Sound Control)
    /// We fist trigger a discovery process to find the current IP address.
    /// </summary>
    public async Task ReconnectOsc()
    {
        if (string.IsNullOrEmpty(OscRemoteIpAddress) && !await DiscoverMixer().ConfigureAwait(false))
            return;
            
        UdpOscConnection?.Dispose();
        UdpOscConnection = new UdpOscConnection(OscRemoteIpAddress, OscRemotePort);

        try
        {
            // Setup keep alive ping to mixer
            // Do this in the background each 7 seconds.
            timer = new Timer(SendKeepAlivePing, null, 0, 7000);
            void SendKeepAlivePing(object? state) => UdpOscConnection.Send(new OscMessage("/xremote"));

            // Setup listener to receive messages from mixer        
            UdpOscConnection.MessageReceived += HandlePacketReceived;
            UdpOscConnection.StartReceiving();
        }
        catch (Exception e)
        {
            PluginLog.Error($"Failed to connect to mixer at {OscRemoteIpAddress}:{OscRemotePort} - {e.Message}");
        }
    }

    private async Task<bool> DiscoverMixer()
    {
        try
        {
            // Send broadcast message to find the mixer and get back some info in the same step:
            using var discoveryClient = new UdpClient { EnableBroadcast = true };
            var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, OscRemotePort);
            var data = new OscMessage("/xinfo").Serialize();            
            await discoveryClient.SendAsync(data, data.Length, broadcastEndpoint).ConfigureAwait(false);
            discoveryClient.Client.ReceiveTimeout = 2000;

            // Wait for any response
            var result = await discoveryClient.ReceiveAsync().ConfigureAwait(false);
            var remoteIp = result.RemoteEndPoint.Address.ToString();
            var responseMessage = OscMessage.Deserialize(result.Buffer);
            Name = responseMessage.Arguments[1] as string ?? "Unknown Mixer";
            Model = responseMessage.Arguments[2] as string ?? "Unknown Model";
            FirmwareVersion = responseMessage.Arguments[3] as string ?? "Unknown Version";
            PluginLog.Info($"Successfully discovered mixer '{Name}' at {remoteIp} (Model:{Model}, FW: {FirmwareVersion})");

            OscRemoteIpAddress = remoteIp;
        }
        catch (Exception e)
        {
            PluginLog.Error($"Mixer discovery failed: {e.Message}");            
            return false;
        }        

        return true;
    }

    public void Close()
    {
        timer?.Dispose();
        UdpOscConnection?.Dispose();
    }


    public async Task Send(string address, object? value = null)
    {
        if (UdpOscConnection == null) 
            await ReconnectOsc().ConfigureAwait(false);

        try
        {
            if (value == null)
                UdpOscConnection?.Send(new OscMessage(address));
            else
                UdpOscConnection?.Send(new OscMessage(address, value));
        }
        catch (Exception e)
        {
            PluginLog.Error($"Failed to send OSC message to mixer at {OscRemoteIpAddress}:{OscRemotePort} - {e.Message}");
        }
    }

    private readonly ConcurrentDictionary<string, EventHandler<OscMessage>> _messageHandlers = new();

    public void RegisterHandler(string address, EventHandler<OscMessage> messageHandler) =>
        _messageHandlers.AddOrUpdate(address, messageHandler, (key, existing) => existing + messageHandler);

    public void RemoveHandler(string address, EventHandler<OscMessage> messageHandler) =>
        // Annoyingly, this doesn't actually remove it from the dictionary, even if we end up with a null
        // value. That's not the end of the world; it's just a bit irritating.
        _messageHandlers.AddOrUpdate(address, messageHandler, (key, existing) => existing - messageHandler ?? existing);


    private void HandlePacketReceived(IOscPacket packet)
    {        
        if (packet is OscMessage message)
        {
            if (_messageHandlers.TryGetValue(message.Address, out var handler) && handler is object)
            {
                handler.Invoke(this, message);
            }
        }
        else if (packet is OscBundle bundle)
        {
            foreach (var innerMessage in bundle.Messages)
            {
                HandlePacketReceived(innerMessage);
            }
        }
    }

    #endregion
}