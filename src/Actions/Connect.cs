namespace Loupedeck.Xr18OscPlugin.Actions;

/// <summary>
/// We curently auto-discover the mixer on connect so this action is not strictly necessary.
/// But if multiple mixers are present on the network or broadcasts are not allowed,
/// it might be useful to set the IP manually.
/// </summary>
internal class ConnectCommand: ActionEditorCommand
{
    public ConnectCommand()
    {
        Name = "Connection Settings";
        DisplayName = "Manually enter Ip Address of Mixer";
        Description = "You only need this action if you want to set the IP manually and skip auto discovery.";
        GroupName = "Initialization";

        ActionEditor.AddControlEx(
            new ActionEditorTextbox(name: "sender_ip", labelText: "Mixer Device IP (leave empty for auto discovery)")
            .SetPlaceholder("127.0.0.1")            
         );                                 
    }

    protected override bool RunCommand(ActionEditorActionParameters actionParameters)
    {        
        if (!actionParameters.TryGetString("sender_ip", out var sender_ip))
        {
            Xr18OscPlugin.Mixer.OscRemoteIpAddress = sender_ip;
        }
        PluginLog.Info($"Running Connect Command to IP: {Xr18OscPlugin.Mixer.OscRemoteIpAddress}");

        Xr18OscPlugin.Mixer.ReconnectOsc(true).Wait();
        return true;
    }

    // protected override string GetCommandDisplayName(ActionEditorActionParameters actionParameters) 
    //     => $"Reconnect ({(Xr18OscPlugin.Mixer.IsConnected ? "Connected" : "Not Connected")})";
}