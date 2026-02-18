namespace Loupedeck.Xr18OscPlugin.Domain;

using global::Xr18OscPlugin.Domain;


/// <summary>
/// Represents a single channel on the mixer. 
/// For XR18 these are channels 1-16 and the 4 fx return channels.
/// 
/// Capabilities:
/// - Name
/// - On/Off (mute)
/// - Fader level
/// - Bus sends (Aux 1-6)
/// 
/// Not yet implemented:
/// - Metering
/// - Panning
/// - Solo
/// - EQ
/// - Dynamics
/// - Fx Sends
/// - ...
/// </summary>
public class Channel
{
    private readonly Mixer _mixer;
    private readonly string _outputMeterAddress; // not finished, yet
    private readonly int _meterIndex;// not finished, yet
    private readonly int? _meterIndex2;// not finished, yet

    /// <summary>
    /// Creates a new channel instance for the given mixer and channel index.
    /// </summary>
    /// <param name="mixer"></param>
    /// <param name="index">Channel index 1..18 (1-based)</param>
    public Channel(Mixer mixer, int index)
    {
        _mixer = mixer;
        Index = index;
        
        var stereo = false; // TODO: stereo config from mixer settings not yet implemented
      
        Name = new SyncedValue<string>(_mixer, $"/ch/{Index:00}/config/name", "Unknown Channel");
        IsOn = new SyncedValue<bool>(_mixer, $"/ch/{Index:00}/mix/on", true);
        MainFaderLevel = new SyncedValue<float>(_mixer, $"/ch/{Index:00}/mix/fader", 0.0f);

        // Mixbus sends:
        // mixbus sends will only work with channels 1..16 and Fx Return (not available for main mix)
        string mixSendFaderLevelAddress;    
        
        mixSendFaderLevelAddress = $"/ch/{Index:00}/mix/{{0}}/level";   
        for (var busIndex = 1; busIndex <= 6; busIndex++)
        {
            BusSendFaderLevels[busIndex - 1] = new SyncedValue<float>(_mixer, string.Format(mixSendFaderLevelAddress, $"{busIndex:00}"), 0.0f);    
        }     
            
        // TODO: implement meter handling
        _outputMeterAddress = $"/meters/1";
        _meterIndex = Index - 1;
        _meterIndex2 = stereo ? Index : default(int?);
    }    

    public int Index { get; }

    public string Key => $"Ch {Index:00}";
        
    public SyncedValue<string> Name { get; }

    /// <summary>
    /// "On" or "Off" (muted) on main mix
    /// </summary>
    public SyncedValue<bool> IsOn { get; }
    
    public SyncedValue<float> MainFaderLevel { get; }

    public SyncedValue<float>[] BusSendFaderLevels = new SyncedValue<float>[6]; // bus index (caution, array index is 0-based!)
}