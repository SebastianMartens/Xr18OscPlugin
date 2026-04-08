namespace Loupedeck.Xr18OscPlugin;

/// <summary>
/// Loupedeck application descriptor for the XR18 OSC plugin.
/// Currently unused since the plugin operates in API-only mode without binding to a specific application.
/// </summary>
public class Xr18OscApplication : ClientApplication
{
    public Xr18OscApplication()
    {
    }

    // This method can be used to link the plugin to a Windows application.
    protected override string GetProcessName() => "";

    // This method can be used to link the plugin to a macOS application.
    protected override string GetBundleName() => "";

    // This method can be used to check whether the application is installed or not.
    public override ClientApplicationStatus GetApplicationStatus() => ClientApplicationStatus.Unknown;
}
