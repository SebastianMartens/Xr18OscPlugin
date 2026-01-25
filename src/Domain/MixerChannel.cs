namespace Loupedeck.Xr18OscPlugin.Domain;

using SharpOSC;

/// <summary>
/// Represents a single channel on the mixer.
/// </summary>
public class MixerChannel
{
    private readonly Mixer _mixer;

    private readonly string _index; // formatted index of the channel (e.g. "01", "02", .. "16")

    // address patterns for OSC communication
    private readonly string _nameAddress;
    private readonly string _faderLevelAddress;
    private readonly string _onAddress;
    private readonly string _mixSendFaderLevelAddress;

    private readonly string _outputMeterAddress; // not finished, yet
    private readonly int _meterIndex;// not finished, yet
    private readonly int? _meterIndex2;// not finished, yet


    public MixerChannel(
            Mixer mixer, string index, string nameAddress, string faderLevelAddress,
            string outputMeterAddress, int meterIndex, int? meterIndex2, string onAddress)
    {
        _mixer = mixer;
        _index = index;
        _nameAddress = nameAddress;
        _onAddress = onAddress;
        _faderLevelAddress = faderLevelAddress;

        // mixbus sends will only work with channels (not available for main mix)
        _mixSendFaderLevelAddress = $"/ch/{index}/mix/{{0}}/level";

        _outputMeterAddress = outputMeterAddress;
        _meterIndex = meterIndex;
        _meterIndex2 = meterIndex2;
        

        // Subscribe handlers to receive updates from mixer:
        _mixer.RegisterHandler(_nameAddress, OnNameChanged);
        _mixer.RegisterHandler(_onAddress, OnIsOnChanged);
        _mixer.RegisterHandler(_faderLevelAddress, OnFaderLevelChanged);
        _mixer.RegisterHandler(string.Format(_mixSendFaderLevelAddress, "01"), OnMixSendFaderLevelChanged); 
        // TODO: implement meter handling
        
        // Init values: Send empty OSC messages to mixer in order to trigger that mixer sends us current values:
        _mixer.Send(_nameAddress).Wait();
        _mixer.Send(_onAddress).Wait();
        _mixer.Send(_faderLevelAddress).Wait();
        _mixer.Send(string.Format(_mixSendFaderLevelAddress, "01")).Wait();
    }    

    public string Key => $"Ch{_index}";
    
    public string Name { get; private set; } = "Unknown Channel";

    /// <summary>
    /// "On" or "Off" (muted) on main mix
    /// </summary>
    public bool IsOn { get; set; } = true;
    
    public float MainFaderLevel = 0.0f;
    public float[] MixSendFaderLevel = new float[6]; // bus index (0-based!)

    #region setters to send data/commands to mixer

    public Task SetIsOn(bool isOn) => _mixer.Send(_onAddress, isOn ? 1 : 0);

    public Task ToggleOnOff() => SetIsOn(!IsOn);

    public Task SetMainFaderLevel(float level) => _mixer.Send(_faderLevelAddress, level);

    public Task SetMixSendFaderLevel(int auxBusNumber, float level) => _mixer.Send($"/ch/{_index}/mix/{auxBusNumber:00}/level", level);

    #endregion

    #region events to communicate changes to consumers (plugin actions etc.)

    public event EventHandler<string>? NameChanged;

    public event EventHandler<bool>? IsOnChanged;

    public event EventHandler<float>? MainFaderLevelChanged;

    public event EventHandler<(int, float)>? MixSendFaderLevelChanged; // (busindex, level)

    #endregion

    #region Handlers for incoming OSC messages from mixer

    private void OnNameChanged(object? sender, OscMessage e)
    {
        if (e.Arguments[0] is string name) 
        {
            Name = name;
            NameChanged?.Invoke(this, name);
        }
    }

    /// <summary>
    /// Mixer sended mute state to us
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnIsOnChanged(object? sender, OscMessage e) 
    {
        if (e.Arguments[0] is int isOn)
        {
            IsOn = isOn == 1;
            IsOnChanged?.Invoke(this, IsOn);
        }
    }

    /// <summary>
    /// Mixer sended main mix level to us
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnFaderLevelChanged(object? sender, OscMessage e) 
    {
        if (e.Arguments[0] is float faderLevel)
        {
            MainFaderLevel = faderLevel;
            MainFaderLevelChanged?.Invoke(this, faderLevel);
        }
    }

    /// <summary>
    /// Mixer sended main mix level to us
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMixSendFaderLevelChanged(object? sender, OscMessage e) 
    {
        if (e.Arguments[0] is float faderLevel)
        {
            MixSendFaderLevel[0] = faderLevel; // TODO: wrong bus index!
            MixSendFaderLevelChanged?.Invoke(this, (1, faderLevel));
        }
    }


    #endregion
}