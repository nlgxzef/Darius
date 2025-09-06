using CoreExtensions.Conversions;
using Nikki.Core;
using Nikki.Reflection.Abstract;
using Nikki.Reflection.Interface;
using Nikki.Support.Shared.Parts.TPKParts;
using Nikki.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Nikki.Support.Shared.Class
{
    public abstract class TPKAnimation : Collectable, IAssembly
    {
        #region Main Properties

        /// <summary>
        /// Collection name of the variable.
        /// </summary>
        public override string CollectionName { get; set; }

        /// <summary>
        /// Game to which the class belongs to.
        /// </summary>
        public override GameINT GameINT => GameINT.None;

        /// <summary>
        /// Game string to which the class belongs to.
        /// </summary>
        public override string GameSTR => GameINT.None.ToString();

        /// <summary>
        /// Animation slots of this <see cref="TPKAnimation"/>.
        /// </summary>
        [Category("Primary")]
        public abstract List<OldAnimSlot> AnimSlots { get; }

        /// <summary>
        /// Number of animation slots of this <see cref="TPKAnimation"/>.
        /// </summary>
        [Category("Primary")]
        public int AnimSlotCount => this.AnimSlots.Count;

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
        /// Assembles <see cref="TPKBlock"/> into a byte array.
        /// </summary>
        /// <param name="bw"><see cref="BinaryWriter"/> to write <see cref="TPKBlock"/> with.</param>
        /// <returns>Byte array of the tpk block.</returns>
        public abstract void Assemble(BinaryWriter bw);

        /// <summary>
        /// Disassembles <see cref="TPKBlock"/> array into separate properties.
        /// </summary>
        /// <param name="br"><see cref="BinaryReader"/> to read <see cref="TPKBlock"/> with.</param>
        public abstract void Disassemble(BinaryReader br);

        /// <summary>
        /// Serializes instance into a byte array and stores it in the file provided.
        /// </summary>
        /// <param name="bw"><see cref="BinaryWriter"/> to write data with.</param>
        public abstract void Serialize(BinaryWriter bw);

        /// <summary>
        /// Deserializes byte array into an instance by loading data from the file provided.
        /// </summary>
		/// <param name="br"><see cref="BinaryReader"/> to read data with.</param>
        public abstract void Deserialize(BinaryReader br);

        #endregion
    }
}
