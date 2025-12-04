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
    public class Skin : SubPart
    {

        [AccessModifiable()]
        [Category("Main")]
        public string Name { get; set; }

        [Category("Main")]
        [TypeConverter(typeof(HexConverter))]
        public uint Key => this.Name.BinHash();

        [AccessModifiable()]
        [Category("Primary")]
        public int NumberOfTransforms
        {
            get => this.Transforms.Count;
            set => this.Transforms.Resize(value);
        }

        [AccessModifiable()]
        [Category("Primary")]
        [TypeConverter(typeof(ExpandableListConverter<Transform>))]
        public List<Transform> Transforms { get; }

        public Skin()
        {
            this.Transforms = new List<Transform>();
        }

        public Skin(UInt32 NameHash)
        {
            this.Name = NameHash.BinString(LookupReturn.EMPTY);
            this.Transforms = new List<Transform>();
        }

        public override SubPart PlainCopy()
        {
            var ret = new Skin() { Name = this.Name };
            foreach (Transform t in this.Transforms) ret.Transforms.Add((Transform)t.PlainCopy());
            return ret;
        }

        public SubPart PlainCopy(string toReplace, string replacement)
        {
            var ret = new Skin() { Name = this.Name.Replace(toReplace, replacement) };
            foreach (Transform t in this.Transforms) ret.Transforms.Add((Transform)t.PlainCopy(toReplace, replacement));
            return ret;
        }

        public override string ToString()
        {
            return Name;
        }

        internal void Write(BinaryWriter writer)
        {
            writer.WriteNullTermUTF8(this.Name);
            writer.Write(this.NumberOfTransforms);

            foreach (var item in this.Transforms) item.Write(writer);
        }

        internal void Read(BinaryReader reader)
        {
            this.Name = reader.ReadNullTermUTF8();
            this.NumberOfTransforms = reader.ReadInt32();

            for (int loop = 0; loop < this.NumberOfTransforms; ++loop) this.Transforms[loop].Read(reader);
        }
    }
}