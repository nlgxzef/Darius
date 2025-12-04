using CoreExtensions.Conversions;
using CoreExtensions.IO;
using Nikki.Reflection.Abstract;
using Nikki.Reflection.Attributes;
using Nikki.Support.Undercover.Parts.BoundParts;
using Nikki.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace Nikki.Support.Undercover.Parts.SkinParts
{

    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable()]
    public class Livery : SubPart
    {

        [AccessModifiable()]
        [Category("Main")]
        public string LiveryName { get; set; }

        [Category("Main")]
        [TypeConverter(typeof(HexConverter))]
        public uint LiveryKey => this.LiveryName.BinHash();


        [AccessModifiable()]
        [Category("Primary")]
        public string SkinName { get; set; }

        [Category("Primary")]
        [TypeConverter(typeof(HexConverter))]
        public uint SkinKey => this.SkinName.BinHash();

        [AccessModifiable()]
        [Category("Primary")]
        public string VectorName { get; set; }

        [Category("Primary")]
        [TypeConverter(typeof(HexConverter))]
        public uint VectorKey => this.VectorName.BinHash();

        [AccessModifiable()]
        [Category("Primary")]
        public uint Unknown1 { get; set; }

        [AccessModifiable()]
        [Category("Primary")]
        public uint Unknown2 { get; set; }

        public Livery() { }
        public Livery(BinaryReader br)
        {
            this.LiveryName = br.ReadUInt32().BinString(LookupReturn.EMPTY);
            this.SkinName = br.ReadUInt32().BinString(LookupReturn.EMPTY);
            this.VectorName = br.ReadUInt32().BinString(LookupReturn.EMPTY);
            this.Unknown1 = br.ReadUInt32();
            this.Unknown2 = br.ReadUInt32();
        }


        public override SubPart PlainCopy()
        {
            return new Livery()
            {
                LiveryName = this.LiveryName,
                SkinName = this.SkinName,
                VectorName = this.VectorName,
                Unknown1 = this.Unknown1,
                Unknown2 = this.Unknown2
            };
        }
        public SubPart PlainCopy(string toReplace, string replacement)
        {
            return new Livery()
            {
                LiveryName = this.LiveryName.Replace(toReplace, replacement),
                SkinName = this.SkinName.Replace(toReplace, replacement),
                VectorName = this.VectorName.Replace(toReplace, replacement),
                Unknown1 = this.Unknown1,
                Unknown2 = this.Unknown2
            };
        }

        public override string ToString()
        {
            return LiveryName;
        }

        internal void Write(BinaryWriter writer)
        {
            writer.WriteNullTermUTF8(this.LiveryName);
            writer.WriteNullTermUTF8(this.SkinName);
            writer.WriteNullTermUTF8(this.VectorName);
            writer.Write(this.Unknown1);
            writer.Write(this.Unknown2);
        }

        internal void Read(BinaryReader reader)
        {
            this.LiveryName = reader.ReadNullTermUTF8();
            this.SkinName = reader.ReadNullTermUTF8();
            this.VectorName = reader.ReadNullTermUTF8();
            this.Unknown1 = reader.ReadUInt32();
            this.Unknown2 = reader.ReadUInt32();
        }
    }
}