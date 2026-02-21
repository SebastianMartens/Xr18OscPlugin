namespace Loupedeck.Xr18OscPlugin.Domain;

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Loupedeck.Xr18OscPlugin;
using SharpOSC;

/// <summary>
/// Represents a Behringer XR18 digital mixer.
/// 
/// - it knows about all sub-components like channels and buses (those reflect the behavior of the mixer)
/// - has only as few own logic as necessary
/// - handles communication with the mixer via OSC (Open Sound Control)
/// </summary>
public class Mixer: IOscClient
{
    /// <summary>
    /// Name of mixer as reported by the mixer itself.
    /// (Can't be changed by us yet)
    /// </summary>
    public string Name { get; private set; } = "Unknown Mixer";

    public string Model { get; private set; } = "Unknown Model";

    public string FirmwareVersion { get; private set; } = "Unknown Version";

    public MainLrBus MainLrBus { get; }

    /// <summary>
    /// Channels represent channel strips (level, compressor, pan, eq etc.) for input channels strips.
    /// </summary>
    public List<Channel> Channels { get; } = new ();

    /// <summary>
    /// Similar to input channels, but control the fx return channels. 
    /// </summary>
    public List<FxChannel> FxChannels { get; } = new ();

    /// <summary>
    /// Busses are used for submixes like monitors or IEMs.
    /// </summary>
    public List<AuxBus> Busses { get; } = new ();

    public Mixer()
    {
        PluginLog.Info("Initializing Mixer domain object...");
        InitConnection().Wait();

        MainLrBus = new MainLrBus(this);        
                
        // Create regular channels 1-16
        // Channels 17/18 are Line Inputs and usually used for USB return but can be 
        // configured as regular channels as well
        for (var channelIndex = 1; channelIndex <= 18; channelIndex++)
        {
            Channels.Add(new Channel(this, channelIndex));
        }

        // Mixbusses Aux1..Aux5
        for (var busIndex = 1; busIndex <= 6; busIndex++)
        {
            Busses.Add(new AuxBus(this, busIndex));
        }

        // Fx return channels
        // Need to be created after busses because they need to know about the busses for the bus sends
        for (var fxIndex = 1; fxIndex <= 4; fxIndex++)
        {
            FxChannels.Add(new FxChannel(this, fxIndex));
        }        
    }

    #region connection Plugin <=> Mixer

    private UdpOscConnection? _udpOscConnection { get; set; }

    public bool IsConnected => _udpOscConnection != null;

    /// <summary>
    /// Event triggered when connection status to mixer changes.
    /// TODO: this is updated correctly on initial connect and on explicist close() but
    /// we don't update periodically or on error (there are not heartbeat messages or similar from the mixer, so we would need to implement that ourselves).
    /// </summary>
    public event EventHandler<bool>? IsConnectedChanged;

    /// <summary>
    /// Timer used to send keep-alive pings to the mixer each few seconds.
    /// Mixer sends updates to clients only if it receives any message from the client periodically.
    /// </summary>
    private Timer? keepAliveTimer;

    /// <summary>
    /// Periodic timer used to try to discover and connect the mixer.
    /// </summary>
    private Timer? tryConnectTimer;

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

