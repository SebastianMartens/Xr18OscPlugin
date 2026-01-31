namespace Xr18OscPlugin.Domain;

using Loupedeck.Xr18OscPlugin.Domain;
using SharpOSC;

/// <summary>
/// Little value object that unifies and eases name handling and updates.
/// </summary>
public class SyncedValue<T>
{
    private readonly Mixer _mixer;
    private readonly string _oscAddress;

    private readonly T _defaultValue;

    public T Value { get; private set; }
    
    public event EventHandler<T>? ValueChanged;
    
    public SyncedValue(Mixer mixer, string oscAddress, T defaultValue)
    {
        _mixer = mixer;
        _oscAddress = oscAddress;
        _defaultValue = defaultValue;

        // Subscribe handlers to receive updates from mixer:
        _mixer.RegisterHandler(oscAddress, OnValueChanged);

        // Init values: Send empty OSC messages to mixer in order to trigger that mixer sends us current values:
        _mixer.Send(oscAddress).Wait();
    }

    public Task Set(T value)
    {
        return typeof(T) == typeof(bool) 
        ? _mixer.Send(_oscAddress, value is bool v && v ? 1 : 0) 
        : _mixer.Send(_oscAddress, value);
    }

    private void OnValueChanged(object? sender, OscMessage e)
    {
        if (e.Arguments[0] is string stringValue && typeof(T) == typeof(string))
        {
            // special case: empty string handling is useful as Bus names can be empty if they were not changed by user
            Value = !string.IsNullOrEmpty(stringValue) ? (T)(object)stringValue : _defaultValue;
            ValueChanged?.Invoke(this, Value);
            return;
        }

        if (e.Arguments[0] is int intValue && typeof(T) == typeof(bool))
        {
            // special case: bools are sent as int (0/1)
            Value = (T)(object)(intValue != 0);            
            ValueChanged?.Invoke(this, Value);
            return;
        }

        if (e.Arguments[0] is T typedValue)
        {
            Value = typedValue;
            ValueChanged?.Invoke(this, Value);
            return;
        }
    }
}