#nullable disable

namespace Loupedeck.Xr18OscPlugin.Actions;

using System.Collections.Generic;

/// <summary>
/// No useful content yet. Only for experiementing with dynamic folders.
/// </summary>
public class Xr18DynamicFolder : PluginDynamicFolder
{     

    private readonly List<string> availableChannelRanges =
    [
        "Channel 1..6",
        "Channel 7..12",
        "Channel 13..16",
    ];

    // we recall the selected mix bus here. Initially, it's empty and we show the list of available mix busses.
    private string currentMixBus = "";

    // recall the selected channel range here. This determines which channels are shown for fader adjustment.
    private string currentChannelRange = "";

    public Xr18DynamicFolder()
    {
        DisplayName = "IEM Mix";
        GroupName = "Custom Mixes (IEM, Monitors, etc.)";
        Description = "Dynamic Folder for testing";

        // Subscribe to bus changes
        foreach (var bus in Xr18OscPlugin.Mixer.Busses.All.Values)
        {
            bus.NameChanged += (s, e) => ButtonActionNamesChanged();
        } 
    }

    public override IEnumerable<string> GetButtonPressActionNames(DeviceType deviceType)
    {
        // There are two levels: the target mix bus and the channel range selection.
        // Initially, we show the list of available mix busses.       
        if (string.IsNullOrEmpty(currentMixBus))
        {
            foreach (var bus in Xr18OscPlugin.Mixer.Busses.All.Values)
            {
                yield return CreateCommandName(bus.Name);
            }
            yield break;
        }
       
        // otherwise show list of channel ranges
        yield return CreateCommandName($"Current Mix Bus: {currentMixBus}");        
        foreach (var range in availableChannelRanges)
        {
            yield return CreateCommandName(range);
        }        
    }

    public override IEnumerable<string> GetEncoderRotateActionNames(DeviceType deviceType)
    {
        // only show dials when a mix bus is selected
        if (string.IsNullOrEmpty(currentMixBus))
            return [];

        switch (currentChannelRange)
        {
            case "Channel 1..6":
                return [
                    CreateAdjustmentName("Channel 1"),
                    CreateAdjustmentName("Channel 2"),
                    CreateAdjustmentName("Channel 3"),
                    CreateAdjustmentName("Channel 4"),
                    CreateAdjustmentName("Channel 5"),
                    CreateAdjustmentName("Channel 6")
                    ];
            case "Channel 7..12":
                return [
                    CreateAdjustmentName("Channel 7"),
                    CreateAdjustmentName("Channel 8"),
                    CreateAdjustmentName("Channel 9"),
                    CreateAdjustmentName("Channel 10"),
                    CreateAdjustmentName("Channel 11"),
                    CreateAdjustmentName("Channel 12")
                    ];
            case "Channel 13..16":
                return [
                    CreateAdjustmentName("Channel 13"),
                    CreateAdjustmentName("Channel 14"),
                    CreateAdjustmentName("Channel 15"),
                    CreateAdjustmentName("Channel 16")
                    ];
            default:
                return [];
        }        
    }


    // TODO: subscribe do all fader level changes of all busses??
    // Maybe better to know which busses/channels are currently displayed in the dynamic folder
    //private void OnVolumeChanged(Object sender, object e) =>
    //    this.AdjustmentValueChanged(e.Id);

    public override void RunCommand(string actionParameter)
    {
        if (actionParameter.StartsWith("Current Mix Bus:"))
        {
            // navigate up to mix bus selection
            currentMixBus = "";
            ButtonActionNamesChanged();
            EncoderActionNamesChanged();
        }
        else
        if (Xr18OscPlugin.Mixer.Busses.All.Values.Select(x => x.Name).Contains(actionParameter))
        {
            // user selected a mix bus
            currentMixBus = actionParameter;
            ButtonActionNamesChanged();
            EncoderActionNamesChanged();
        }
        else if (availableChannelRanges.Contains(actionParameter))
        {
            currentChannelRange = actionParameter;
            EncoderActionNamesChanged();
        }
    }

    public override string GetAdjustmentValue(string actionParameter)
    {
        // a dial is targeting a specific channel on a specific bus
        // e.g. "Bus 1 - Channel 3".
        // => TODO: identify mix (main mix, bus 1-6, fx 1-4) and channel from actionParameter
        // then read current fader level for that channel on that bus
        return "0%";
    }
}
