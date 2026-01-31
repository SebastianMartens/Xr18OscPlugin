namespace Loupedeck.Xr18OscPlugin.Domain;

using global::Xr18OscPlugin.Domain;

/// <summary>
/// Represents the Main LR channel on the mixer.
/// </summary>
public class MainLrBus
{    
    private Mixer _mixer { get; }

    public MainLrBus(Mixer mixer)
    {
        _mixer = mixer;
        
        Name = new SyncedValue<string>(_mixer, "/lr/config/name", "Main LR");
        IsOn = new SyncedValue<bool>(_mixer, "/lr/mix/on", true);
        MainFaderLevel = new SyncedValue<float>(_mixer, "/lr/mix/fader", 0.0f);       
        Pan = new SyncedValue<float>(_mixer, "/lr/mix/pan", 0.0f);
    }

    /// <summary>
    /// Name of the bus as configured in the mixer UI (can be changed by user).
    /// </summary>
    public SyncedValue<string> Name { get; }
    
    public SyncedValue<bool> IsOn { get; }
    
    public SyncedValue<float> MainFaderLevel { get; }

    public SyncedValue<float> Pan { get; }
}