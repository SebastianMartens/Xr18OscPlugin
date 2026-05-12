namespace Loupedeck.Xr18OscPlugin.Actions;

using Domain;
using global::Xr18OscPlugin.Helpers;

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
            if (TryGetParameter(channel.Key, out var param)) param.ResetDisplayName = $"Mute {channel.Key}";
        }
        // Subscribe to channel changes to update displayed adjustment values on the dials:        
        foreach (var channel in Xr18OscPlugin.Mixer.Channels)
        {
            channel.Name.ValueChanged += (_, _) => AdjustmentValueChanged(channel.Key);
            channel.IsOn.ValueChanged += (_, _) => AdjustmentValueChanged(channel.Key);
            channel.MainFaderLevel.ValueChanged += (_, _) => AdjustmentValueChanged(channel.Key);
            channel.MeterValueUpdated += (_, _) => AdjustmentValueChanged(channel.Key);
        }


        // create one adjustment per fx channel
        foreach (var fxChannel in Xr18OscPlugin.Mixer.FxChannels)
        {            
            AddParameter(fxChannel.Key, $"{fxChannel.Key} Volume", "FX Channel Adjustments");            
            if (TryGetParameter(fxChannel.Key, out var param)) param.ResetDisplayName = $"Mute {fxChannel.Key}";
        }
        foreach (var fxChannel in Xr18OscPlugin.Mixer.FxChannels)
        {
            fxChannel.Name.ValueChanged += (_, _) => AdjustmentValueChanged(fxChannel.Key);
            fxChannel.IsOn.ValueChanged += (_, _) => AdjustmentValueChanged(fxChannel.Key);
            fxChannel.MainFaderLevel.ValueChanged += (_, _) => AdjustmentValueChanged(fxChannel.Key);
            fxChannel.MeterValueUpdated += (_, _) => AdjustmentValueChanged(fxChannel.Key);
        }
        

        // add main LR as well
        var mainLrBus = Xr18OscPlugin.Mixer.MainLrBus;
        AddParameter("lr", "Main LR Volume", "Main Adjustments");    
        if (TryGetParameter("lr", out var lrParam)) lrParam.ResetDisplayName = $"Mute Main LR";
        mainLrBus.Name.ValueChanged += (_, _) => AdjustmentValueChanged("lr");
        mainLrBus.IsOn.ValueChanged += (_, _) => AdjustmentValueChanged("lr");
        mainLrBus.MainFaderLevel.ValueChanged += (_, _) => AdjustmentValueChanged("lr");
        mainLrBus.MeterValueUpdated += (_, _) => AdjustmentValueChanged("lr");
    }

    protected override void ApplyAdjustment(string actionParameter, int diff)
    {
        IChannelBase? faderObj = Xr18OscPlugin.Mixer.Channels.SingleOrDefault(x => x.Key == actionParameter);
        if (faderObj == null && actionParameter.StartsWith("Fx")) 
            faderObj = Xr18OscPlugin.Mixer.FxChannels.SingleOrDefault(x => x.Key == actionParameter);
        
        if (faderObj == null && actionParameter == "lr") 
            faderObj = Xr18OscPlugin.Mixer.MainLrBus;
        
        if (faderObj == null)
            return;
        
        var newFaderLevel = faderObj.MainFaderLevel.Value;

        newFaderLevel += Math.Abs(diff) switch
        {
            1 => diff * 0.007f,
            2 => diff * 0.015f,
            _ => diff * 0.01f,
        };

        if (newFaderLevel > 1) newFaderLevel = 1.0f;
        if (newFaderLevel < 0) newFaderLevel = 0.0f;

        faderObj.MainFaderLevel.Set(newFaderLevel);        
    }

    /// <summary>
    /// Called on dial press.
    /// We mute the channel on press but only if we're on the main mix.
    /// </summary>
    /// <param name="actionParameter"></param>
    protected override void RunCommand(string actionParameter)
    {
        IChannelBase? channel = Xr18OscPlugin.Mixer.Channels.SingleOrDefault(x => x.Key == actionParameter);
        if (channel == null && actionParameter.StartsWith("Fx")) channel = Xr18OscPlugin.Mixer.FxChannels.SingleOrDefault(x => x.Key == actionParameter);
        if (channel == null && actionParameter == "lr") channel = Xr18OscPlugin.Mixer.MainLrBus;
        if (channel == null)
            return;

        var oldValue = channel.IsOn.Value;
        channel.IsOn.Set(!oldValue);        
    }

    protected override string GetAdjustmentValue(string actionParameter)
    {
        IChannelBase? channel;

        if (actionParameter == "lr")
            channel = Xr18OscPlugin.Mixer.MainLrBus;
        else if (actionParameter.StartsWith("Fx"))
            channel = Xr18OscPlugin.Mixer.FxChannels.SingleOrDefault(x => x.Key == actionParameter);
        else
            channel = Xr18OscPlugin.Mixer.Channels.SingleOrDefault(x => x.Key == actionParameter);

        if (channel == null)
            return "";

        if (!channel.IsOn.Value)
            return "MUTE";

        var faderText = channel.MainFaderLevel.Value.ToString("#.00");
        var meterBar = MeterDisplayFormatter.FormatMeterBar(channel.MeterValue);
        return $"{faderText}\n{meterBar}";
    }

    protected override string GetAdjustmentDisplayName(string actionParameter, PluginImageSize imageSize)
    {
        IChannelBase? channel = Xr18OscPlugin.Mixer.Channels.SingleOrDefault(x => x.Key == actionParameter);
        if (channel != null)
            return channel.Name.Value;
        
        channel = Xr18OscPlugin.Mixer.FxChannels.SingleOrDefault(x => x.Key == actionParameter);
        if (channel != null)
            return channel.Name.Value;

        if (actionParameter == "lr")
            return Xr18OscPlugin.Mixer.MainLrBus.Name.Value;

        return actionParameter;
    }
}
