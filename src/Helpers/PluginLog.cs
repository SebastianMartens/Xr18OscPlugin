namespace Loupedeck.Xr18OscPlugin;

using System;

// A helper class that enables logging from the plugin code.

internal static class PluginLog
{
    private static PluginLogFile? _pluginLogFile;

    /// <summary>
    /// Tipp: filter log messages (e.g. in VSCode Debug Console) by using the prefix "[Xr18OscPlugin]" 
    /// which is added to all log messages by this helper.
    /// </summary>
    private static readonly string _logMsgPrefix = "[Xr18OscPlugin]";

    public static void Init(PluginLogFile pluginLogFile)
    {
        pluginLogFile.CheckNullArgument(nameof(pluginLogFile));
        PluginLog._pluginLogFile = pluginLogFile;
    }

    public static void Verbose(string text) => PluginLog._pluginLogFile?.Verbose($"{_logMsgPrefix} {text}");

    public static void Verbose(Exception ex, string text) => PluginLog._pluginLogFile?.Verbose(ex, $"{_logMsgPrefix} {text}");

    public static void Info(string text) => PluginLog._pluginLogFile?.Info($"{_logMsgPrefix} {text}");

    public static void Info(Exception ex, string text) => PluginLog._pluginLogFile?.Info(ex, $"{_logMsgPrefix} {text}");

    public static void Warning(string text) => PluginLog._pluginLogFile?.Warning($"{_logMsgPrefix} {text}");

    public static void Warning(Exception ex, string text) => PluginLog._pluginLogFile?.Warning(ex, $"{_logMsgPrefix} {text}");

    public static void Error(string text) => PluginLog._pluginLogFile?.Error($"{_logMsgPrefix} {text}");

    public static void Error(Exception ex, string text) => PluginLog._pluginLogFile?.Error(ex, $"{_logMsgPrefix} {text}");
}
