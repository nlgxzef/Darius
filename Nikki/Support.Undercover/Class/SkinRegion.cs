using CoreExtensions.Conversions;
using CoreExtensions.IO;
using Nikki.Core;
using Nikki.Reflection.Abstract;
using Nikki.Reflection.Attributes;
using Nikki.Reflection.Enum;
using Nikki.Reflection.Interface;
using Nikki.Support.Undercover.Framework;
using Nikki.Support.Undercover.Parts.SkinParts;
using Nikki.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Nikki.Support.Undercover.Class
{
    public class SkinRegion : Collectable, IAssembly
    {
        #region Fields
        private string _collection_name;
        [AccessModifiable()]
        [Category("Main")]
        public override string CollectionName
        {
            get => this._collection_name;
            set
            {
                this.Manager?.CreationCheck(value);
                this._collection_name = value;
            }
        }

        [Category("Main")]
        [TypeConverter(typeof(HexConverter))]
        public uint BinKey => this._collection_name.BinHash();

        [Browsable(false)]
        public override GameINT GameINT => GameINT.Undercover;

        [Browsable(false)]
        public override string GameSTR => GameINT.Undercover.ToString();

        [Browsable(false)]
        public SkinRegionManager Manager { get; set; }


        [AccessModifiable()]
        [Category("Primary")]
        public int NumberOfSkins
        {
            get => this.Skins.Count;
            set => this.Skins.Resize(value);
        }
        [AccessModifiable()]
        [Category("Primary")]
        public int NumberOfLiveries
        {
            get => this.Liveries.Count;
            set => this.Liveries.Resize(value);
        }

        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Primary")]
        public float ScaleU { get; set; }
        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Primary")]
        public float ScaleV { get; set; }


        [Category("Secondary")]
        [TypeConverter(typeof(ExpandableListConverter<Skin>))]
        public List<Skin> Skins { get; }

        [Category("Secondary")]
        [TypeConverter(typeof(ExpandableListConverter<Livery>))]
        public List<Livery> Liveries { get; }



        #endregion

        #region Main
        public SkinRegion()
        {
            this.Skins = new List<Skin>();
            this.Liveries = new List<Livery>();
        }
        public SkinRegion(string Name, SkinRegionManager mgr)
        {
            this.Manager = mgr;
            this.CollectionName = Name;
            this.Skins = new List<Skin>();
            this.Liveries = new List<Livery>();
        }

        public SkinRegion(BinaryReader br, SkinRegionManager mgr, UInt32 xname)
        {
            this.Manager = mgr;
            this.CollectionName = xname.BinString(LookupReturn.EMPTY);
            this.ScaleU = br.ReadSingle();
            this.ScaleV = br.ReadSingle();

            this.Skins = new List<Skin>();
            this.Liveries = new List<Livery>();
        }
        #endregion

        #region Methods
        public override Collectable MemoryCast(string CName)
        {
            var result = new SkinRegion(CName, this.Manager)
            {
                ScaleU = this.ScaleU,
                ScaleV = this.ScaleV
            };
            foreach (var item in this.Skins) result.Skins.Add((Skin)item.PlainCopy(this.CollectionName, CName));
            foreach (var item in this.Liveries) result.Liveries.Add((Livery)item.PlainCopy(this.CollectionName, CName));

            return result;
        }

        public void Assemble(BinaryWriter bw) => throw new NotImplementedException();
        public void Disassemble(BinaryReader br) => throw new NotImplementedException();

        public void Serialize(BinaryWriter bw)
        {
            byte[] array;
            using (var ms = new MemoryStream(0x100))
            using (var writer = new BinaryWriter(ms))
            {

                writer.WriteNullTermUTF8(this._collection_name);
                writer.Write(this.NumberOfSkins);
                writer.Write(this.NumberOfLiveries);

                foreach (var item in this.Skins) item.Write(writer);
                foreach (var item in this.Liveries) item.Write(writer);

                writer.Write(this.ScaleU);
                writer.Write(this.ScaleV);

                array = ms.ToArray();

            }

            array = Interop.Compress(array, LZCompressionType.RAWW);

            var header = new SerializationHeader(array.Length, this.GameINT, this.Manager.Name);
            header.Write(bw);
            bw.Write(array.Length);
            bw.Write(array);
        }

        public void Deserialize(BinaryReader br)
        {
            int size = br.ReadInt32();
            var array = br.ReadBytes(size);

            array = Interop.Decompress(array);

            using var ms = new MemoryStream(array);
            using var reader = new BinaryReader(ms);

            this._collection_name = reader.ReadNullTermUTF8();
            this.NumberOfSkins = reader.ReadInt32();
            this.NumberOfLiveries = reader.ReadInt32();

            for (int loop = 0; loop < this.NumberOfSkins; ++loop) this.Skins[loop].Read(reader);
            for (int loop = 0; loop < this.NumberOfLiveries; ++loop) this.Liveries[loop].Read(reader);

            this.ScaleU = reader.ReadSingle();
            this.ScaleV = reader.ReadSingle();
        }

        public override string ToString()
        {
            return $"Collection Name: {this.CollectionName} | " +
                   $"BinKey: {this.BinKey:X8} | Game: {this.GameSTR}";
        }
        #endregion

    }
}