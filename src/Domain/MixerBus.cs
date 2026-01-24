namespace Loupedeck.Xr18OscPlugin.Domain;

using SharpOSC;

public class MixerBus
{
    /// <summary>
    /// The parent mixer of this bus.
    /// </summary>
    private Mixer _mixer { get; }

    private readonly string _nameAddress;
    
    public MixerBus(Mixer mixer, string nameAddress, int busNumber)
    {
        _mixer = mixer;
        _nameAddress = nameAddress;
        BusNumber = busNumber;

        // Subscribe handlers to receive updates from mixer:
        _mixer.RegisterHandler(_nameAddress, OnNameChanged);

        // Init values: Send empty OSC messages to mixer in order to trigger that mixer sends us current values:
        _mixer.Send(_nameAddress).Wait();
    }

    /// <summary>
    /// XR18 has six mix-buses numbered 1-6.
    /// </summary>
    public int BusNumber { get; }
    
    public string Name { get; private set; } = "Unknown Bus";

    //public float FaderLevel { get; set; }
    //public bool IsMuted { get; set; }

    public double Pan { 
        get; 
        set 
        { 
            field = value; 
            _mixer.Send($"/bus/{BusNumber}/mix/pan", value).Wait();
        }
    }

    private void OnNameChanged(object? sender, OscMessage e)
    {
        if (e.Arguments[0] is string name) 
        {
            Name = string.IsNullOrEmpty(name) ? $"Bus {BusNumber}" : name;
            NameChanged?.Invoke(this, name);
        }
    }

    #region events to communicate changes to consumers (plugin actions etc.)

    public event EventHandler<string>? NameChanged;
    
    #endregion
}