namespace Loupedeck.Xr18OscPlugin.Domain;

/// <summary>
/// We group all mixer channels here to ease initialization.
/// </summary>
public class MixerChannels 
{
    private Mixer _mixer { get; }

    /// <summary>
    /// Gets or sets the list of channels on the mixer.
    /// </summary>
    public Dictionary<string, MixerChannel> Channels { get; set; } = [];

    public MixerChannels(Mixer mixer)
    {
        _mixer = mixer;
        InitChannels();        
    }

    private void InitChannels() 
    {
        Channels = [];       
        var stereo = false; // TODO: stereo config from mixer settings not yet implemented

        // Create regular channels 1-16
        // Channels 17/18 are Line Inputs and usually used for USB return but can be 
        // configured as regular channels as well (currently not yet supported here)
        for (var channelIndex = 1; channelIndex <= 16; channelIndex++)
        {
            Channels.Add($"{channelIndex:00}", new MixerChannel(
                _mixer,
                $"/ch/{channelIndex:00}/config/name",
                $"/ch/{channelIndex:00}/mix/fader",
                $"/meters/1",
                meterIndex: channelIndex - 1,
                meterIndex2: stereo ? channelIndex : default(int?),
                $"/ch/{channelIndex:00}/mix/on")
            );
        }

        // Main mix channel
        Channels.Add("lr", new MixerChannel(
            _mixer,
            $"/lr/config/name",
            $"/lr/mix/fader",
            $"/meters/5",
            meterIndex: 6,
            meterIndex2: 7,
            $"/lr/mix/on"));

        // Fx return channels
        for (var fxIndex = 1; fxIndex <= 4; fxIndex++)
        {
            Channels.Add($"rtn{fxIndex}", new MixerChannel(
                _mixer,
                $"/rtn/{fxIndex}/config/name",
                $"/rtn/{fxIndex}/mix/fader",
                $"/meters/3",
                meterIndex: 4 + fxIndex, // TODO: verify meter indices (not tested yet)
                meterIndex2: null,
                $"/rtn/{fxIndex}/mix/on"));
        }     
    }      
}
    