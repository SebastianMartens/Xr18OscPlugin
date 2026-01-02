namespace SharpOSC;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

public static class Serializer
{
    static void AddPadding(int size, List<byte> buffer)
    {
        for (var i = 0; i < size; i++) buffer.Add(0);
    }

    public static void SetInt(int value, List<byte> buffer)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        Unsafe.As<byte, int>(ref bytes[0]) = value;

        buffer.Add(bytes[3]);
        buffer.Add(bytes[2]);
        buffer.Add(bytes[1]);
        buffer.Add(bytes[0]);
    }

    public static void SetFloat(float value, List<byte> buffer)
    {
        Span<byte> bytes = stackalloc byte[sizeof(float)];
        Unsafe.As<byte, float>(ref bytes[0]) = value;

        buffer.Add(bytes[3]);
        buffer.Add(bytes[2]);
        buffer.Add(bytes[1]);
        buffer.Add(bytes[0]);
    }

    public static void SetString(string value, List<byte> buffer)
    {
        var len = Utils.AlignedStringLength(value);

        Span<byte> bytes = stackalloc byte[len];
        Encoding.ASCII.GetBytes(value, bytes);
        buffer.AddRange(bytes);
    }

    public static void SetBlob(ReadOnlySpan<byte> value, List<byte> buffer)
    {
        var len = value.Length + sizeof(int);

        SetInt(value.Length, buffer);
        buffer.AddRange(value);

        AddPadding(IOscPacket.Padding - len % IOscPacket.Padding, buffer);
    }

    public static void SetLong(long value, List<byte> buffer)
    {
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        Unsafe.As<byte, long>(ref bytes[0]) = value;

        buffer.Add(bytes[7]);
        buffer.Add(bytes[6]);
        buffer.Add(bytes[5]);
        buffer.Add(bytes[4]);
        buffer.Add(bytes[3]);
        buffer.Add(bytes[2]);
        buffer.Add(bytes[1]);
        buffer.Add(bytes[0]);
    }

    public static void SetULong(ulong value, List<byte> buffer)
    {
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        Unsafe.As<byte, ulong>(ref bytes[0]) = value;

        buffer.Add(bytes[7]);
        buffer.Add(bytes[6]);
        buffer.Add(bytes[5]);
        buffer.Add(bytes[4]);
        buffer.Add(bytes[3]);
        buffer.Add(bytes[2]);
        buffer.Add(bytes[1]);
        buffer.Add(bytes[0]);
    }

    public static void SetDouble(double value, List<byte> buffer)
    {
        Span<byte> bytes = stackalloc byte[sizeof(double)];
        Unsafe.As<byte, double>(ref bytes[0]) = value;

        buffer.Add(bytes[7]);
        buffer.Add(bytes[6]);
        buffer.Add(bytes[5]);
        buffer.Add(bytes[4]);
        buffer.Add(bytes[3]);
        buffer.Add(bytes[2]);
        buffer.Add(bytes[1]);
        buffer.Add(bytes[0]);
    }

    public static void SetChar(char value, List<byte> buffer)
    {
        buffer.Add(0);
        buffer.Add(0);
        buffer.Add(0);
        buffer.Add((byte)value);
    }
}
