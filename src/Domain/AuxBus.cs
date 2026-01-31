namespace Loupedeck.Xr18OscPlugin.Domain;

using global::Xr18OscPlugin.Domain;

/// <summary>
/// Represents a single auxiliary bus (monitor bus 1..6) on the mixer.
/// </summary>
public class AuxBus
{
    private Mixer _mixer { get; }
        
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
                
        Name = new SyncedValue<string>(_mixer, $"/bus/{index}/config/name", $"Bus {index}");        
    }

    /// <summary>
    /// XR18 has six mix-buses numbered 1-6 (index is 1-based).
    /// </summary>
    public int Index { get; }
    
    /// <summary>
    /// Name of the bus as configured in the mixer UI (can be changed by user).
    /// </summary>
    public SyncedValue<string> Name { get; }
}