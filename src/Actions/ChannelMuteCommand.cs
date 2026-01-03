// namespace Loupedeck.Xr18OscPlugin;

// /// <summary>
// /// This action command can be used to mute a channel.
// /// It's more an example than a useful command because the ChannelVolumeAdjustmentCommand already has a mute option.
// /// </summary>
// public class MuteCommand : ActionEditorCommand
// {
//     public MuteCommand()
//     {
//         Name = "Mute Channel";
//         DisplayName = "Mute";
//         GroupName = "Channel Commands";

//         ActionEditor.AddControlEx(
//             new ActionEditorTextbox("channel", "Channel #")
//             .SetPlaceholder("01")
//             .SetRequired()
//         );            
//     }

//     // This method is called when the user executes the command.
//     protected override bool RunCommand(ActionEditorActionParameters actionParameter)
//     {
//         var channelNumber = actionParameter.GetString("channel");
//         var channel = Xr18OscPlugin.Mixer.MixerChannels.Channels[channelNumber];

//         channel.ToggleOnOff();
//         ActionImageChanged();
//         return true;
//     }
// }
