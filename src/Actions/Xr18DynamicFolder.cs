namespace Loupedeck.Xr18OscPlugin.Actions;

using System.Collections.Generic;
using Loupedeck.Xr18OscPlugin.Domain;

/// <summary>
/// Dynamic folder for adjusting mix bus send levels for channels.
/// - users place the "Aux Mix Adjustments" dynamic folder on a button in Loupedeck software
/// - when pressing the button, first a list of available mix busses is shown (currently Aux 1-6 with use defined names)
/// - users select a mix bus by pressing the button (if two aux busses are linked to be stereo it's sufficient to select one of them)
/// - afterwards buttons can be used to select a channel range (e.g. "Channels 1..6", "Channels 7..12", "Channels 13..18").
/// - When a channel range is selected, the dials of the Loupedeck can be used to adjust the mix send level for the selected 
///   bus. The current value is shown on the display of the Loupedeck.
/// </summary>
public class Xr18DynamicFolder : PluginDynamicFolder
{     
    // TODO: for Loupedeck Live a set of 6 knobs is available, for Loupedeck Live S only 2.
    // => create a lists for each supported device type
    private readonly List<string> availableChannelRanges =
    [
        "Channels 1..6",
        "Channels 7..12",
        "Channels 13..18",
        // TODO: we could also support FX1-4 here.
    ];

    /// <summary>
    /// We recall the selected mix bus here. Initially, it's empty and we show the list of available mix busses.
    /// This will be used as key (actionParameter) to identify which bus the user wants to adjust.
    /// We use the "Key" property of the bus instead of "Name" because name is not necessarily unique.
    /// Keep possible values in sync with Key implementation!
    /// - "" (empty - nothing selected)
    /// - "lr" (Main LR Bus)
    /// - "Aux1".."AuxN" (depending on the number of aux busses supported by the mixer, e.g. Aux1..Aux6 for XR18)
    /// - "Fx Return 1".."Fx Return 4"
    /// </summary>
    private string currentMixBus = "";

    // recall the selected channel range here. This determines which channels are shown for fader adjustment.
    private string currentChannelRange = "";

    public Xr18DynamicFolder()
    {
        DisplayName = "Aux Mix Adjustments";
        GroupName = "Aux Bus Mixes";
        Description = "Opens menu to select Aux Bus and use dials to set Mix Send Levels for each channel";

        // Subscribe to bus changes
        foreach (var bus in Xr18OscPlugin.Mixer.Busses)
        {
            bus.Name.ValueChanged += (s, e) => ButtonActionNamesChanged();            
        }

        // Subscribe to channel changes
        foreach (var channel in Xr18OscPlugin.Mixer.Channels)
        {
            foreach (var busSendFader in channel.BusSendFaderLevels)
            {
                busSendFader.ValueChanged += (s, e) => AdjustmentValueChanged(channel.Key);
            }
        }

        // Subscribe to fx channel changes
        foreach (var fxChannel in Xr18OscPlugin.Mixer.FxChannels)
        {
            foreach (var busSendFader in fxChannel.BusSendFaderLevels)
            {
                busSendFader.ValueChanged += (s, e) => AdjustmentValueChanged(fxChannel.Key);    
            }
        }
    }

    /// <summary>
    /// Gets the list of the actionParameters for buttons ("which buttons are defined").
    /// </summary>
    /// <param name="deviceType"></param>
    /// <returns></returns>
    public override IEnumerable<string> GetButtonPressActionNames(DeviceType deviceType)
    {
        // There are two levels: the target mix bus and the channel range selection.
        // Initially, we show the list of available mix busses.       
        if (string.IsNullOrEmpty(currentMixBus))
        {
            foreach (var bus in Xr18OscPlugin.Mixer.Busses)
            {
                yield return CreateCommandName(bus.Key);
            }
            yield break;
        }
       
        // otherwise show "Bus select" and list of channel ranges:
        yield return CreateCommandName($"Change Bus (current: {currentMixBus})");        
        foreach (var range in availableChannelRanges)
        {
            yield return CreateCommandName(range);
        }        
    }

