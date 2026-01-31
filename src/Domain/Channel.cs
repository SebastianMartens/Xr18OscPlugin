namespace Loupedeck.Xr18OscPlugin.Domain;

using global::Xr18OscPlugin.Domain;
using SharpOSC;


/// <summary>
/// Represents a single channel on the mixer. Capabilities:
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

    private readonly string _index;

    // address patterns for OSC communication
   
    private readonly string _mixSendFaderLevelAddress;

    private readonly string _outputMeterAddress; // not finished, yet
    private readonly int _meterIndex;// not finished, yet
    private readonly int? _meterIndex2;// not finished, yet


    public Channel(
            Mixer mixer, string index, string nameAddress, string faderLevelAddress,
            string outputMeterAddress, int meterIndex, int? meterIndex2, string onAddress)
    {
        _mixer = mixer;
        _index = index;                
        
        // mixbus sends will only work with channels (not available for main mix)
        _mixSendFaderLevelAddress = $"/ch/{index}/mix/{{0}}/level";

        _outputMeterAddress = outputMeterAddress;
        _meterIndex = meterIndex;
        _meterIndex2 = meterIndex2;
        
        Name = new SyncedValue<string>(_mixer, nameAddress, "Unknown Channel");
        IsOn = new SyncedValue<bool>(_mixer, onAddress, true);
        MainFaderLevel = new SyncedValue<float>(_mixer, faderLevelAddress, 0.0f);

        
        // Mixbus sends:
        // TODO: refactor to use new SyncedValue class
        _mixer.RegisterHandler(string.Format(_mixSendFaderLevelAddress, "01"), (s, e) => OnBusSendFaderLevelChanged(e, 1)); 
        _mixer.RegisterHandler(string.Format(_mixSendFaderLevelAddress, "02"), (s, e) => OnBusSendFaderLevelChanged(e, 2)); 
        _mixer.RegisterHandler(string.Format(_mixSendFaderLevelAddress, "03"), (s, e) => OnBusSendFaderLevelChanged(e, 3)); 
        _mixer.RegisterHandler(string.Format(_mixSendFaderLevelAddress, "04"), (s, e) => OnBusSendFaderLevelChanged(e, 4)); 
        _mixer.RegisterHandler(string.Format(_mixSendFaderLevelAddress, "05"), (s, e) => OnBusSendFaderLevelChanged(e, 5)); 
        _mixer.RegisterHandler(string.Format(_mixSendFaderLevelAddress, "06"), (s, e) => OnBusSendFaderLevelChanged(e, 6));
        
        // TODO: implement meter handling
        
        // Init values: Send empty OSC messages to mixer in order to trigger that mixer sends us current values:                        
        _mixer.Send(string.Format(_mixSendFaderLevelAddress, "01")).Wait();
        _mixer.Send(string.Format(_mixSendFaderLevelAddress, "02")).Wait();
        _mixer.Send(string.Format(_mixSendFaderLevelAddress, "03")).Wait();
        _mixer.Send(string.Format(_mixSendFaderLevelAddress, "04")).Wait();
        _mixer.Send(string.Format(_mixSendFaderLevelAddress, "05")).Wait();
        _mixer.Send(string.Format(_mixSendFaderLevelAddress, "06")).Wait();
    }    

    public string Key => $"Ch {_index}";
    
    public SyncedValue<string> Name { get; }

    /// <summary>
    /// "On" or "Off" (muted) on main mix
    /// </summary>
    public SyncedValue<bool> IsOn { get; }
    
    public SyncedValue<float> MainFaderLevel { get; }

    public float[] BusSendFaderLevel = new float[6]; // bus index (0-based!)
    

    public Task SetBusSendFaderLevel(int auxBusNumber, float level) => _mixer.Send($"/ch/{_index}/mix/{auxBusNumber:00}/level", level);

    public event EventHandler<(int, float)>? BusSendFaderLevelChanged; // (busindex, level)

    /// <summary>
    /// Mixer sended main mix level to us
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnBusSendFaderLevelChanged(OscMessage e, int busIndex) 
    {
        if (e.Arguments[0] is float faderLevel)
        {
            BusSendFaderLevel[busIndex - 1] = faderLevel;
            BusSendFaderLevelChanged?.Invoke(this, (busIndex, faderLevel));
        }
    }
}