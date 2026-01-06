namespace Loupedeck.Xr18OscPlugin;

/// This action command can be used to send any floating-point value to the mixer via OSC.
/// Loupedeck itself supports things like toggles or grouping multiple commands together so this
/// command is quite powerful if you know what OSC messages to send.
/// 
/// Examples: Address: /ch/01/mix/pan   Value: 0.6   => Pan channel 1 slightly to the right
///           Address: /ch/02/mix/fader Value: 0.3 => Set channel 2 fader level to "medium"
public class SendOscMessageFloatCommand : ActionEditorCommand
{
    public SendOscMessageFloatCommand()
    {
        Name = "Send OSC Command (float)";
        DisplayName = "Send OSC command with float parameter to mixer";
        GroupName = "Commands";

        ActionEditor.AddControlEx(
            new ActionEditorTextbox("Address", "OSC Address", "The OSC address to send a value to.")
            .SetPlaceholder("/ch/01/mix/pan")
            .SetRequired()
        );

        ActionEditor.AddControlEx(
            new ActionEditorTextbox("Value", "Value", "The OSC value to send. Must be a floating point number.")
            .SetPlaceholder("0.25")
            .SetRequired()
        );
    }
    
    protected override bool RunCommand(ActionEditorActionParameters actionParameter)
    {
        var address = actionParameter.GetString("Address");
        if (!actionParameter.TryGetString("Value", out var value))
            return false;
            
        if (!float.TryParse(value, out var floatValue))
            return false;
            
        Xr18OscPlugin.Mixer.Send(address, floatValue).Wait();
        ActionImageChanged();
        return true;
    }
}
