namespace Loupedeck.Xr18OscPlugin.Tests.Domain;

using Loupedeck.Xr18OscPlugin.Domain;

public class LevelConversionsTests
{
    #region LevelLinearToDb

    [Theory]
    [InlineData(1.0f, 10.0)]    // max: 1.0 => +10 dB
    [InlineData(0.75f, 0.0)]    // 0.75 => 0 dB
    [InlineData(0.5f, -10.0)]   // boundary between first two segments
    [InlineData(0.25f, -30.0)]  // boundary between segments 2 and 3
    [InlineData(0.0625f, -60.0)] // boundary between segments 3 and 4
    [InlineData(0.0f, -90.0)]   // min: 0.0 => -90 dB
    public void LevelLinearToDb_KnownValues(float linear, double expectedDb)
    {
        var result = LevelConversionsAccessor.LinearToDb(linear);
        Assert.Equal(expectedDb, result, precision: 1);
    }

    [Fact]
    public void LevelLinearToDb_NegativeValue_Throws()
    {
        var ex = Assert.ThrowsAny<Exception>(() => LevelConversionsAccessor.LinearToDb(-0.1f));
        Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException ?? ex);
    }

    [Fact]
    public void LevelLinearToDb_IsMonotonicallyIncreasing()
    {
        var previous = double.MinValue;
        for (var f = 0.0f; f <= 1.0f; f += 0.01f)
        {
            var db = LevelConversionsAccessor.LinearToDb(f);
            Assert.True(db >= previous, $"Not monotonically increasing at {f}: {db} < {previous}");
            previous = db;
        }
    }

    #endregion

    #region LevelDbToLinear

    [Theory]
    [InlineData(0f, 1.0)]       // 0 dB => 1.0
    [InlineData(-5f, 500.0 / 600.0)]
    [InlineData(-10f, 400.0 / 600.0)]
    [InlineData(-20f, 300.0 / 600.0)]
    [InlineData(-30f, 200.0 / 600.0)]
    [InlineData(-50f, 100.0 / 600.0)]
    [InlineData(-75f, 0.0)]
    [InlineData(-100f, 0.0)]    // below range clamps to 0
    public void LevelDbToLinear_KnownValues(float db, double expectedLinear)
    {
        var result = LevelConversionsAccessor.DbToLinear(db);
        Assert.Equal(expectedLinear, result, precision: 4);
    }

    [Fact]
    public void LevelDbToLinear_IsMonotonicallyIncreasing()
    {
        var previous = double.MinValue;
        for (var db = -80f; db <= 0f; db += 0.5f)
        {
            var linear = LevelConversionsAccessor.DbToLinear(db);
            Assert.True(linear >= previous, $"Not monotonically increasing at {db}: {linear} < {previous}");
            previous = linear;
        }
    }

    #endregion
}

/// <summary>
/// Helper to access internal methods via reflection-free approach.
/// Since LevelConversions is internal, we use InternalsVisibleTo or a wrapper.
/// </summary>
internal static class LevelConversionsAccessor
{
    // We need access to internal methods. We'll use reflection.
    private static readonly System.Reflection.MethodInfo _linearToDb =
        typeof(LevelConversions).GetMethod("LevelLinearToDb",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;

    private static readonly System.Reflection.MethodInfo _dbToLinear =
        typeof(LevelConversions).GetMethod("LevelDbToLinear",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;

    public static double LinearToDb(float value) => (double)_linearToDb.Invoke(null, [value])!;
    public static double DbToLinear(float value) => (double)_dbToLinear.Invoke(null, [value])!;
}
