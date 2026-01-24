namespace Loupedeck.Xr18OscPlugin.Domain;

using SharpOSC;

/// <summary>
/// Represents a single auxiliary bus (monitor bus 1..6) on the mixer.
/// </summary>
public class MainLrBus
{
    /// <summary>
    /// The parent mixer of this bus.
    /// </summary>
    private Mixer _mixer { get; }

    private readonly string _nameAddress;
    
    public MainLrBus(Mixer mixer)
    {
        _mixer = mixer;
        _nameAddress = "/lr/config/name";
        
        // Subscribe handlers to receive updates from mixer:
        _mixer.RegisterHandler(_nameAddress, OnNameChanged);

        // Init values: Send empty OSC messages to mixer in order to trigger that mixer sends us current values:
        _mixer.Send(_nameAddress).Wait();
    }

    /// <summary>
    /// Name of the bus as configured in the mixer UI (can be changed by user).
    /// </summary>
    public string Name { get; private set; } = "Main LR";

    public float Pan { get; }

    public Task SetPan(float pan) => _mixer.Send($"/lr/mix/pan", pan);

    private void OnNameChanged(object? sender, OscMessage e)
    {
        if (e.Arguments[0] is string name) 
        {
            Name = string.IsNullOrEmpty(name) ? "Main LR" : name;
            NameChanged?.Invoke(this, name);
        }
    }

    #region events to communicate changes to consumers (plugin actions etc.)

    public event EventHandler<string>? NameChanged;
    
    #endregion
}