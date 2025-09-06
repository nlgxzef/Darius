using CoreExtensions.Conversions;
using CoreExtensions.IO;
using Nikki.Core;
using Nikki.Reflection.Abstract;
using Nikki.Reflection.Attributes;
using Nikki.Reflection.Enum;
using Nikki.Reflection.Exception;
using Nikki.Support.Underground1.Framework;
using Nikki.Support.Shared.Parts.STRParts;
using Nikki.Support.Shared.Parts.TPKParts;
using Nikki.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Nikki.Support.Underground1.Class
{
    public class TPKAnimation : Shared.Class.TPKAnimation
    {
        #region Fields
        
        private string _collection_name;
        private int _version;
        private List<OldAnimSlot> _anim_slots;

        #endregion

        #region Properties

        /// <summary>
        /// Game to which the class belongs to.
        /// </summary>
        [Browsable(false)]
        public override GameINT GameINT => GameINT.Underground1;

        /// <summary>
        /// Game string to which the class belongs to.
        /// </summary>
        [Browsable(false)]
        public override string GameSTR => GameINT.Underground1.ToString();

        /// <summary>
        /// Animation slots of this <see cref="TPKAnimation"/>.
        /// </summary>
        [Category("Primary")]
        public override List<OldAnimSlot> AnimSlots => this._anim_slots;


        #endregion

        #region Main

        /// <summary>
        /// Initializes new instance of <see cref="TPKAnimation"/>.
        /// </summary>
        public TPKAnimation()
        {
            this._version = 1;
            this._anim_slots = new List<OldAnimSlot>();
        }

        /// <summary>
        /// Initializes new instance of <see cref="TPKAnimation"/>.
        /// </summary>
        /// <param name="CName">CollectionName of the new instance.</param>
        public TPKAnimation(string CName) : this()
        {
            this._collection_name = CName;
        }

        /// <summary>
        /// Initializes new instance of <see cref="TPKAnimation"/>.
        /// </summary>
        /// <param name="br"><see cref="BinaryReader"/> to read text data with.</param>
        public TPKAnimation(BinaryReader br) : this()
        {
            this.Disassemble(br);
        }

        #endregion


        #region Methods

        /// <summary>
        /// Casts all attributes from this object to another one.
        /// </summary>
        /// <param name="CName">CollectionName of the new created object.</param>
        /// <returns>Memory casted copy of the object.</returns>
        public override Collectable MemoryCast(string CName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Assembles <see cref="TPKAnimation"/> into a byte array.
        /// </summary>
        /// <param name="bw"><see cref="BinaryWriter"/> to write <see cref="TPKAnimation"/> with.</param>
        /// <returns>Byte array of the tpk block.</returns>
        public override void Assemble(BinaryWriter bw)
        {
            // Write ID and temporary size
            bw.WriteEnum(BinBlockID.OldAnimationPack);
            bw.Write(-1);

            // Save position
            var position = bw.BaseStream.Position;

            // Part 1 (TextureAnimPackHeader)
            bw.WriteEnum(BinBlockID.OldAnimationPackPart1);
            bw.Write(0x10); // size
            bw.Write(this._version); // Version
            bw.Write(0); // pTextureAnimPack
            bw.Write(0); // EndianSwapped
            bw.Write(0); // pad

            // Part 2 (TextureAnim)
            bw.WriteEnum(BinBlockID.OldAnimationPackPart2);
            var size = this._anim_slots.Count * OldAnimSlot.size;
            bw.Write(size); // size

            var numtex = 0;
            for (int loop = 0; loop < this._anim_slots.Count; ++loop)
            {
                this._anim_slots[loop].Write(bw); // Write slot
                numtex += this._anim_slots[loop].FrameTextures.Count; // Count total number of textures
            }

            // Part 3 (TextureAnimEntry)
            bw.WriteEnum(BinBlockID.OldAnimationPackPart3);
            bw.Write(numtex * 0x10); // size

            for (int loop = 0; loop < this._anim_slots.Count; ++loop)
            {
                for (int loop2 = 0; loop2 < this._anim_slots[loop].FrameTextures.Count; ++loop2)
                {
                    bw.Write(this._anim_slots[loop].FrameTextures[loop2].BinKey);
                    bw.Write(0); // TextureInfo
                    bw.Write(0); // pPlatAnimData
                    bw.Write(0); // pad
                }
            }

            // Write size
            size = (int)(bw.BaseStream.Position - position);
            bw.BaseStream.Position -= size + 4;
            bw.Write(size);

            // Go to end
            bw.BaseStream.Position = position + size;

        }

        /// <summary>
        /// Disassembles <see cref="TPKAnimation"/> array into separate properties.
        /// </summary>
        /// <param name="br"><see cref="BinaryReader"/> to read <see cref="TPKAnimation"/> with.</param>
        public override void Disassemble(BinaryReader br)
        {
            var Start = br.BaseStream.Position;
            var ID = br.ReadEnum<BinBlockID>();
            int size = br.ReadInt32();
            var Final = br.BaseStream.Position + size;

            // Part 1 (TextureAnimPackHeader (skip))
            ID = br.ReadEnum<BinBlockID>();
            size = br.ReadInt32();
            var pos = br.BaseStream.Position;
            this._version = br.ReadInt32();
            // pTextureAnimPack
            // EndianSwapped
            // pad

            br.BaseStream.Position = pos + size;

            // Part 2 (TextureAnim)
            ID = br.ReadEnum<BinBlockID>();
            size = br.ReadInt32();

            this._anim_slots.Capacity = size / OldAnimSlot.size;

            for (int loop = 0; loop < this._anim_slots.Capacity; ++loop)
            {
                var slot = new OldAnimSlot();
                slot.Read(br);
                this._anim_slots.Add(slot);
            }

            // Part 3 (TextureAnimEntry)
            ID = br.ReadEnum<BinBlockID>();
            size = br.ReadInt32() / 0x10;

            for (int loop = 0, slot = 0, count = 0;
                loop < size && slot < this._anim_slots.Count;
                ++loop, ++count) // Total number of entries
            {
                // Read the entry first
                var entry = new FrameEntry()
                {
                    Name = br.ReadUInt32().BinString(LookupReturn.EMPTY)
                    // TextureInfo
                    // pPlatAnimData
                    // pad
                };
                br.BaseStream.Position += 12;

                // Calculate which slot to add to
                if (count >= this._anim_slots[slot].FrameTextures.Capacity)
                {
                    slot++;
                    count = 0;
                    if (slot >= this._anim_slots.Count) break;
                }

                // Add to the correct slot
                this._anim_slots[slot].FrameTextures.Add(entry);
                
            }

            // Set position to end
            br.BaseStream.Position = Final;

        }

        /// <summary>
        /// Serializes instance into a byte array and stores it in the file provided.
        /// </summary>
        /// <param name="bw"><see cref="BinaryWriter"/> to write data with.</param>
        public override void Serialize(BinaryWriter bw)
        {
            byte[] array;
            var size = 4; // For count

            // Precalculate size
            for (int loop = 0; loop < this._anim_slots.Count; ++loop)
            {
                size += this._anim_slots[loop].GetSerializedSize();
            }

            using (var ms = new MemoryStream(size))
            {
                using (var writer = new BinaryWriter(ms))
                {
                    var ct = this._anim_slots.Count;
                    writer.Write(ct);

                    for (int loop = 0; loop < this._anim_slots.Count; ++loop)
                    {
                        this._anim_slots[loop].WriteSerialized(writer);
                    }

                    // Get array
                    array = ms.ToArray();
                }
            }

            array = Interop.Compress(array, LZCompressionType.RAWW);

            var header = new SerializationHeader(array.Length, this.GameINT, String.Empty);
            header.Write(bw);
            bw.Write(array.Length);
            bw.Write(array);
        }

        /// <summary>
        /// Deserializes byte array into an instance by loading data from the file provided.
        /// </summary>
		/// <param name="br"><see cref="BinaryReader"/> to read data with.</param>
        public override void Deserialize(BinaryReader br)
        {
            int size = br.ReadInt32();
            var array = br.ReadBytes(size);

            array = Interop.Decompress(array);

            using var ms = new MemoryStream(array);
            using var reader = new BinaryReader(ms);
            
            var ct = reader.ReadInt32();

            for (int loop = 0; loop < ct; ++loop)
            {
                var slot = new OldAnimSlot();
                slot.ReadSerialized(reader);
                this._anim_slots.Add(slot);
            }
        }

        /// <summary>
		/// Synchronizes all parts of this instance with another instance passed.
		/// </summary>
		/// <param name="other"><see cref="TPKAnimation"/> to synchronize with.</param>
		internal void Synchronize(TPKAnimation other)
        {
            var slots = new List<OldAnimSlot>(other._anim_slots);

            for (int i = 0; i < this._anim_slots.Count; ++i)
            {

                bool found = false;

                for (int j = 0; j < other._anim_slots.Count; ++j)
                {

                    if (other._anim_slots[j].BinKey == this._anim_slots[i].BinKey)
                    {

                        found = true;
                        break;

                    }

                }

                if (!found) slots.Add(this._anim_slots[i]);

            }

            this._anim_slots = slots;
        }

        #endregion
    }
}
