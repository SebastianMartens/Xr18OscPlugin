namespace Loupedeck.Xr18OscPlugin.Actions;

/// <summary>
/// Control main mix volume of individual channels on the mixer.
/// </summary>
public class ChannelVolumeAdjustment : PluginDynamicAdjustment
{   
    public ChannelVolumeAdjustment(): base(true)
    {   
        // create one adjustment per channel (18 channels + 4 Fx return channels)
        foreach (var channel in Xr18OscPlugin.Mixer.Channels.All)
        {
            // "AddParameter" is a badly named "Please create an adjustment item in the Loupedeck software for me".
            AddParameter(channel.Key, $"{channel.Key} Volume", "Channel Adjustments");
            
            if (TryGetParameter(channel.Key, out var param))
            {
                param.ResetDisplayName = $"Mute Channel {channel.Key}";
            }
        }

        // add main LR as well
        var mainLrBus = Xr18OscPlugin.Mixer.MainLrBus;
        AddParameter("lr", "Main LR Volume", "Channel Adjustments");    
        if (TryGetParameter("lr", out var lrParam))
        {
            lrParam.ResetDisplayName = $"Mute Main LR";
        }
        mainLrBus.Name.ValueChanged += (s, e) => AdjustmentValueChanged();
        mainLrBus.IsOn.ValueChanged += (s, e) => AdjustmentValueChanged();
        mainLrBus.MainFaderLevel.ValueChanged += (s, e) => AdjustmentValueChanged();

        // Subscribe to channel changes to update displayed adjustment values on the dials:
        foreach (var channel in Xr18OscPlugin.Mixer.Channels.All)
        {
            channel.Name.ValueChanged += (s, e) => AdjustmentValueChanged();
            channel.IsOn.ValueChanged += (s, e) => AdjustmentValueChanged();
            channel.MainFaderLevel.ValueChanged += (s, e) => AdjustmentValueChanged();
        }
    }

    protected override void ApplyAdjustment(string actionParameter, int diff)
    {
        if (actionParameter == "lr")
        {
            var lrMix = Xr18OscPlugin.Mixer.MainLrBus;
            var newLrFaderLevel = lrMix.MainFaderLevel.Value;
        
            switch (Math.Abs(diff))
            {
                case 1:
                    newLrFaderLevel += diff * 0.007f;
                    break;
                case 2:
                    newLrFaderLevel += diff * 0.01f;
                    break;
                default:
                    newLrFaderLevel += diff * 0.01f;
                    break;
            }   

            if (newLrFaderLevel > 1)
            {
                newLrFaderLevel = 1.0f;
            }
            if (newLrFaderLevel < 0)
            {
                newLrFaderLevel = 0.0f;
            }

            lrMix.MainFaderLevel.Set(newLrFaderLevel).Wait();        
            return;
        }
        else {
            var channel = Xr18OscPlugin.Mixer.Channels.All.SingleOrDefault(x => x.Key == actionParameter);
            if (channel == null)
                return;
            var newMainMixFaderLevel = channel.MainFaderLevel.Value;
            
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

            channel.MainFaderLevel.Set(newMainMixFaderLevel).Wait();   
        }     
    }

    /// <summary>
    /// Called on dial press.
    /// We mute the channel on press but only if we're on the main mix.
    /// </summary>
    /// <param name="actionParameter"></param>
    protected override void RunCommand(string actionParameter)
    {
        if (actionParameter == "lr")
        {
            var oldValue = Xr18OscPlugin.Mixer.MainLrBus.IsOn.Value;
            Xr18OscPlugin.Mixer.MainLrBus.IsOn.Set(!oldValue).Wait();
            return;
        }

        var ch = Xr18OscPlugin.Mixer.Channels.All.SingleOrDefault(x => x.Key == actionParameter);
        if (ch != null)
        {
            var oldValue = ch.IsOn.Value;
            ch.IsOn.Set(!oldValue).Wait();
        }
    }

    // Returns the adjustment value that is shown next to the dial.
    protected override string GetAdjustmentValue(string actionParameter)
    {
        var channel = Xr18OscPlugin.Mixer.Channels.All.SingleOrDefault(x => x.Key == actionParameter);
        if (channel != null)
            return channel.IsOn.Value ? channel.MainFaderLevel.Value.ToString("#.00") : "MUTE";

        if (actionParameter == "lr")
            return Xr18OscPlugin.Mixer.MainLrBus.IsOn.Value ? Xr18OscPlugin.Mixer.MainLrBus.MainFaderLevel.Value.ToString("#.00") : "MUTE";

        return "";
    }

    protected override string GetAdjustmentDisplayName(string actionParameter, PluginImageSize imageSize)
    {
        var channel = Xr18OscPlugin.Mixer.Channels.All.SingleOrDefault(x => x.Key == actionParameter);
        if (channel != null)
            return channel.Name.Value;
        
        if (actionParameter == "lr")
            return Xr18OscPlugin.Mixer.MainLrBus.Name.Value;

        return actionParameter;
    }
}
