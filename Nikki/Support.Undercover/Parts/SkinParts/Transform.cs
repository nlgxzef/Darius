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
    public class Transform : SubPart
    {

        [AccessModifiable()]
        [Category("Main")]
        public string Name { get; set; }

        [Category("Main")]
        [TypeConverter(typeof(HexConverter))]
        public uint Key => this.Name.BinHash();


        [AccessModifiable()]
        [Category("Primary")]
        public float TranslateU { get; set; }

        [AccessModifiable()]
        [Category("Primary")]
        public float TranslateV { get; set; }

        [AccessModifiable()]
        [Category("Primary")]
        public float Scale1 { get; set; }

        [AccessModifiable()]
        [Category("Primary")]
        public float Scale2 { get; set; }

        internal float _rotation;
        [AccessModifiable()]
        [Category("Primary")]
        public float Rotation
        {
            get => (float)(this._rotation * 180.0 / Math.PI);
            set => this._rotation = (float)(value * Math.PI / 180.0);
        }


        public Transform() { }
        public Transform(BinaryReader br)
        {
            this.Name = br.ReadUInt32().BinString(LookupReturn.EMPTY);
            this.TranslateU = br.ReadSingle();
            this.TranslateV = br.ReadSingle();
            this.Scale1 = br.ReadSingle();
            this.Scale2 = br.ReadSingle();
            this._rotation = br.ReadSingle();
        }



        public override SubPart PlainCopy()
        {
            return new Transform()
            {
                Name = this.Name,
                TranslateU = this.TranslateU,
                TranslateV = this.TranslateV,
                Scale1 = this.Scale1,
                Scale2 = this.Scale2,
                _rotation = this._rotation
            };
        }

        public SubPart PlainCopy(string toReplace, string replacement)
        {
            return new Transform()
            {
                Name = this.Name.Replace(toReplace, replacement),
                TranslateU = this.TranslateU,
                TranslateV = this.TranslateV,
                Scale1 = this.Scale1,
                Scale2 = this.Scale2,
                _rotation = this._rotation
            };
        }

        public override string ToString()
        {
            return Name;
        }

        internal void Write(BinaryWriter writer)
        {
            writer.WriteNullTermUTF8(this.Name);
            writer.Write(this.TranslateU);
            writer.Write(this.TranslateV);
            writer.Write(this.Scale1);
            writer.Write(this.Scale2);
            writer.Write(this._rotation);
        }

        internal void Read(BinaryReader reader)
        {
            this.Name = reader.ReadNullTermUTF8();
            this.TranslateU = reader.ReadSingle();
            this.TranslateV = reader.ReadSingle();
            this.Scale1 = reader.ReadSingle();
            this.Scale2 = reader.ReadSingle();
            this._rotation = reader.ReadSingle();
        }
    }
}