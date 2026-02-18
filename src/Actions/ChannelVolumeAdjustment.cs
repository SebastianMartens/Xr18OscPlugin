namespace Loupedeck.Xr18OscPlugin.Actions;

using Loupedeck.Xr18OscPlugin.Domain;

/// <summary>
/// Control main mix volume of individual channels on the mixer.
/// This class is resuable for both regular channels and fx return channels..
/// </summary>
public class ChannelVolumeAdjustment : PluginDynamicAdjustment
{   
    public ChannelVolumeAdjustment(): base(true)
    {   
        // create one adjustment per input channel
        foreach (var channel in Xr18OscPlugin.Mixer.Channels)
        {
            // "AddParameter" is a badly named "Please create an adjustment item in the Loupedeck software for me".
            AddParameter(channel.Key, $"{channel.Key} Volume", "Input Channel Adjustments");            
            if (TryGetParameter(channel.Key, out var param))
            {
                param.ResetDisplayName = $"Mute {channel.Key}";
            }
        }
        // Subscribe to channel changes to update displayed adjustment values on the dials:        
        foreach (var channel in Xr18OscPlugin.Mixer.Channels)
        {
            channel.Name.ValueChanged += (s, e) => AdjustmentValueChanged(channel.Key);
            channel.IsOn.ValueChanged += (s, e) => AdjustmentValueChanged(channel.Key);
            channel.MainFaderLevel.ValueChanged += (s, e) => AdjustmentValueChanged(channel.Key);            
        }


        // create one adjustment per fx channel
        foreach (var fxChannel in Xr18OscPlugin.Mixer.FxChannels)
        {            
            AddParameter(fxChannel.Key, $"{fxChannel.Key} Volume", "FX Channel Adjustments");            
            if (TryGetParameter(fxChannel.Key, out var param))
            {
                param.ResetDisplayName = $"Mute {fxChannel.Key}";
            }
        }
        foreach (var fxChannel in Xr18OscPlugin.Mixer.FxChannels)
        {
            fxChannel.Name.ValueChanged += (s, e) => AdjustmentValueChanged(fxChannel.Key);
            fxChannel.IsOn.ValueChanged += (s, e) => AdjustmentValueChanged(fxChannel.Key);
            fxChannel.MainFaderLevel.ValueChanged += (s, e) => AdjustmentValueChanged(fxChannel.Key);            
        }
        

        // add main LR as well
        var mainLrBus = Xr18OscPlugin.Mixer.MainLrBus;
        AddParameter("lr", "Main LR Volume", "Main Adjustments");    
        if (TryGetParameter("lr", out var lrParam))
        {
            lrParam.ResetDisplayName = $"Mute Main LR";
        }
        mainLrBus.Name.ValueChanged += (s, e) => AdjustmentValueChanged("lr");
        mainLrBus.IsOn.ValueChanged += (s, e) => AdjustmentValueChanged("lr");
        mainLrBus.MainFaderLevel.ValueChanged += (s, e) => AdjustmentValueChanged("lr");
    }

    protected override void ApplyAdjustment(string actionParameter, int diff)
    {
        IChannelBase? faderObj;
        switch (actionParameter)
        {
            case "lr":
                faderObj = (IChannelBase?)Xr18OscPlugin.Mixer.MainLrBus;
                break;
            default:
            {
                faderObj = actionParameter.StartsWith("Fx")
                    ? Xr18OscPlugin.Mixer.FxChannels.SingleOrDefault(x => x.Key == actionParameter)
                    : Xr18OscPlugin.Mixer.Channels.SingleOrDefault(x => x.Key == actionParameter);
                break;
            }
        }

        if (faderObj == null)
            return;
        var newFaderLevel = faderObj.MainFaderLevel.Value;

        newFaderLevel += Math.Abs(diff) switch
        {
            1 => diff * 0.007f,
            2 => diff * 0.01f,
            _ => diff * 0.01f,
        };

        if (newFaderLevel > 1)
        {
            newFaderLevel = 1.0f;
        }
        if (newFaderLevel < 0)
        {
            newFaderLevel = 0.0f;
        }

        faderObj.MainFaderLevel.Set(newFaderLevel).Wait();        
    }

    /// <summary>
    /// Called on dial press.
    /// We mute the channel on press but only if we're on the main mix.
    /// </summary>
    /// <param name="actionParameter"></param>
    protected override void RunCommand(string actionParameter)
    {
        IChannelBase? channel = Xr18OscPlugin.Mixer.Channels.SingleOrDefault(x => x.Key == actionParameter);
        if (channel == null && actionParameter.StartsWith("Fx"))
        {
            channel = Xr18OscPlugin.Mixer.FxChannels.SingleOrDefault(x => x.Key == actionParameter);
        }
        if (channel == null && actionParameter == "lr")
        {
            channel = Xr18OscPlugin.Mixer.MainLrBus;
        }
        if (channel == null)
            return;

        var oldValue = channel.IsOn.Value;
        channel.IsOn.Set(!oldValue).Wait();        
    }

    // Returns the adjustment value that is shown next to the dial.
    protected override string GetAdjustmentValue(string actionParameter)
    {
        if (actionParameter == "lr")
            return Xr18OscPlugin.Mixer.MainLrBus.IsOn.Value ? Xr18OscPlugin.Mixer.MainLrBus.MainFaderLevel.Value.ToString("#.00") : "MUTE";

        if (actionParameter.StartsWith("Fx"))
        {
            var fxChannel = Xr18OscPlugin.Mixer.FxChannels.SingleOrDefault(x => x.Key == actionParameter);
            if (fxChannel != null)
                return fxChannel.IsOn.Value ? fxChannel.MainFaderLevel.Value.ToString("#.00") : "MUTE";
        }
        
        var channel = Xr18OscPlugin.Mixer.Channels.SingleOrDefault(x => x.Key == actionParameter);
        if (channel != null)
            return channel.IsOn.Value ? channel.MainFaderLevel.Value.ToString("#.00") : "MUTE";

        return "";
    }

    protected override string GetAdjustmentDisplayName(string actionParameter, PluginImageSize imageSize)
    {
        IChannelBase? channel = Xr18OscPlugin.Mixer.Channels.SingleOrDefault(x => x.Key == actionParameter);
        if (channel != null)
            return channel.Name.Value ?? string.Empty;
        
        channel = Xr18OscPlugin.Mixer.FxChannels.SingleOrDefault(x => x.Key == actionParameter);
        if (channel != null)
            return channel.Name.Value ?? string.Empty;

        if (actionParameter == "lr")
            return Xr18OscPlugin.Mixer.MainLrBus.Name.Value ?? string.Empty;       

        return actionParameter;
    }
}
