namespace Loupedeck.Xr18OscPlugin.Domain;

using global::Xr18OscPlugin.Domain;


/// <summary>
/// Represents a single FX return channel on the mixer. 
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
public class FxChannel
{
    private readonly Mixer _mixer;

    private readonly string _outputMeterAddress; // not finished, yet

    private readonly int _meterIndex;// not finished, yet
    private readonly int? _meterIndex2;// not finished, yet

    public int Index { get; }

    public FxChannel(Mixer mixer, int index)
    {         
        _mixer = mixer;
        Index = index;
        
        Name = new SyncedValue<string>(_mixer, $"/rtn/{Index}/config/name", "Unknown Channel");
        IsOn = new SyncedValue<bool>(_mixer, $"/rtn/{Index}/mix/on", true);
        MainFaderLevel = new SyncedValue<float>(_mixer, $"/rtn/{Index}/mix/fader", 0.0f);

        // Mixbus sends:
        // mixbus sends will only work with channels 1..16 and Fx Return (not available for main mix)
        string mixSendFaderLevelAddress;
        // if (nameAddress.Contains("/ch/"))
        // {
        //     mixSendFaderLevelAddress = $"/ch/{index}/mix/{{0}}/level";   
        //     for (var busIndex = 1; busIndex <= 6; busIndex++)
        //     {
        //         BusSendFaderLevels[busIndex - 1] = new SyncedValue<float>(_mixer, string.Format(mixSendFaderLevelAddress, $"{busIndex:00}"), 0.0f);    
        //     }     
        // }
        //else // it's a fx return channel
        //{
            // TODO: index here is "rtn1".."rtn4", but we need the fxIndex (1..4) for the address => fix namings!
            // TODO: test
            mixSendFaderLevelAddress = $"/rtn/{index}/mix/{{0}}/level";
            // TODO: create separate fader levels collecion similar to BusSendFaderLevels or reuse the same?
        //}
            
        // TODO: implement meter handling
        _outputMeterAddress = $"/meters/3";
        _meterIndex = 4 + Index;
        _meterIndex2 = null;
    }    

    public string Key => $"rtn{Index}"; // TODO unify format? Behringer sometimes uses "01" and sometimes "1"
        
    public SyncedValue<string> Name { get; }

    /// <summary>
    /// "On" or "Off" (muted) on main mix
    /// </summary>
    public SyncedValue<bool> IsOn { get; }
    
    public SyncedValue<float> MainFaderLevel { get; }

    public SyncedValue<float>[] BusSendFaderLevels = new SyncedValue<float>[6]; // bus index (caution, array index is 0-based!)
}