using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

public static class BinaryHelper
{
    public static readonly Endianness PlatformEndianness =
        BitConverter.IsLittleEndian ? Endianness.Little : Endianness.Big;

    public static int GetOffsetSize(this BinaryObjectReader reader)
    {
        switch (reader.OffsetBinaryFormat)
        {
            case OffsetBinaryFormat.U32:
                return 4;

            case OffsetBinaryFormat.U64:
                return 8;
        }

        throw new NotImplementedException();
    }
    public static string ReadStringOffset(this BinaryObjectReader reader, StringBinaryFormat format = StringBinaryFormat.NullTerminated, int fixedLength = -1)
    {
        var offset = reader.ReadOffsetValue();
        if (offset == 0)
            return null;

        using var token = reader.AtOffset(offset);
        return reader.ReadString(format, fixedLength);
    }

    public static SeekToken At(this BinaryValueReader reader)
        => new SeekToken(reader.GetBaseStream(), reader.Position, SeekOrigin.Begin);

    public static SeekToken At(this BinaryValueWriter reader)
        => new SeekToken(reader.GetBaseStream(), reader.Position, SeekOrigin.Begin);

}