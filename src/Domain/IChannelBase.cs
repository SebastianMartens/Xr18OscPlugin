namespace Loupedeck.Xr18OscPlugin.Domain;

using global::Xr18OscPlugin.Helpers;

/// <summary>
/// Some features are common for different types of objects on the mixer (e.g. input channels and fx return channels both have main fader levels).
/// </summary>
public interface IChannelBase
{
    public SyncedValue<string> Name { get; }
    
    /// <summary>
    /// "On" or "Off" (muted) on main mix
    /// </summary>
    public SyncedValue<bool> IsOn { get; }

    /// <summary>
    /// Fader level for the main mix (not the bus sends). Value range is 0.0..1.0.
    /// For fx return channels, the actual return signal level is determined by the send levels from the input channels and 
    /// the fader level of the fx return channel itself.
    /// For input channels, this is just the main fader level of the channel.
    /// </summary>
    public SyncedValue<float> MainFaderLevel { get; }

    /// <summary>
    /// Current meter value (live volume level) received from the mixer.
    /// </summary>
    public float MeterValue { get; }

    /// <summary>
    /// Raised whenever a new meter value is received from the mixer.
    /// </summary>
    public event EventHandler<float>? MeterValueUpdated;
}
