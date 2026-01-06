namespace Loupedeck.Xr18OscPlugin.Actions;

public class ChannelVolumeAdjustment : PluginDynamicAdjustment
{   
    /// <summary>
    /// The curent Mix Bus defines which volume levels are adjusted.
    /// E.g. if the dials should adjust the main mix or the channel volume send to a specific aux bus.
    /// Bus "lr" is the main mix.
    /// Bus "aux1"-"aux6" are the aux buses.
    /// Bus "fx1"-"fx4" are the FX return channels.
    /// </summary>
    //public static string currentMixBus = "lr"; // default to main mix

    public ChannelVolumeAdjustment(): base(true)
    {   
        // create one adjustment per channel (18 channels + main mix + 4 Fx return channels)
        foreach (var channel in Xr18OscPlugin.Mixer.MixerChannels.Channels)
        {
            AddParameter(channel.Key, $"{channel.Key} Volume", "Channel Adjustments");
            if (TryGetParameter(channel.Key, out var param))
            {
                param.ResetDisplayName = $"Mute Channel {channel.Key}";
            }
        }
                
        // Subscribe to channel changes to update displayed adjustment values on the dials:
        foreach (var channel in Xr18OscPlugin.Mixer.MixerChannels.Channels.Values)
        {
            channel.NameChanged += (s, e) => AdjustmentValueChanged();
            channel.IsOnChanged += (s, e) => AdjustmentValueChanged();
            channel.FaderLevelChanged += (s, e) => AdjustmentValueChanged();        
        }        
    }

    protected override void ApplyAdjustment(string actionParameter, int diff)
    {
        var channel = Xr18OscPlugin.Mixer.MixerChannels.Channels[actionParameter];
        
        // TODO: currently we only set level of bus "lr" (main mix)
        // => introduce cocept of "current bus" (a dial should control a specific bus which is selectable)
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
        Xr18OscPlugin.Mixer.MixerChannels.Channels[actionParameter].ToggleOnOff();

    // Returns the adjustment value that is shown next to the dial.
    protected override string GetAdjustmentValue(string actionParameter)
    {
        if (!Xr18OscPlugin.Mixer.MixerChannels.Channels.TryGetValue(actionParameter, out var channel))
            return "";

        //switch (currentMixBus)
        //{
            //case "lr":
                return channel.IsOn ? channel.FaderLevel.ToString("#.00") : "MUTE";
            //default:
                //return channel.BusSendLevels[int.Parse(currentMixBus.Replace("aux", ""))].ToString("#.00");
        //}
    }

    protected override string GetAdjustmentDisplayName(string actionParameter, PluginImageSize imageSize)
    {
        return !Xr18OscPlugin.Mixer.MixerChannels.Channels.TryGetValue(actionParameter, out var channel)
            ? ""
            : channel.Name ?? actionParameter;
    }

}
