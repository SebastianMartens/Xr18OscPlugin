namespace Xr18OscPlugin.Helpers;

/// <summary>
/// Formats meter values as text-based bar representations for the Loupedeck display.
/// </summary>
public static class MeterDisplayFormatter
{
    private const int BarLength = 8;
    private const char FilledBlock = '▓';
    private const char EmptyBlock = '░';

    /// <summary>
    /// Converts a meter value (0.0..1.0+) to a text bar like "▓▓▓▓░░░░".
    /// Values are clamped to 0.0..1.0.
    /// </summary>
    public static string FormatMeterBar(float meterValue)
    {
        var clamped = Math.Clamp(meterValue, 0f, 1f);
        var filledCount = (int)Math.Round(clamped * BarLength);
        return new string(FilledBlock, filledCount) + new string(EmptyBlock, BarLength - filledCount);
    }
}
