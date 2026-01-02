namespace SharpOSC;

using System;
using System.Text;

public static class Deserializer
{
    public static string GetAddress(ReadOnlySpan<byte> buffer, int index)
    {
        var i = index;
        for (; i < buffer.Length; i += 4)
        {
            if (buffer[i] != ',') continue;
            if (i == 0) return string.Empty;

            var address = Encoding.ASCII.GetString(buffer.Slice(index, i - 1));
            return address.Replace("\0", null);
        }

        if (i >= buffer.Length) 
            throw new Exception("No comma found after address");
        
        throw new Exception("Address is null");
    }

    public static int GetTypesLength(ReadOnlySpan<byte> buffer, int index)
    {
        var i = index + 4;

        for (; i < buffer.Length; i += 4)
        {
            if (buffer[i - 1] != 0) 
                continue;
            return buffer[index..i].Length;
        }

        if (i >= buffer.Length) 
            throw new Exception("No null terminator after type string");

        throw new Exception("Types is null");
    }

    public static int GetTypes(ReadOnlySpan<byte> buffer, int index, Span<char> types)
    {
        var i = index + 4;

        for (; i < buffer.Length; i += 4)
        {
            if (buffer[i - 1] != 0) 
                continue;
            return Encoding.ASCII.GetChars(buffer[index..i], types);
        }

        if (i >= buffer.Length) 
            throw new Exception("No null terminator after type string");
            
        throw new Exception("Types is null");
    }

    public static int GetInt(ReadOnlySpan<byte> buffer, int index)
    {
        Span<byte> value = stackalloc byte[sizeof(int)];
        value[3] = buffer[index + 0];
        value[2] = buffer[index + 1];
        value[1] = buffer[index + 2];
        value[0] = buffer[index + 3];

        return BitConverter.ToInt32(value);
    }

    public static float GetFloat(ReadOnlySpan<byte> buffer, int index)
    {
        Span<byte> value = stackalloc byte[sizeof(float)];
        value[3] = buffer[index + 0];
        value[2] = buffer[index + 1];
        value[1] = buffer[index + 2];
        value[0] = buffer[index + 3];

        return BitConverter.ToSingle(value);
    }

    public static string? GetString(ReadOnlySpan<byte> buffer, int index)
    {
        var i = index + 4;
        for (; i <= buffer.Length; i += 4)
        {
            if (buffer[i - 1] != 0) continue;
            var output = Encoding.ASCII.GetString(buffer[index..i]);
            return output.Replace("\0", null);
        }

        return i >= buffer.Length ? 
            throw new Exception("No null terminator after type string") : null;
    }

    public static ReadOnlySpan<byte> GetBlob(ReadOnlySpan<byte> buffer, int index)
    {
        var size = GetInt(buffer, index);
        return buffer.Slice(index + sizeof(int), size);
    }

    public static ulong GetULong(ReadOnlySpan<byte> buffer, int index)
    {
        Span<byte> value = stackalloc byte[sizeof(ulong)];
        value[7] = buffer[index + 0];
        value[6] = buffer[index + 1];
        value[5] = buffer[index + 2];
        value[4] = buffer[index + 3];
        value[3] = buffer[index + 4];
        value[2] = buffer[index + 5];
        value[1] = buffer[index + 6];
        value[0] = buffer[index + 7];

        return BitConverter.ToUInt64(value);
    }

    public static long GetLong(ReadOnlySpan<byte> buffer, int index)
    {
        Span<byte> value = stackalloc byte[sizeof(long)];
        value[7] = buffer[index + 0];
        value[6] = buffer[index + 1];
        value[5] = buffer[index + 2];
        value[4] = buffer[index + 3];
        value[3] = buffer[index + 4];
        value[2] = buffer[index + 5];
        value[1] = buffer[index + 6];
        value[0] = buffer[index + 7];

        return BitConverter.ToInt64(value);
    }

    public static double GetDouble(ReadOnlySpan<byte> buffer, int index)
    {
        Span<byte> value = stackalloc byte[sizeof(double)];
        value[7] = buffer[index + 0];
        value[6] = buffer[index + 1];
        value[5] = buffer[index + 2];
        value[4] = buffer[index + 3];
        value[3] = buffer[index + 4];
        value[2] = buffer[index + 5];
        value[1] = buffer[index + 6];
        value[0] = buffer[index + 7];

        return BitConverter.ToDouble(value);
    }

    public static char GetChar(ReadOnlySpan<byte> buffer, int index)
    {
        return buffer[index + 0] != 0 ||
               buffer[index + 1] != 0 ||
               buffer[index + 2] != 0
            ? throw new Exception()
            : (char)buffer[index + 3];
    }
}
