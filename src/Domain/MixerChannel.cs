namespace Loupedeck.Xr18OscPlugin.Domain;

using global::Xr18OscPlugin.Domain;
using SharpOSC;

/// <summary>
/// Represents a single channel on the mixer.
/// </summary>
public class MixerChannel
{
    private readonly Mixer _mixer;

    // address patterns for OSC communication
    private readonly string _nameAddress;
    private readonly string _faderLevelAddress;
    private readonly string _onAddress;
    private readonly string _outputMeterAddress; // not finished, yet
    private readonly int _meterIndex;// not finished, yet
    private readonly int? _meterIndex2;// not finished, yet

    public MixerChannel(
            Mixer mixer, string nameAddress, string faderLevelAddress,
            string outputMeterAddress, int meterIndex, int? meterIndex2, string onAddress)
    {
        _mixer = mixer;

        _nameAddress = nameAddress;
        _onAddress = onAddress;
        _faderLevelAddress = faderLevelAddress;

        _outputMeterAddress = outputMeterAddress;
        _meterIndex = meterIndex;
        _meterIndex2 = meterIndex2;
        

        // Subscribe handlers to receive updates from mixer:
        _mixer.RegisterHandler(_nameAddress, OnNameChanged);
        _mixer.RegisterHandler(_onAddress, OnIsOnChanged);
        _mixer.RegisterHandler(_faderLevelAddress, OnFaderLevelChanged);

        // Init values: Send empty OSC messages to mixer in order to trigger that mixer sends us current values:
        _mixer.Send(_nameAddress).Wait();
        _mixer.Send(_onAddress).Wait();
        _mixer.Send(_faderLevelAddress).Wait();
    }    

    public string Name { get; private set; } = "Unknown Channel";

    /// <summary>
    /// "On" or "Off" (muted) on main mix
    /// </summary>
    public bool IsOn { get; set; } = true;
    
    public float FaderLevel = 0.0f;

    #region setters to send data/commands to mixer

    public Task SetIsOn(bool isOn) => _mixer.Send(_onAddress, isOn ? 1 : 0);

    public Task ToggleOnOff() => SetIsOn(!IsOn);

    public Task SetFaderLevel(float level) => _mixer.Send(_faderLevelAddress, level);

   
    #endregion

    #region events to communicate changes to consumers (plugin actions etc.)

    public event EventHandler<string>? NameChanged;

    public event EventHandler<bool>? IsOnChanged;

    public event EventHandler<float>? FaderLevelChanged;

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
            FaderLevel = faderLevel;
            FaderLevelChanged?.Invoke(this, FaderLevel);
        }
    }

    #endregion
    
    #region Bus Fader Level - w.i.p.

    /// <summary>
    /// Channel mixbus sends level
    /// Key: 1-6 (Aux Bus number)
    /// Value: Level (0.0 - 1.0)
    /// </summary>
    // public Dictionary<int, float> BusSendLevels { get; } = new Dictionary<int, float>()
    // {
    //     // TODO: Initialize all bus sends with values from mixer        
    //     { 1, 0.0f },
    //     { 2, 0.0f },
    //     { 3, 0.0f },
    //     { 4, 0.0f },
    //     { 5, 0.0f },
    //     { 6, 0.0f }
    // };
    
    //public void SetBusSendLevel(int busNumber, float level) => _mixer.Send($"/ch/{ChannelNumber}/mix/{busNumber}/level", level).Wait();

    //internal void OnFaderChanged(OscMessage msg) => FaderLevel = msg.TryGetFieldValue<double>("");

    #endregion
}