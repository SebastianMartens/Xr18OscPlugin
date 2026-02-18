namespace Loupedeck.Xr18OscPlugin.Domain;

using global::Xr18OscPlugin.Domain;

public interface IChannelBase
{
    public SyncedValue<string> Name { get; }
    
    /// <summary>
    /// "On" or "Off" (muted) on main mix
    /// </summary>
    public SyncedValue<bool> IsOn { get; }

    public SyncedValue<float> MainFaderLevel { get; }
}
