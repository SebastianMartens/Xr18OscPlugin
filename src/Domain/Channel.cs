namespace Loupedeck.Xr18OscPlugin.Domain;

using global::Xr18OscPlugin.Domain;

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
/// - Metering
/// - Panning
/// - Solo
/// - EQ
/// - Dynamics
/// - Fx Sends
/// - ...
/// </summary>
public class Channel: IChannelBase
{
    private readonly IOscClient _mixer;
    // private readonly string _outputMeterAddress; // not finished, yet
    // private readonly int _meterIndex;// not finished, yet
    // private readonly int? _meterIndex2;// not finished, yet

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
        Color = new SyncedValue<int>(_mixer, $"/ch/{Index:00}/config/color", 0);

        // Mixbus sends: Control volume of the channel in the respective bus.
        // Mixbus sends will only work with channels 1..18 and Fx Return (not available for main mix)
        var mixSendFaderLevelAddress = $"/ch/{Index:00}/mix/{{0}}/level";   
        for (var busIndex = 1; busIndex <= Mixer.BusCount; busIndex++)
        {
            BusSendFaderLevels[busIndex - 1] = new SyncedValue<float>(_mixer, string.Format(mixSendFaderLevelAddress, $"{busIndex:00}"), 0.0f);    
        }     
            
        // TODO: implement meter handling
        // _outputMeterAddress = $"/meters/1";
        // _meterIndex = Index - 1;
        // _meterIndex2 = stereo ? Index : default(int?);
    }    

    public int Index { get; }

    public string Key => $"Ch {Index:00}";
        
    public SyncedValue<string> Name { get; }

    public SyncedValue<bool> IsOn { get; }
    
    public SyncedValue<float> MainFaderLevel { get; }

    /// <summary>
    /// Channel color as configured in the mixer UI. Integer value 0–15:
    /// 0=OFF, 1=RD, 2=GN, 3=YE, 4=BL, 5=MG, 6=CY, 7=WH,
    /// 8=OFFi, 9=RDi, 10=GNi, 11=YEi, 12=BLi, 13=MGi, 14=CYi, 15=WHi
    /// (where "i" = inverted/bright variant).
    /// </summary>
    public SyncedValue<int> Color { get; }

    public SyncedValue<float>[] BusSendFaderLevels = new SyncedValue<float>[6]; // bus index (caution, array index is 0-based!)
}