namespace Loupedeck.Xr18OscPlugin.Actions;

/// <summary>
/// This action command can be used to send any integer value to the mixer via OSC.
/// Loupedeck itself supports things like toggles or grouping multiple commands together so this
/// command is quite powerful if you know what OSC messages to send.
/// 
/// Examples: Address: /ch/01/dyn/on Value: 0 => Switch off the compressor on channel 1
///           Address: /ch/02/eq/on Value: 1 => Switch on the EQ on channel 2
///           Address: /headamp/01/phantom Value: 1 => Enable phantom power on preamp of channel 1
/// </summary>
public class SendOscMessageIntCommand : ActionEditorCommand
{
    public SendOscMessageIntCommand()
    {
        Name = "Send OSC Command (int/bool)";
        DisplayName = "Send OSC command with int parameter to mixer";
        GroupName = "Commands";

        ActionEditor.AddControlEx(
            new ActionEditorTextbox("Address", "OSC Address", "The OSC address to send a value to.")
            .SetPlaceholder("/ch/01/dyn/on")
            .SetRequired()
        );

        ActionEditor.AddControlEx(
            new ActionEditorTextbox("Value", "Value", "The OSC value to send. Must be an integer number")
            .SetPlaceholder("0")
            .SetRequired()
        );
    }
    
    protected override bool RunCommand(ActionEditorActionParameters actionParameter)
    {
        var address = actionParameter.GetString("Address");
        if (!actionParameter.TryGetInt32("Value", out var value))
            return false;
            
        Xr18OscPlugin.Mixer.Send(address, value).Wait();
        ActionImageChanged();
        return true;
    }

}