    /// <summary>
    /// Get the current display name for a button.
    /// </summary>
    /// <param name="actionParameter"></param>
    /// <param name="imageSize"></param>
    /// <returns></returns>
    public override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize)
    {
        // Buttons with key "Aux1".."Aux6" represent a "Select aux bus" action:
        if (actionParameter.StartsWith("Aux"))
        {
            var auxBus = Xr18OscPlugin.Mixer.Busses.Single(x => x.Key == actionParameter);
            return auxBus.Name.Value;
        }

        // Channel selection buttons have key "Channels 1..6", "Channels 7..12" or "Channels 13..18"
        return actionParameter.StartsWith("Channels ") 
            ? actionParameter 
            : actionParameter;
    }

    public override IEnumerable<string> GetEncoderRotateActionNames(DeviceType deviceType)
    {
        // only show dials when a mix bus is selected
        if (string.IsNullOrEmpty(currentMixBus))
            return [];

        switch (currentChannelRange)
        {
            case "Channels 1..6":
                return [
                    CreateAdjustmentName("Ch 01"),
                    CreateAdjustmentName("Ch 02"),
                    CreateAdjustmentName("Ch 03"),
                    CreateAdjustmentName("Ch 04"),
                    CreateAdjustmentName("Ch 05"),
                    CreateAdjustmentName("Ch 06")
                    ];
            case "Channels 7..12":
                return [
                    CreateAdjustmentName("Ch 07"),
                    CreateAdjustmentName("Ch 08"),
                    CreateAdjustmentName("Ch 09"),
                    CreateAdjustmentName("Ch 10"),
                    CreateAdjustmentName("Ch 11"),
                    CreateAdjustmentName("Ch 12")
                    ];
            case "Channels 13..18":
                return [
                    CreateAdjustmentName("Ch 13"),
                    CreateAdjustmentName("Ch 14"),
                    CreateAdjustmentName("Ch 15"),
                    CreateAdjustmentName("Ch 16"),
                    CreateAdjustmentName("Ch 17"),
                    CreateAdjustmentName("Ch 18")
                    ];
            default:
                return [];
        }        
    }

    public override void RunCommand(string actionParameter)
    {
        if (actionParameter.StartsWith("Change Bus"))
        {
            // navigate up to mix bus selection
            currentMixBus = "";
            ButtonActionNamesChanged();
            EncoderActionNamesChanged();
        }
        else
        if (Xr18OscPlugin.Mixer.Busses.Select(x => x.Key).Contains(actionParameter))
        {
            // user selected a mix bus
            currentMixBus = actionParameter;
            ButtonActionNamesChanged();
            EncoderActionNamesChanged();
            var bus = Xr18OscPlugin.Mixer.Busses.Single(x => x.Key == currentMixBus);
            foreach (var channel in Xr18OscPlugin.Mixer.Channels)
            {
                channel.BusSendFaderLevels[bus.Index-1].ValueChanged += (s, e) => AdjustmentValueChanged(channel.Key);                
            }
        }
        else if (availableChannelRanges.Contains(actionParameter))
        {
            // user selected a channel range
            currentChannelRange = actionParameter;
            EncoderActionNamesChanged();
        }
    }

    public override void ApplyAdjustment(string actionParameter, int diff)
    {
        if (string.IsNullOrEmpty(currentMixBus))
            return;
            
        var bus = Xr18OscPlugin.Mixer.Busses.Single(x => x.Key == currentMixBus);
        var channel = Xr18OscPlugin.Mixer.Channels.Single(x => x.Key == actionParameter);
        
        var newMixFaderLevel = channel.BusSendFaderLevels[bus.Index-1].Value;

        newMixFaderLevel += Math.Abs(diff) switch
        {
            1 => diff * 0.007f,
            2 => diff * 0.01f,
            _ => diff * 0.01f,
        };

        if (newMixFaderLevel > 1)
        {
            newMixFaderLevel = 1.0f;
        }
        if (newMixFaderLevel < 0)
        {
            newMixFaderLevel = 0.0f;
        }

        channel.BusSendFaderLevels[bus.Index-1].Set(newMixFaderLevel);        
    }

    public override string GetAdjustmentValue(string actionParameter)
    {
        // a dial is targeting a specific channel on a specific bus
        // e.g. "Bus 1 - Channel 3".
        if (string.IsNullOrEmpty(currentMixBus))
            return "";

        var bus = Xr18OscPlugin.Mixer.Busses.Single(x => x.Key == currentMixBus);
        return Xr18OscPlugin.Mixer.Channels.Single(x => x.Key == actionParameter).BusSendFaderLevels[bus.Index-1].Value.ToString("P0");
    }
}