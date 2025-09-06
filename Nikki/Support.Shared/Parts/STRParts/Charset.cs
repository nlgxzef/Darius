using CoreExtensions.Text;
using Nikki.Reflection.Abstract;
using Nikki.Reflection.Exception;
using Nikki.Support.Shared.Class;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Nikki.Support.Shared.Parts.STRParts
{
    public class Charset : SubPart
    {
        /// <summary>
		/// Number of entries in this <see cref="Charset"/>.
		/// </summary>
		public int NumEntries { get; set; }

        /// <summary>
        /// List of the entries in this <see cref="Charset"/>.
        /// </summary>
        public ushort[] EntryTable { get; set; }

        /// <summary>
		/// Constant string value "NumEntries".
		/// </summary>
		public const string numEntries = "NumEntries";

        /// <summary>
		/// Initializes new instance of <see cref="Charset"/>.
		/// </summary>
		/// <param name="block"><see cref="STRBlock"/> to which this 
		/// <see cref="Charset"/> belongs to.</param>
		public Charset()
        {
            this.NumEntries = 0;
            this.EntryTable = new ushort[0xC00];
        }
        /*
        /// <summary>
		/// Determines whether this instance and a specified object, which must also be a
		/// <see cref="Charset"/> object, have the same value.
		/// </summary>
		/// <param name="obj">The <see cref="Charset"/> to compare to this instance.</param>
		/// <returns>True if obj is a <see cref="Charset"/> and its value is the same as 
		/// this instance; false otherwise. If obj is null, the method returns false.
		/// </returns>
		public override bool Equals(object obj) => obj is Charset set && this == set;

        /// <summary>
		/// Determines whether two specified <see cref="Charset"/> have the same value.
		/// </summary>
		/// <param name="s1">The first <see cref="Charset"/> to compare, or null.</param>
		/// <param name="s2">The second <see cref="Charset"/> to compare, or null.</param>
		/// <returns>True if the value of c1 is the same as the value of c2; false otherwise.</returns>
		public static bool operator ==(Charset s1, Charset s2)
        {
            if (s1 is null) return s2 is null;
            else if (s2 is null) return false;
            return s1.NumEntries == s2.NumEntries && s1.EntryTable == s2.EntryTable;
        }

        /// <summary>
        /// Determines whether two specified <see cref="Charset"/> have different values.
        /// </summary>
        /// <param name="s1">The first <see cref="Charset"/> to compare, or null.</param>
        /// <param name="s2">The second <see cref="Charset"/> to compare, or null.</param>
        /// <returns>True if the value of c1 is different from the value of c2; false otherwise.</returns>
        public static bool operator !=(Charset s1, Charset s2) => !(s1 == s2);
        */

        /// <summary>
		/// Returns number of entries of this <see cref="Charset"/> as a string.
		/// </summary>
		/// <returns>String value.</returns>
		public override string ToString()
        {
            return $"{numEntries}: {this.NumEntries}";
        }

        /// <summary>
        /// Returns total length of this <see cref="Charset"/>.
        /// </summary>
        /// <returns>Length of charset.</returns>
        public int Size()
        {
            return (sizeof(int) + EntryTable.Length * sizeof(ushort));
        }

        /// <summary>
        /// Decodes a given byte array using this <see cref="Charset"/>.
        /// </summary>
        /// <returns>Decoded string.</returns>
        public string Decode(byte[] bytes)
        {
            var bld = new StringBuilder();

            for (int i = 0; i < bytes.Length;)
            {
                char chr = Convert.ToChar(bytes[i++]);

                if (chr == 0) break;

                if (chr >= 0x80)
                {
                    char hst = Convert.ToChar(EntryTable[chr]);

                    if (hst >= 0x80)
                    {
                        chr = hst;
                    }
                    else if (hst != 0)
                    {
                        byte nxt = bytes[i++];
                        if (nxt >= 0x80) chr = Convert.ToChar(EntryTable[128 * hst - 128 + nxt]);
                    }
                    else return System.Text.Encoding.GetEncoding("ISO-8859-1").GetString(bytes); // Cannot decode the string
                }

                bld.Append(chr);
            }

            return bld.ToString();
        }

        /// <summary>
        /// Encodes a given string using this <see cref="Charset"/>.
        /// </summary>
        /// <returns>Encoded byte array.</returns>
        public byte[] Encode(string str)
        {
            var bytes = new List<byte>();

            char[] chr = str.ToCharArray();

            foreach (char c in chr)
            {
                if (c >= 0xFF80) continue; // skip the character that cannot get encoded

                char sav = c;

                if (c >= 0x80)
                {
                    int cur = 128;
                    int max = NumEntries;

                    if (NumEntries > 128)
                    {
                        while (cur < max)
                        {
                            if (EntryTable[cur] == c) break;
                            cur++;
                        }
                    }

                    if (cur >= 256)
                    {
                        if (cur != max)
                        {
                            sav = (char)128;
                            int src = 128;
                            bool update = true;

                            while (EntryTable[src] != cur >> 7)
                            {
                                sav++;
                                if (sav >= 256)
                                {
                                    update = false;
                                    break;
                                }
                                src++;
                            }

                            if (update)
                            {
                                bytes.Add(Convert.ToByte(sav));
                                bytes.Add((byte)Convert.ToSByte(cur % 128 - 128));
                            }
                        }

                        bool notFound = sav == 256 || cur == max;
                        if (notFound) continue; // skip the character that cannot get encoded
                    }
                    else bytes.Add(Convert.ToByte(cur));
                }
                else bytes.Add(Convert.ToByte(c));
            }

            // Add null character at the end
            bytes.Add((byte)0);

            return bytes.ToArray();
        }

        /// <summary>
		/// Creates a plain copy of the objects that contains same values.
		/// </summary>
		/// <returns>Exact plain copy of the object.</returns>
		public override SubPart PlainCopy()
        {
            var result = new Charset()
            {
                NumEntries = this.NumEntries,
                EntryTable = this.EntryTable
            };

            return result;
        }
    }
}
