namespace Xr18OscPlugin.Domain;

using System.Collections.Generic;
using Loupedeck.Xr18OscPlugin.Domain;

/// <summary>
/// Represents a collection of mixer busses (monitor bus 1..6, Fx sends, etc).
/// </summary>
public class MixerBusses
{
    private readonly Mixer _mixer;

    /// <summary>
    /// Key: Name of the Bus how it's identified in the mixer (e.g. "Bus 1"). Does NOT change when user renames the bus in the mixer UI.
    /// </summary>
    public readonly List<AuxBus> All = [];
    
    public MixerBusses(Mixer mixer)
    {
        _mixer = mixer;
        InitBuses();
    }

    private void InitBuses()
    {
        // TODO: need common interface to group different kind of busses?
        //All.Add("Main Mix", new MainLrBus(_mixer));

        for (var busIndex = 1; busIndex <= 6; busIndex++)
        {
            All.Add(new AuxBus(_mixer, busIndex));
        }
                
        // TODO: rethink concept of BusNumber for Fx => better to use separate classes similar to OSC APi (https://behringer.world/wiki/doku.php?id=x-air_osc)?!
        // for (var fxIndex = 1; fxIndex <= 4; fxIndex++)
        // {
        //     All.Add($"FX Send {fxIndex}", new FxBus(
        //         _mixer, 
        //         $"/fxsend/{fxIndex}/config/name",
        //         100 + fxIndex));
        // }
    }

}