namespace Loupedeck.Xr18OscPlugin.Actions;

/// <summary>
/// Control main mix volume of individual channels on the mixer.
/// </summary>
public class ChannelVolumeAdjustment : PluginDynamicAdjustment
{   
    public ChannelVolumeAdjustment(): base(true)
    {   
        // create one adjustment per channel (18 channels + main mix + 4 Fx return channels)
        foreach (var channel in Xr18OscPlugin.Mixer.Channels.All)
        {
            // "AddParameter" is a badly named "Please create an adjustment item in the Loupedeck software for me".
            AddParameter(channel.Key, $"{channel.Key} Volume", "Channel Adjustments");
            
            if (TryGetParameter(channel.Key, out var param))
            {
                param.ResetDisplayName = $"Mute Channel {channel.Key}";
            }
        }
                
        // Subscribe to channel changes to update displayed adjustment values on the dials:
        foreach (var channel in Xr18OscPlugin.Mixer.Channels.All.Values)
        {
            channel.NameChanged += (s, e) => AdjustmentValueChanged();
            channel.IsOnChanged += (s, e) => AdjustmentValueChanged();
            channel.FaderLevelChanged += (s, e) => AdjustmentValueChanged();        
        }        
    }

    protected override void ApplyAdjustment(string actionParameter, int diff)
    {
        var channel = Xr18OscPlugin.Mixer.Channels.All[actionParameter];        
        var newMainMixFaderLevel = channel.FaderLevel;
        
        switch (Math.Abs(diff))
        {
            case 1:
                newMainMixFaderLevel += diff * 0.007f;
                break;
            case 2:
                newMainMixFaderLevel += diff * 0.01f;
                break;
            default:
                newMainMixFaderLevel += diff * 0.01f;
                break;
        }   

        if (newMainMixFaderLevel > 1)
        {
            newMainMixFaderLevel = 1.0f;
        }
        if (newMainMixFaderLevel < 0)
        {
            newMainMixFaderLevel = 0.0f;
        }

        channel.SetFaderLevel(newMainMixFaderLevel).Wait();        
    }

    /// <summary>
    /// Called on dial press.
    /// We mute the channel on press but only if we're on the main mix.
    /// </summary>
    /// <param name="actionParameter"></param>
    protected override void RunCommand(string actionParameter) =>        
        Xr18OscPlugin.Mixer.Channels.All[actionParameter].ToggleOnOff();

    // Returns the adjustment value that is shown next to the dial.
    protected override string GetAdjustmentValue(string actionParameter)
    {
        if (!Xr18OscPlugin.Mixer.Channels.All.TryGetValue(actionParameter, out var channel))
            return "";

        return channel.IsOn ? channel.FaderLevel.ToString("#.00") : "MUTE";        
    }

    protected override string GetAdjustmentDisplayName(string actionParameter, PluginImageSize imageSize)
    {
        return !Xr18OscPlugin.Mixer.Channels.All.TryGetValue(actionParameter, out var channel)
            ? ""
            : channel.Name ?? actionParameter;
    }

}
