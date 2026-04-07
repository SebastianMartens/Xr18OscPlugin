namespace Loupedeck.Xr18OscPlugin.Tests.Helpers;

using global::Xr18OscPlugin.Helpers;

public class MeterDisplayFormatterTests
{
    [Fact]
    public void FormatMeterBar_Zero_ReturnsAllEmpty()
    {
        var result = MeterDisplayFormatter.FormatMeterBar(0f);
        Assert.Equal("░░░░░░░░", result);
    }

    [Fact]
    public void FormatMeterBar_One_ReturnsAllFilled()
    {
        var result = MeterDisplayFormatter.FormatMeterBar(1f);
        Assert.Equal("▓▓▓▓▓▓▓▓", result);
    }

    [Fact]
    public void FormatMeterBar_Half_ReturnsHalfFilled()
    {
        var result = MeterDisplayFormatter.FormatMeterBar(0.5f);
        Assert.Equal("▓▓▓▓░░░░", result);
    }

    [Fact]
    public void FormatMeterBar_NegativeValue_ClampsToZero()
    {
        var result = MeterDisplayFormatter.FormatMeterBar(-0.5f);
        Assert.Equal("░░░░░░░░", result);
    }

    [Fact]
    public void FormatMeterBar_OverOne_ClampsToOne()
    {
        var result = MeterDisplayFormatter.FormatMeterBar(1.5f);
        Assert.Equal("▓▓▓▓▓▓▓▓", result);
    }

    [Fact]
    public void FormatMeterBar_QuarterValue_ReturnsTwoFilled()
    {
        var result = MeterDisplayFormatter.FormatMeterBar(0.25f);
        Assert.Equal("▓▓░░░░░░", result);
    }

    [Fact]
    public void FormatMeterBar_AlwaysReturnsEightCharacters()
    {
        for (var v = 0f; v <= 1f; v += 0.1f)
        {
            var result = MeterDisplayFormatter.FormatMeterBar(v);
            Assert.Equal(8, result.Length);
        }
    }
}
