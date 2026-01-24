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
    public readonly Dictionary<string, MixerBus> All = [];
    
    public MixerBusses(Mixer mixer)
    {
        _mixer = mixer;
        InitBuses();
    }

    private void InitBuses()
    {
        All.Add("Main Mix", new MixerBus(_mixer, "/lr/config/name") { BusNumber = 0 });

        All.Add("Bus 1", new MixerBus(_mixer, "/bus/1/config/name") { BusNumber = 1 });
        All.Add("Bus 2", new MixerBus(_mixer, "/bus/2/config/name") { BusNumber = 2 });
        All.Add("Bus 3", new MixerBus(_mixer, "/bus/3/config/name") { BusNumber = 3 });
        All.Add("Bus 4", new MixerBus(_mixer, "/bus/4/config/name") { BusNumber = 4 });
        All.Add("Bus 5", new MixerBus(_mixer, "/bus/5/config/name") { BusNumber = 5 });
        All.Add("Bus 6", new MixerBus(_mixer, "/bus/6/config/name") { BusNumber = 6 });
        
        // TODO: rethink concept of BusNumber for Fx => better to use separate classes similar to OSC APi (https://behringer.world/wiki/doku.php?id=x-air_osc)?!
        All.Add("FX 1 Sends", new MixerBus(_mixer, "/fxsend/1/config/name") { BusNumber = 101 });
        All.Add("FX 2 Sends", new MixerBus(_mixer, "/fxsend/2/config/name") { BusNumber = 102 });
        All.Add("FX 3 Sends", new MixerBus(_mixer, "/fxsend/3/config/name") { BusNumber = 103 });
        All.Add("FX 4 Sends", new MixerBus(_mixer, "/fxsend/4/config/name") { BusNumber = 104 });
    }

}