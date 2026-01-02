// namespace Xr18OscPlugin.Domain;

// public class MixerBus
// {
//     /// <summary>
//     /// The parent mixer of this bus.
//     /// </summary>
//     private Mixer Mixer { get; }

//     public MixerBus(Mixer mixer) => Mixer = mixer; // TODO: init state from mixer state            

//     /// <summary>
//     /// XR18 has six mix-buses numbered 1-6.
//     /// </summary>
//     public int BusNumber { get; set; }
    
//     //public float FaderLevel { get; set; }

//     //public bool IsMuted { get; set; }

//     public double Pan { 
//         get; 
//         set 
//         { 
//             field = value; 
//             Mixer.Send($"/bus/{BusNumber}/mix/pan", value).Wait();
//         }
//     }

// }