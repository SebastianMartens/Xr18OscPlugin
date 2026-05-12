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
            channel.Color.ValueChanged += (s, e) => AdjustmentValueChanged(channel.Key);
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
        IChannelBase? faderObj = Xr18OscPlugin.Mixer.Channels.SingleOrDefault(x => x.Key == actionParameter);
        if (faderObj == null && actionParameter.StartsWith("Fx"))
        {
            faderObj = Xr18OscPlugin.Mixer.FxChannels.SingleOrDefault(x => x.Key == actionParameter);
        }
        if (faderObj == null && actionParameter == "lr")
        {
            faderObj = Xr18OscPlugin.Mixer.MainLrBus;
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
        channel.IsOn.Set(!oldValue);        
    }

    // Returns the adjustment value that is shown next to the dial.
    // For regular input channels the value is embedded in the custom BitmapImage, so return
    // an empty string to suppress the duplicate label rendered by the framework.
    protected override string GetAdjustmentValue(string actionParameter)
    {
        if (actionParameter == "lr")
            return Xr18OscPlugin.Mixer.MainLrBus.IsOn.Value ? FormatLevel(Xr18OscPlugin.Mixer.MainLrBus.MainFaderLevel.Value) : "MUTE";

        if (actionParameter.StartsWith("Fx"))
        {
            var fxChannel = Xr18OscPlugin.Mixer.FxChannels.SingleOrDefault(x => x.Key == actionParameter);
            if (fxChannel != null)
                return fxChannel.IsOn.Value ? FormatLevel(fxChannel.MainFaderLevel.Value) : "MUTE";
        }

        // Regular input channels render name + value inside GetAdjustmentImage; suppress the extra label.
        var channel = Xr18OscPlugin.Mixer.Channels.SingleOrDefault(x => x.Key == actionParameter);
        if (channel != null)
            return string.Empty;

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

    protected override BitmapImage GetAdjustmentImage(string actionParameter, PluginImageSize imageSize)
    {
        // Only render with a color background for regular input channels (Ch 01-18).
        // FX channels and Main LR fall back to the framework's default text rendering.
        var channel = Xr18OscPlugin.Mixer.Channels.SingleOrDefault(x => x.Key == actionParameter);
        if (channel == null)
            return base.GetAdjustmentImage(actionParameter, imageSize);

        var bgColor = MixerColorToBitmapColor(channel.Color.Value);
        var fgColor = IsColorBright(bgColor) ? BitmapColor.Black : BitmapColor.White;

        using var builder = new BitmapBuilder(imageSize);
        builder.Clear(bgColor);
        builder.DrawText(
            channel.Name.Value ?? channel.Key,
            0, 0, builder.Width, builder.Height / 2,
            fgColor, 14, 14, 2, null);

        var value = channel.IsOn.Value
            ? FormatLevel(channel.MainFaderLevel.Value)
            : "MUTE";
        builder.DrawText(
            value,
            0, builder.Height / 2, builder.Width, builder.Height / 2,
            fgColor, 14, 14, 2, null);

        return builder.ToImage();
    }

    /// <summary>
    /// Maps a Behringer XR18 channel color index (0–15) to a <see cref="BitmapColor"/>.
    /// 0=OFF, 1=RD, 2=GN, 3=YE, 4=BL, 5=MG, 6=CY, 7=WH,
    /// 8=OFFi, 9=RDi, 10=GNi, 11=YEi, 12=BLi, 13=MGi, 14=CYi, 15=WHi
    /// (where "i" = inverted/bright variant).
    /// </summary>
    private static BitmapColor MixerColorToBitmapColor(int colorIndex) => colorIndex switch
    {
        1  => new BitmapColor(180, 40,  40),   // RD
        2  => new BitmapColor(40,  180, 40),   // GN
        3  => new BitmapColor(180, 180, 40),   // YE
        4  => new BitmapColor(40,  40,  180),  // BL
        5  => new BitmapColor(180, 40,  180),  // MG
        6  => new BitmapColor(40,  180, 180),  // CY
        7  => new BitmapColor(200, 200, 200),  // WH
        8  => BitmapColor.Black,               // OFFi (inverted off = same as off)
        9  => new BitmapColor(255, 120, 120),  // RDi
        10 => new BitmapColor(120, 255, 120),  // GNi
        11 => new BitmapColor(255, 255, 120),  // YEi
        12 => new BitmapColor(120, 120, 255),  // BLi
        13 => new BitmapColor(255, 120, 255),  // MGi
        14 => new BitmapColor(120, 255, 255),  // CYi
        15 => BitmapColor.White,               // WHi
        _  => BitmapColor.Black,               // 0=OFF or unknown
    };

    /// <summary>
    /// Returns true if the color is bright enough that black text will be legible on it.
    /// </summary>
    private static bool IsColorBright(BitmapColor color) =>
        (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) > 128;

    /// <summary>
    /// Converts a linear fader level (0.0–1.0) to a formatted dB string, e.g. "+0.0 dB" or "-10.5 dB".
    /// </summary>
    private static string FormatLevel(float volFloat) =>
        $"{LevelConversions.LevelLinearToDb(volFloat):+0.0;-0.0;0.0} dB";
}
