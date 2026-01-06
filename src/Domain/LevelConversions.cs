namespace Loupedeck.Xr18OscPlugin.Domain;

internal static class LevelConversions
{
    /// <summary>
    /// Converts dB (from output, i.e. expected to be -inf to 0) to
    /// a linear value in the range 0-1, suitable for display.
    /// Taken from John Skeets original implementation (https://github.com/jskeet/DemoCode/blob/main/OscMixerControl/OscMixerControl/LevelConversions.cs).
    /// </summary>
    internal static double LevelDbToLinear(float db)
    {
        // Historically we scaled to 0-600. We can adjust this later...
        var scaled = db switch
        {
            >= -5f => (db + 5f) * (100f / 5f) + 500,
            >= -10f => (db + 10f) * (100f / 5f) + 400,
            >= -20f => (db + 20f) * (100f / 10f) + 300,
            >= -30f => (db + 30f) * (100f / 10f) + 200,
            >= -50f => (db + 50f) * (100f / 20f) + 100,
            >= -75f => (db + 75f) * (100f / 25) + 0,
            _ => 0f
        };

        return scaled / 600.0;

        // The code above is adapted from John Skeet's original implementation fo XR18.
        // The code below is for X32.
        // TODO: both are not in use right now and need to be verified.

        // var scaledX32 = db switch
        // {
        //     < -60 => (db + 90) / 480,
        //     < -30 => (db + 70) / 160,
        //     < -10 => (db + 50) / 80,
        //     <= 10 => (db + 30) / 40,
        //     _ => throw new ArgumentOutOfRangeException(nameof(db), "dB value must be in range -90 to +10")
        // };
        // return Convert.ToInt32(scaledX32 * 1023.5) / 1023; // round “f” to a X32 known value
    }

    /// <summary>
    /// Converts a linear float-value in the range 0-1 like it's used for OSC
    /// communication to dB.
    /// Reason: instead of sending db values on a log scale, the mixer uses
    /// linear float values from 0.0 to 1.0, which are then mapped to four different
    /// linear functions to approximate dB values.
    /// Taken from https://behringer.world/wiki/doku.php?id=x32_midi_table (linked pdf)
    /// 
    /// TODO: this seems to be true for X32. Need to verify for XR18.
    /// </summary>
    internal static double LevelLinearToDb(float volFloat)
    {
        return volFloat switch
        {
            >= 0.5f => (double)(volFloat * 40 - 30),        // max dB value: +10
            >= 0.25f => (double)(volFloat * 80 - 50),
            >= 0.0625f => (double)(volFloat * 160 - 70),
            >= 0.0f => (double)(volFloat * 480 - 90),       // min dB value: -90 or -oo
            _ => throw new ArgumentOutOfRangeException(nameof(volFloat), "Volume float must be in range 0.0 to 1.0"),
        };
    }
}