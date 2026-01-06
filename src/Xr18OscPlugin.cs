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
    }

    public override void Load() => base.Load();

    public override void Unload()
    {
        Mixer.Close();
        base.Unload();
    }
}