    private async Task InitConnection()
    {
        if (IsConnected)
            return;

        var connected = await ReconnectOsc().ConfigureAwait(false);
        if (!connected)
        {
            PluginLog.Info("Failed to connect to mixer. Starting periodic retry...");
            tryConnectTimer = new Timer(async _ => await ReconnectOsc().ConfigureAwait(false), null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }        
    }

    /// <summary>
    /// Reconnects this plugin to the mixer via OSC (Open Sound Control)
    /// We fist trigger a discovery process to find the current IP address.
    /// </summary>
    public async Task<bool> ReconnectOsc(bool forceReconnect = false)
    {
        if (IsConnected && !forceReconnect)
            return true;

        if (string.IsNullOrEmpty(OscRemoteIpAddress) && !await DiscoverMixer().ConfigureAwait(false))
            return false;
            
        CloseConnection();
        _udpOscConnection = new UdpOscConnection(OscRemoteIpAddress, OscRemotePort);

        try
        {
            // Setup keep alive ping to mixer
            // Do this in the background each 7 seconds.            
            keepAliveTimer = new Timer(SendKeepAlivePing, null, 0, 7000);
            void SendKeepAlivePing(object? state)
            {
                _udpOscConnection.Send(new OscMessage("/xremote"));
                IsConnectedChanged?.Invoke(this, true); // fire even if connection status has not changed in order to update plugin status.
            }

            // Setup listener to receive messages from mixer        
            _udpOscConnection.MessageReceived += HandlePacketReceived;
            _udpOscConnection.StartReceiving();

            // if there are already registered handlers, then we should try to get initial values.
            // => Send empty OSC messages to mixer in order to trigger that mixer sends us current values:            
            foreach (var handler in _messageHandlers.ToList())
            {
                Send(handler.Key);
            }            
        }
        catch (Exception e)
        {
            PluginLog.Error($"Failed to connect to mixer at {OscRemoteIpAddress}:{OscRemotePort} - {e.Message}");
            CloseConnection();            
            return false;
        }
        return true;
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

            // Wait for any response (ReceiveTimeout does not apply to ReceiveAsync)
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(0.5));
            var result = await discoveryClient.ReceiveAsync(timeoutCts.Token).ConfigureAwait(false);
            OscRemoteIpAddress = result.RemoteEndPoint.Address.ToString();
            var responseMessage = OscMessage.Deserialize(result.Buffer);
            Name = responseMessage.Arguments[1] as string ?? "Unknown Mixer";
            Model = responseMessage.Arguments[2] as string ?? "Unknown Model";
            FirmwareVersion = responseMessage.Arguments[3] as string ?? "Unknown Version";
            PluginLog.Info($"Successfully discovered mixer '{Name}' at {OscRemoteIpAddress} (Model:{Model}, FW: {FirmwareVersion})");
        }
        catch (OperationCanceledException)
        {
            PluginLog.Info("Mixer discovery timed out.");
            IsConnectedChanged?.Invoke(this, true); // we only fire on failure. Success events will be triggered by the keep-alive ping.
            return false;
        }
        catch (Exception e)
        {
            PluginLog.Error($"Mixer discovery failed: {e.Message}");            
            return false;
        }        

        return true;
    }

    public void CloseConnection()
    {
        keepAliveTimer?.Dispose();
        tryConnectTimer?.Dispose();
        _udpOscConnection?.Dispose();
        _udpOscConnection = null;
        IsConnectedChanged?.Invoke(this, false);
    }


    public void Send(string address, object? value = null)
    {
        if (!IsConnected)
            return;

        try
        {
            if (value == null)
                _udpOscConnection?.Send(new OscMessage(address));
            else
                _udpOscConnection?.Send(new OscMessage(address, value));
        }
        catch (Exception e)
        {
            PluginLog.Error($"Failed to send OSC message to mixer at {OscRemoteIpAddress}:{OscRemotePort} - {e.Message}");
        }
    }

    private readonly ConcurrentDictionary<string, EventHandler<OscMessage>> _messageHandlers = new();

    public void RegisterHandler(string address, EventHandler<OscMessage> messageHandler)
    {
         _messageHandlers.AddOrUpdate(address, messageHandler, (key, existing) => existing + messageHandler);
        
        // send empty message to trigger mixer to send us the current value for this address.        
        if (IsConnected)
        {
            Task.Delay(10).Wait(); // wait a bit. The message handler sometimes is not executed otherwise.
            Send(address);
        }
    }

    public void RemoveHandler(string address, EventHandler<OscMessage> messageHandler) =>
        // Annoyingly, this doesn't actually remove it from the dictionary, even if we end up with a null
        // value.
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
