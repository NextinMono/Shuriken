using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;
using XNCPLib.Extensions;
using XNCPLib.Misc;

namespace XNCPLib.XNCP
{
    public class XTexture
    {

        public string Name { get; set; }
        public uint Field04 { get; set; }
        public uint Field00 { get; set; }
        public ushort Field08 { get; set; } = 5;
        public ushort Field0A { get; set; } = 1;
        public uint Field0C { get; set; }
        public uint Field10 { get; set; }
        public byte[] Data { get; set; }

        public XTexture()
        {

        }

        public void Read(BinaryObjectReader reader, uint offset = 0)
        {
            uint nameOffset = reader.ReadUInt32();
            if (offset != 0) nameOffset = offset;
            Field04 = reader.ReadUInt32();
            Name = reader.ReadStringOffset(nameOffset);
            
        }
        public void ReadNN(BinaryObjectReader reader)
        {
            Field00 = reader.Read<uint>();
            Name = reader.ReadStringOffset();
            Field08 = reader.Read<ushort>();
            Field0A = reader.Read<ushort>();
            Field0C = reader.Read<uint>();
            Field10 = reader.Read<uint>();

        }

        public void Write(BinaryObjectWriter writer, uint nameOffset)
        {
            writer.WriteUInt32(nameOffset);
            writer.WriteStringOffset(nameOffset, Name);
            writer.WriteUInt32(Field04);
        }
        public void WriteNN(BinaryObjectWriter writer, uint nameOffset)
        {
            writer.Write(Field00);
            writer.WriteStringOffset(nameOffset-1, Name);
            writer.Write(Field08);
            writer.Write(Field0A);
            writer.Write(Field0C);
            writer.Write(Field10);
        }

        
    }
}
