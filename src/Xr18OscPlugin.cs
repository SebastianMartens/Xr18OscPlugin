namespace Loupedeck.Xr18OscPlugin;

using Loupedeck.Xr18OscPlugin.Domain;

public class Xr18OscPlugin : Plugin
{
    // Gets a value indicating whether this is an API-only plugin.
    public override bool UsesApplicationApiOnly => true;

    // Gets a value indicating whether this is a Universal plugin or an Application plugin.
    public override bool HasNoApplication => true;

    /// <summary>
    /// Our own custom Mixer class is the main domain object for this plugin.
    /// All actions and other operations are routed through this object
    /// (as long as it does not grow too large and needs to be split up further).
    /// </summary>
    public static Mixer Mixer { get; } = new();

    public Xr18OscPlugin()
    {
        PluginLog.Init(Log);
        PluginResources.Init(Assembly);
        
        Mixer.IsConnectedChanged += (sender, isConnected) => UpdateStatus();
    }

    public void UpdateStatus()
    {
        if (Mixer.IsConnected)
        {
            OnPluginStatusChanged(Loupedeck.PluginStatus.Disabled, $"Connected to mixer: {Mixer.Name} (Model: {Mixer.Model}, FW: {Mixer.FirmwareVersion}) at IP Address {Mixer.OscRemoteIpAddress}.", null, null);
        }
        else
        {
            OnPluginStatusChanged(Loupedeck.PluginStatus.Warning, "Could not find mixer. Please ensure the mixer is powered on and connected to the network. Use Connect button action to connect to mixers if broadcasts are not allowed in your network.", null, null);
        }
    }

    public override void Load() => base.Load();

    public override void Unload()
    {
        Mixer.CloseConnection();
        base.Unload();
    }
}
