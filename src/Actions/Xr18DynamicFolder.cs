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

    /// <summary>
    /// We recall the selected mix bus here. Initially, it's empty and we show the list of available mix busses.
    /// This will be used as key (actionParameter) to identify which bus the user wants to adjust.
    /// We can't use the "Name" property of the bus directly because that is not necessarily unique.
    /// Possible values:
    /// - "" (empty - nothing selected)
    /// - "LR" (Main LR Bus)
    /// - "Aux1".."Aux6"
    /// - "Fx1".."Fx4"
    /// </summary>
    private string currentMixBus = "";

    // recall the selected channel range here. This determines which channels are shown for fader adjustment.
    private string currentChannelRange = "";

    public Xr18DynamicFolder()
    {
        DisplayName = "Mixbus";
        GroupName = "Custom Mixes (IEM, Monitors, etc.)";
        Description = "Dynamic Folder for testing";

        // Subscribe to bus changes
        foreach (var bus in Xr18OscPlugin.Mixer.Busses.All)
        {
            bus.NameChanged += (s, e) => ButtonActionNamesChanged();
        }

        // Subscribe to channel changes
        foreach (var channel in Xr18OscPlugin.Mixer.Channels.All)
        {
            channel.MixSendFaderLevelChanged += (s, e) => AdjustmentValueChanged("Ch01");
        }
    }

    public override IEnumerable<string> GetButtonPressActionNames(DeviceType deviceType)
    {
        // There are two levels: the target mix bus and the channel range selection.
        // Initially, we show the list of available mix busses.       
        if (string.IsNullOrEmpty(currentMixBus))
        {
            foreach (var bus in Xr18OscPlugin.Mixer.Busses.All)
            {
                yield return CreateCommandName(bus.Key);
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

    public override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize)
    {
        if (actionParameter.StartsWith("Aux"))
        {
            var auxBus = Xr18OscPlugin.Mixer.Busses.All.Single(x => x.Key == actionParameter);
            return auxBus.Name;
        }

        // fallback
        return actionParameter;
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
                    CreateAdjustmentName("Ch01"),
                    CreateAdjustmentName("Ch02"),
                    CreateAdjustmentName("Ch03"),
                    CreateAdjustmentName("Ch04"),
                    CreateAdjustmentName("Ch05"),
                    CreateAdjustmentName("Ch06")
                    ];
            case "Channel 7..12":
                return [
                    CreateAdjustmentName("Ch07"),
                    CreateAdjustmentName("Ch08"),
                    CreateAdjustmentName("Ch09"),
                    CreateAdjustmentName("Ch10"),
                    CreateAdjustmentName("Ch11"),
                    CreateAdjustmentName("Ch12")
                    ];
            case "Channel 13..16":
                return [
                    CreateAdjustmentName("Ch13"),
                    CreateAdjustmentName("Ch14"),
                    CreateAdjustmentName("Ch15"),
                    CreateAdjustmentName("Ch16")
                    ];
            default:
                return [];
        }        
    }



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
        if (Xr18OscPlugin.Mixer.Busses.All.Select(x => x.Key).Contains(actionParameter))
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

    public override void ApplyAdjustment(string actionParameter, int diff)
    {
        var bus = Xr18OscPlugin.Mixer.Busses.All.Single(x => x.Key == currentMixBus);
        var channel = Xr18OscPlugin.Mixer.Channels.All.Single(x => x.Key == actionParameter);
        
        var newMixFaderLevel = channel.MixSendFaderLevel[bus.Index-1];
        
        switch (Math.Abs(diff))
        {
            case 1:
                newMixFaderLevel += diff * 0.007f;
                break;
            case 2:
                newMixFaderLevel += diff * 0.01f;
                break;
            default:
                newMixFaderLevel += diff * 0.01f;
                break;
        }   

        if (newMixFaderLevel > 1)
        {
            newMixFaderLevel = 1.0f;
        }
        if (newMixFaderLevel < 0)
        {
            newMixFaderLevel = 0.0f;
        }

        channel.SetMixSendFaderLevel(bus.Index, newMixFaderLevel).Wait();        
    }

    public override string GetAdjustmentValue(string actionParameter)
    {
        // a dial is targeting a specific channel on a specific bus
        // e.g. "Bus 1 - Channel 3".
        // => TODO: identify mix (main mix, bus 1-6, fx 1-4) and channel from actionParameter
        // then read current fader level for that channel on that bus
        if (string.IsNullOrEmpty(currentMixBus))
            return "";

        // TODO: read from OSC "/ch/01/mix/01/level"
        // TODO: this should work for AuxBusses but how does it work for Fx busses and Main?
        var bus = Xr18OscPlugin.Mixer.Busses.All.Single(x => x.Key == currentMixBus);
        return Xr18OscPlugin.Mixer.Channels.All.Single(x => x.Key == actionParameter).MixSendFaderLevel[bus.Index-1].ToString("P0");
    }
}
