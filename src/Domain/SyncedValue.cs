namespace Loupedeck.Xr18OscPlugin.Domain;

using SharpOSC;

/// <summary>
/// Little value object that unifies and eases name handling and updates.
/// </summary>
public class SyncedValue<T>
{
    private readonly IOscClient _oscClient;
    private readonly string _oscAddress;

    private readonly T _defaultValue;

    public T Value { get; private set; }
    
    public event EventHandler<T>? ValueChanged;
    
    public SyncedValue(IOscClient oscClient, string oscAddress, T defaultValue)
    {
        _oscClient = oscClient;
        _oscAddress = oscAddress;
        _defaultValue = defaultValue;
        Value = defaultValue;

        // Subscribe handlers to receive updates from mixer:
        _oscClient.RegisterHandler(oscAddress, OnValueChanged);
    }

    public void Set(T value)
    {
        if (typeof(T) == typeof(bool))
        {
            _oscClient.Send(_oscAddress, value is bool v && v ? 1 : 0);
        }
        else 
        {
        _oscClient.Send(_oscAddress, value);
        }
    }

    private void OnValueChanged(object? sender, OscMessage e)
    {
        if (e.Arguments[0] is string stringValue && typeof(T) == typeof(string))
        {
            // special case: empty string handling is useful as mixer will report Bus names to be "" if they were not changed by user
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