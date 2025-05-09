﻿using System.Runtime.CompilerServices;

namespace DNS.Protocol.Utils;

public static class ByteExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static byte GetBitValueAt(this byte b, byte offset, byte length)
    {
        return (byte)((b >> offset) & ~(0xff << length));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static byte GetBitValueAt(this byte b, byte offset)
    {
        return b.GetBitValueAt(offset, 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static byte SetBitValueAt(this byte b, byte offset, byte length, byte value)
    {
        int mask = ~(0xff << length);
        value = (byte)(value & mask);

        return (byte)((value << offset) | (b & ~(mask << offset)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static byte SetBitValueAt(this byte b, byte offset, byte value)
    {
        return b.SetBitValueAt(offset, 1, value);
    }
}
