using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using Nikki.Utils;
using Nikki.Reflection.Enum;
using Nikki.Reflection.Abstract;
using Nikki.Support.Shared.Class;
using CoreExtensions.IO;
using CoreExtensions.Conversions;



namespace Nikki.Support.Shared.Parts.TPKParts
{
	/// <summary>
	/// A unit animation block for <see cref="TPKBlock"/>.
	/// </summary>
	public class OldAnimSlot : SubPart
	{
		private string _name = String.Empty;
		public const int size = 0x34;

        /// <summary>
        /// Name of this <see cref="OldAnimSlot"/>.
        /// </summary>
        [Category("Main")]
		public string Name
		{
			get => this._name;
			set
			{
                if (String.IsNullOrWhiteSpace(value))
                {

                    throw new ArgumentNullException("This value cannot be left empty.");

                }

                if (value.Contains(" "))
                {

                    throw new Exception("Name cannot contain whitespace.");


                }

                this._name = value;
				this.BinKey = value.BinHash();
			}
		}

		/// <summary>
		/// Binary memory hash of the name.
		/// </summary>
		[Category("Main")]
		[ReadOnly(true)]
		[TypeConverter(typeof(HexConverter))]
		public uint BinKey { get; set; }

		/// <summary>
		/// Number of frames shown per second.
		/// </summary>
		[Category("Primary")]
		public int FramesPerSecond { get; set; }

		/// <summary>
		/// Time base of this <see cref="OldAnimSlot"/>.
		/// </summary>
		[Category("Primary")]
		public int TimeBase { get; set; }
		
		/// <summary>
		/// Frame textures of this <see cref="OldAnimSlot"/>.
		/// </summary>
		[Category("Primary")]
		public List<FrameEntry> FrameTextures { get; }

		/// <summary>
		/// Initializes new instance of <see cref="OldAnimSlot"/>.
		/// </summary>
		public OldAnimSlot() => this.FrameTextures = new List<FrameEntry>();

		/// <summary>
		/// Creates a plain copy of the objects that contains same values.
		/// </summary>
		/// <returns>Exact plain copy of the object.</returns>
		public override SubPart PlainCopy()
		{
			var result = new OldAnimSlot()
			{
				FramesPerSecond = this.FramesPerSecond,
				TimeBase = this.TimeBase,
				Name = this.Name
			};

			foreach (var entry in this.FrameTextures)
			{

				result.FrameTextures.Add(new FrameEntry() { Name = entry.Name });

			}

			return result;
		}

		/// <summary>
		/// Clones values of another <see cref="SubPart"/>.
		/// </summary>
		/// <param name="other"><see cref="SubPart"/> to clone.</param>
		public override void CloneValuesFrom(SubPart other)
		{
			if (other is OldAnimSlot anim)
			{

				this.FramesPerSecond = anim.FramesPerSecond;
				this.TimeBase = anim.TimeBase;
				this.Name = anim.Name;
				this.FrameTextures.Capacity = anim.FrameTextures.Capacity;

				foreach (var entry in anim.FrameTextures)
				{

					this.FrameTextures.Add(new FrameEntry() { Name = entry.Name });

				}

			}
		}

		/// <summary>
		/// Reads data using <see cref="BinaryReader"/> provided.
		/// </summary>
		/// <param name="br"><see cref="BinaryReader"/> to read data with.</param>
		public void Read(BinaryReader br)
		{
			var pos = br.BaseStream.Position;

            this._name = br.ReadNullTermUTF8(0x18);
			this.BinKey = br.ReadUInt32();

			this.FrameTextures.Capacity = br.ReadInt32();
			this.FramesPerSecond = br.ReadInt32();
			this.TimeBase = br.ReadInt32();

			br.BaseStream.Position = pos + size;

            // Frame textures get read in TPKAnimation
        }

        /// <summary>
        /// Writes data using <see cref="BinaryWriter"/> provided.
        /// </summary>
        /// <param name="bw"><see cref="BinaryWriter"/> to write data with.</param>
        public void Write(BinaryWriter bw)
		{
			bw.WriteNullTermUTF8(this._name, 0x18);
			bw.Write(this.BinKey);
			bw.Write(this.FrameTextures.Count);
			bw.Write(this.FramesPerSecond);
			bw.Write(this.TimeBase);
            bw.Write(0); // TextureAnimTable
            bw.Write(0); // Valid
            bw.Write(0); // CurrentFrame

            // Frame textures get written in TPKAnimation
        }

        /// <summary>
        /// Reads serialized data using <see cref="BinaryReader"/> provided.
        /// </summary>
        /// <param name="br"><see cref="BinaryReader"/> to read data with.</param>
        public void ReadSerialized(BinaryReader br)
		{
            this._name = br.ReadNullTermUTF8();
            var count = br.ReadInt32();
            this.FrameTextures.Capacity = count;
            this.FramesPerSecond = br.ReadInt32();
            this.TimeBase = br.ReadInt32();

            // Read FrameTextures
            for (int loop = 0; loop < count; ++loop)
            {
                var entry = new FrameEntry()
                {
                    Name = br.ReadUInt32().BinString(LookupReturn.EMPTY)
                };
                this.FrameTextures.Add(entry);
            }
        }

        /// <summary>
        /// Writes serialized data using <see cref="BinaryWriter"/> provided.
        /// </summary>
        /// <param name="bw"><see cref="BinaryWriter"/> to write data with.</param>
        public void WriteSerialized(BinaryWriter bw)
		{
            bw.WriteNullTermUTF8(this._name);
            bw.Write(this.FrameTextures.Count);
            bw.Write(this.FramesPerSecond);
            bw.Write(this.TimeBase);

            // Write FrameTextures
            for (int loop = 0; loop < this.FrameTextures.Count; ++loop)
            {
                bw.Write(this.FrameTextures[loop].BinKey);
            }
        }

        /// <summary>
        /// Size of the animation slot.
        /// </summary>
        /// <returns>Size of the slot.</returns>
		public int GetSerializedSize() => this._name.Length + 12 + this.FrameTextures.Count * 4;

        /// <summary>
        /// Name of the animation slot.
        /// </summary>
        /// <returns>Name of the slot as a string value.</returns>
        public override string ToString() => this.Name;
	}
}
