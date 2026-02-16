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

    public Channel(
            Mixer mixer, string index, string nameAddress, string faderLevelAddress,
            string outputMeterAddress, int meterIndex, int? meterIndex2, string onAddress)
    {
        _mixer = mixer;
        Key = index;
        
        _outputMeterAddress = outputMeterAddress;
        _meterIndex = meterIndex;
        _meterIndex2 = meterIndex2;
        
        Name = new SyncedValue<string>(_mixer, nameAddress, "Unknown Channel");
        IsOn = new SyncedValue<bool>(_mixer, onAddress, true);
        MainFaderLevel = new SyncedValue<float>(_mixer, faderLevelAddress, 0.0f);

        // Mixbus sends:
        // mixbus sends will only work with channels 1..16 and Fx Return (not available for main mix)
        string mixSendFaderLevelAddress;
        if (nameAddress.Contains("/ch/"))
        {
            mixSendFaderLevelAddress = $"/ch/{index}/mix/{{0}}/level";   
            for (var busIndex = 1; busIndex <= 6; busIndex++)
            {
                BusSendFaderLevels[busIndex - 1] = new SyncedValue<float>(_mixer, string.Format(mixSendFaderLevelAddress, $"{busIndex:00}"), 0.0f);    
            }     
        }
        else // it's a fx return channel
        {
            // TODO: index here is "rtn1".."rtn4", but we need the fxIndex (1..4) for the address => fix namings!
            // TODO: test
            mixSendFaderLevelAddress = $"/rtn/{index}/mix/{{0}}/level";
            // TODO: create separate fader levels collecion similar to BusSendFaderLevels or reuse the same?
        }
    
                
        // TODO: implement meter handling
    }    

    public string Key => $"Ch {field}";
        
    public SyncedValue<string> Name { get; }

    /// <summary>
    /// "On" or "Off" (muted) on main mix
    /// </summary>
    public SyncedValue<bool> IsOn { get; }
    
    public SyncedValue<float> MainFaderLevel { get; }

    public SyncedValue<float>[] BusSendFaderLevels = new SyncedValue<float>[6]; // bus index (caution, array index is 0-based!)
}