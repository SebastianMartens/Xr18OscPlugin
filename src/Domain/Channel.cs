namespace Loupedeck.Xr18OscPlugin.Domain;

using global::Xr18OscPlugin.Helpers;
using SharpOSC;

/// <summary>
/// Represents a single channel on the mixer. 
/// For XR18 these are channels 1-18 and the 4 fx return channels.
/// 
/// Capabilities:
/// - Name
/// - On/Off (mute)
/// - Fader level
/// - Bus sends (Aux 1-n)
/// 
/// Not yet implemented:
/// - Panning
/// - EQ
/// - Dynamics
/// - ...
/// </summary>
public class Channel: IChannelBase
{
    private readonly IOscClient _mixer;
    private readonly string _outputMeterAddress;
    private readonly int _meterIndex;

    /// <summary>
    /// Creates a new channel instance for the given mixer and channel index.
    /// </summary>
    /// <param name="mixer"></param>
    /// <param name="index">Channel index 1..18 (1-based)</param>
    public Channel(IOscClient mixer, int index)
    {
        _mixer = mixer;
        Index = index;            
      
        Name = new SyncedValue<string>(_mixer, $"/ch/{Index:00}/config/name", $"Channel {Index:00}");
        IsOn = new SyncedValue<bool>(_mixer, $"/ch/{Index:00}/mix/on", true);
        MainFaderLevel = new SyncedValue<float>(_mixer, $"/ch/{Index:00}/mix/fader", 0.0f);

        // Mixbus sends: Control volume of the channel in the respective bus.
        // Mixbus sends will only work with channels 1..18 and Fx Return (not available for main mix)
        var mixSendFaderLevelAddress = $"/ch/{Index:00}/mix/{{0}}/level";   
        for (var busIndex = 1; busIndex <= Mixer.BusCount; busIndex++)
        {
            BusSendFaderLevels[busIndex - 1] = new SyncedValue<float>(_mixer, string.Format(mixSendFaderLevelAddress, $"{busIndex:00}"), 0.0f);    
        }     
            
        // Meter handling: /meters/1 sends a blob of Int16 values, one per channel
        _outputMeterAddress = "/meters/1";
        _meterIndex = Index - 1;
        _mixer.RegisterHandler(_outputMeterAddress, HandleMeterMessage);
    }

    private void HandleMeterMessage(object? sender, OscMessage message)
    {
        if (message.Arguments.Length == 0 || message.Arguments[0] is not byte[] blob)
            return;

        // Blob contains little-endian Int16 values; each meter value is 2 bytes
        var byteIndex = _meterIndex * 2;
        if (byteIndex + 1 >= blob.Length)
            return;

        var rawValue = BitConverter.ToInt16(blob, byteIndex);
        var meterValue = rawValue / 256.0f;

        MeterValue = meterValue;
        MeterValueUpdated?.Invoke(this, meterValue);
    }

    public int Index { get; }

    public string Key => $"Ch {Index:00}";
        
    public SyncedValue<string> Name { get; }

    public SyncedValue<bool> IsOn { get; }
    
    public SyncedValue<float> MainFaderLevel { get; }

    public SyncedValue<float>[] BusSendFaderLevels = new SyncedValue<float>[6]; // bus index (caution, array index is 0-based!)

    public float MeterValue { get; private set; }

    public event EventHandler<float>? MeterValueUpdated;
}