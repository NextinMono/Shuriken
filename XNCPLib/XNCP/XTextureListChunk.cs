﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;
using Amicitia.IO.Binary.Extensions;
using XNCPLib.Extensions;
using XNCPLib.Misc;

namespace XNCPLib.XNCP
{
    public class XTextureListChunk
    {
        public uint Signature { get; set; }
        public uint Field0C { get; set; }
        public List<XTexture> Textures { get; set; }

        public XTextureListChunk()
        {
            Textures = new List<XTexture>();
        }

        public void Read(BinaryObjectReader reader)
        {
            reader.PushOffsetOrigin();
            Endianness endianPrev = reader.Endianness;

            // Header is always little endian
            uint size;
            reader.Endianness = Endianness.Little;
            {
                Signature = reader.ReadUInt32();
                size = reader.ReadUInt32();
            }
            reader.Endianness = endianPrev;

            uint listOffset = reader.ReadUInt32();
            Field0C = reader.ReadUInt32();
            uint textureCount = reader.ReadUInt32();
            uint texturesOffset = reader.ReadUInt32();
            uint dataOffset = texturesOffset > 24 ? reader.ReadUInt32() : 0;

            reader.Seek(reader.GetOffsetOrigin() + texturesOffset, SeekOrigin.Begin);
            for (int i = 0; i < textureCount; ++i)
            {
                XTexture texture = new XTexture();
                texture.Read(reader);

                if (string.IsNullOrEmpty(texture.Name))
                    texture.Name = $"Texture_{i}";

                Textures.Add(texture);
            }

            if (dataOffset > 0)
            {
                for (int i = 0; i < textureCount; ++i)
                {
                    reader.Seek(reader.GetOffsetOrigin() + dataOffset + i * 8, SeekOrigin.Begin);
                    int length = reader.ReadInt32();
                    uint offset = reader.ReadUInt32();

                    reader.Seek(reader.GetOffsetOrigin() + offset, SeekOrigin.Begin);
                    Textures[i].Data = reader.ReadArray<byte>(length);
                }
            }
            
            reader.PopOffsetOrigin();
        }

        public void Write(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            writer.PushOffsetOrigin();
            Endianness endianPrev = writer.Endianness;

            // Header is always little endian
            writer.Endianness = Endianness.Little;
            {
                writer.WriteUInt32(Signature);

                // Skipped: size
                writer.Skip(4);
            }
            writer.Endianness = endianPrev;
            uint dataStart = (uint)writer.Position;

            // Is this always just 0x10?
            writer.WriteUInt32(0x10);
            writer.WriteUInt32(Field0C);

            writer.WriteUInt32((uint)Textures.Count);

            // DataOffset is always just 0x18?
            offsetChunk.Add(writer);
            writer.WriteUInt32(0x18);

            uint textureDataStart = (uint)writer.Length;
            Utilities.PadZeroBytes(writer, Textures.Count * 0x8);
            for (int i = 0; i < Textures.Count; ++i)
            {
                writer.Seek(textureDataStart + (i * 0x8), SeekOrigin.Begin);

                offsetChunk.Add(writer);
                uint textureNameOffset = (uint)(writer.Length - writer.GetOffsetOrigin());
                Textures[i].Write(writer, textureNameOffset);

                // Align to 4 bytes if the texture name wasn't
                writer.Seek(0, SeekOrigin.End);
                writer.Align(4);
            }

            // Go back and write size
            writer.Endianness = Endianness.Little;
            {
                // It looks like it always tries to align to 32-bit
                writer.Seek(0, SeekOrigin.End);
                while ((writer.Length - writer.GetOffsetOrigin()) % 0x10 != 0)
                {
                    writer.WriteByte(0x00);
                }

                writer.Seek(writer.GetOffsetOrigin() + 4, SeekOrigin.Begin);
                writer.WriteUInt32((uint)(writer.Length - dataStart));
                writer.Seek(0, SeekOrigin.End);
            }
            writer.Endianness = endianPrev;

            writer.PopOffsetOrigin();
        }
    }
}
