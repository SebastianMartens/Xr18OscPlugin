namespace Xr18OscPlugin.Domain;

using System.Collections.Generic;
using Loupedeck.Xr18OscPlugin.Domain;

/// <summary>
/// Represents a collection of mixer busses (XR18: monitor bus 1..6).
/// </summary>
public class AuxBusses
{
    private readonly Mixer _mixer;

    public readonly List<AuxBus> All = [];
    
    public AuxBusses(Mixer mixer)
    {
        _mixer = mixer;
        InitBuses();
    }

    private void InitBuses()
    {
        for (var busIndex = 1; busIndex <= 6; busIndex++)
        {
            All.Add(new AuxBus(_mixer, busIndex));
        }
    }

}