namespace SharpOSC;

using System;

public interface IOscPacket
{
    public const int Padding = 4;

    public static IOscPacket Deserialize(ReadOnlySpan<byte> buffer)
    {
        return buffer[0] == '#' 
            ? OscBundle.Deserialize(buffer) 
            : OscMessage.Deserialize(buffer);
    }

    public abstract byte[] Serialize();
}
