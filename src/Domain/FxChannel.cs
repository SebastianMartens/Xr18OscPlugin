namespace Loupedeck.Xr18OscPlugin.Domain;

using global::Xr18OscPlugin.Domain;

/// <summary>
/// Represents a single FX return channel on the mixer. 
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
public class FxChannel: IChannelBase
{
    private readonly IOscClient _mixer;

    // private readonly string _outputMeterAddress; // not finished, yet
    // private readonly int _meterIndex;// not finished, yet
    // private readonly int? _meterIndex2;// not finished, yet

    public int Index { get; }

    public FxChannel(IOscClient mixer, int index)
    {         
        _mixer = mixer;
        Index = index;
        
        Name = new SyncedValue<string>(_mixer, $"/rtn/{Index}/config/name", $"FX {Index}");
        IsOn = new SyncedValue<bool>(_mixer, $"/rtn/{Index}/mix/on", true);
        MainFaderLevel = new SyncedValue<float>(_mixer, $"/rtn/{Index}/mix/fader", 0.0f);

        // Mixbus sends
        // index is without leading 0 here! Channel index WITH leading 0!
        var mixSendFaderLevelAddress = $"/rtn/{Index}/mix/{{0}}/level";
        for (var busIndex = 1; busIndex <= Mixer.BusCount; busIndex++)
        {
            BusSendFaderLevels[busIndex - 1] = new SyncedValue<float>(_mixer, string.Format(mixSendFaderLevelAddress, $"{busIndex:00}"), 0.0f);    
        }
    
        // TODO: implement meter handling
        // _outputMeterAddress = $"/meters/3";
        // _meterIndex = 4 + Index;
        // _meterIndex2 = null;
    }    

    public string Key => $"Fx Return {Index}";
        
    public SyncedValue<string> Name { get; }

    /// <summary>
    /// "On" or "Off" (muted) on main mix
    /// </summary>
    public SyncedValue<bool> IsOn { get; }
    
    public SyncedValue<float> MainFaderLevel { get; }

    public SyncedValue<float>[] BusSendFaderLevels = new SyncedValue<float>[Mixer.BusCount]; // bus index (caution, array index is 0-based!)
}