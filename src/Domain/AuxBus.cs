namespace Loupedeck.Xr18OscPlugin.Domain;

using SharpOSC;

/// <summary>
/// Represents a single auxiliary bus (monitor bus 1..6) on the mixer.
/// </summary>
public class AuxBus
{
    /// <summary>
    /// The parent mixer of this bus.
    /// </summary>
    private Mixer _mixer { get; }

    private readonly string _nameAddress;
    
    /// <summary>
    /// The key to identify this bus (e.g. "Aux1", "Aux2", .. "Aux6").
    /// We can't use the user defined Name property here because that is not necessarily unique.
    /// Also using only the index as key is not nice as we use the key in context with other indexed items (e.g. "Fx1", "Fx2", ..).
    /// </summary>
    public string Key => $"Aux{Index}";

    public AuxBus(Mixer mixer, int index)
    {
        _mixer = mixer;
        Index = index;
        _nameAddress = $"/bus/{index}/config/name";
        

        // Subscribe handlers to receive updates from mixer:
        _mixer.RegisterHandler(_nameAddress, OnNameChanged);

        // Init values: Send empty OSC messages to mixer in order to trigger that mixer sends us current values:
        _mixer.Send(_nameAddress).Wait();
    }

    /// <summary>
    /// XR18 has six mix-buses numbered 1-6.
    /// </summary>
    public int Index { get; }
    
    /// <summary>
    /// Name of the bus as configured in the mixer UI (can be changed by user).
    /// </summary>
    public string Name { get; private set; } = "Unknown Bus";

    private void OnNameChanged(object? sender, OscMessage e)
    {
        if (e.Arguments[0] is string name) 
        {
            Name = string.IsNullOrEmpty(name) ? $"Bus {Index}" : name;
            NameChanged?.Invoke(this, name);
        }
    }

    #region events to communicate changes to consumers (plugin actions etc.)

    public event EventHandler<string>? NameChanged;
    
    #endregion
}