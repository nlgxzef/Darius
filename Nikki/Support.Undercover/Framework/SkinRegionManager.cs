using CoreExtensions.IO;
using Nikki.Core;
using Nikki.Reflection.Enum;
using Nikki.Reflection.Exception;
using Nikki.Reflection.Interface;
using Nikki.Support.Undercover.Class;
using Nikki.Support.Undercover.Parts.SkinParts;
using Nikki.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Nikki.Support.Undercover.Framework
{

    public class SkinRegionManager : Manager<SkinRegion>
    {
        public override GameINT GameINT => GameINT.Undercover;

        public override string GameSTR => GameINT.Undercover.ToString();

        public override string Name => "SkinRegions";

        public override bool AllowsNoSerialization => false;

        public override bool IsReadOnly => false;

        public override Alignment Alignment { get; }

        public override Type CollectionType => typeof(SkinRegion);

        public SkinRegionManager(Datamap db) : base(db)
        {
            this.Extender = 5;
            this.Alignment = Alignment.Default;
        }


        struct AssembleSkinRegions
        {
            public uint xname;
            public int skinMin;
            public int skinMax;
            public int liveryMin;
            public int liveryMax;
            public float unknownFloat1;
            public float unknownFloat2;
        }

        struct AssembleSkins
        {
            public uint skinNameKey;
            public int transformMin;
            public int transformMax;
        }

        struct AssembleTransforms
        {
            public uint transformNameKey;
            public float unknownFloat1;
            public float unknownFloat2;
            public float unknownFloat3;
            public float unknownFloat4;
            public float unknownFloat5;
        }

        struct AssembleLiveries
        {
            public uint nameKey;
            public uint skinNameKey;
            public uint name2Key;
            public uint unknown1;
            public uint unknown2;
        }

        internal override void Assemble(BinaryWriter bw, string mark)
        {
            if (this.Count == 0) return;

            this.SortByKey();

            //prepare
            List<AssembleSkinRegions> skinRegions = new List<AssembleSkinRegions>();
            List<AssembleSkins> skins = new List<AssembleSkins>();
            List<AssembleTransforms> transforms = new List<AssembleTransforms>();
            List<AssembleLiveries> liveries = new List<AssembleLiveries>();
            foreach (var skinRegion in this)
            {
                int skinMin = skins.Count;
                int liverMin = liveries.Count;
                foreach (var sk in skinRegion.Skins)
                {
                    int transformMin = transforms.Count;
                    foreach (var tr in sk.Transforms)
                    {
                        transforms.Add(new AssembleTransforms()
                        {
                            transformNameKey = tr.Key,
                            unknownFloat1 = tr.TranslateU,
                            unknownFloat2 = tr.TranslateV,
                            unknownFloat3 = tr.Scale1,
                            unknownFloat4 = tr.Scale2,
                            unknownFloat5 = tr._rotation
                        });
                    }
                    skins.Add(new AssembleSkins()
                    {
                        skinNameKey = sk.Key,
                        transformMin = transformMin,
                        transformMax = transforms.Count
                    });
                }
                foreach (var lv in skinRegion.Liveries)
                {
                    liveries.Add(new AssembleLiveries()
                    {
                        nameKey = lv.LiveryKey,
                        skinNameKey = lv.SkinKey,
                        name2Key = lv.VectorKey,
                        unknown1 = lv.Unknown1,
                        unknown2 = lv.Unknown2
                    });
                }
                skinRegions.Add(new AssembleSkinRegions()
                {
                    xname = skinRegion.BinKey,
                    skinMin = skinMin,
                    skinMax = skins.Count,
                    liveryMin = liverMin,
                    liveryMax = liveries.Count,
                    unknownFloat1 = skinRegion.ScaleU,
                    unknownFloat2 = skinRegion.ScaleV
                });
            }


            bw.GeneratePadding(mark, this.Alignment);

            bw.WriteEnum(BinBlockID.SkinRegionDB);
            var size = 6 * 4 + skinRegions.Count * 4 * 7 + skins.Count * 4 * 3 + transforms.Count * 4 * 7 + liveries.Count * 4 * 5;
            bw.Write(size); //SIZE (this.Count * SkinRegion.BaseClassSize + 8)
            bw.Write(0x5757DA1A);
            bw.Write(skinRegions.Count);
            bw.Write(skins.Count);
            bw.Write(transforms.Count);
            bw.Write(liveries.Count);
            bw.Write(0);

            //make SkinRegions array
            foreach (var skinRegion in skinRegions)
            {
                bw.Write(skinRegion.xname);
                bw.Write(skinRegion.skinMin);
                bw.Write(skinRegion.skinMax);
                bw.Write(skinRegion.liveryMin);
                bw.Write(skinRegion.liveryMax);
                bw.Write(skinRegion.unknownFloat1);
                bw.Write(skinRegion.unknownFloat2);
            }

            //make Skins array
            foreach (var sk in skins)
            {
                bw.Write(sk.skinNameKey);
                bw.Write(sk.transformMin);
                bw.Write(sk.transformMax);
            }

            //make Transforms array
            foreach (var tr in transforms)
            {
                bw.Write(tr.transformNameKey);
                bw.Write(tr.unknownFloat1);
                bw.Write(tr.unknownFloat2);
                bw.Write(tr.unknownFloat3);
                bw.Write(tr.unknownFloat4);
                bw.Write(tr.unknownFloat5);
            }

            //[0 to transformcount]
            for (int i = 0; i < transforms.Count; i++) bw.Write(i);

            //make Liveries array
            foreach (var lv in liveries)
            {
                bw.Write(lv.nameKey);
                bw.Write(lv.skinNameKey);
                bw.Write(lv.name2Key);
                bw.Write(lv.unknown1);
                bw.Write(lv.unknown2);
            }

        }

        internal override void Disassemble(BinaryReader br, Block block)
        {
            //block at 46480016 %16 - 46485552 %16 length 5536 (UCE 1.18)
            //block at 46316848 %16 - 46322464 %32 length 5616 (UCE 1.0)

            if (Block.IsNullOrEmpty(block)) return;
            if (block.BlockID != BinBlockID.SkinRegionDB) return;

            this.Capacity = 0;
            for (int loop = 0; loop < block.Offsets.Count; ++loop)
            {

                br.BaseStream.Position = block.Offsets[loop] + 4;
                var blockSize = br.ReadInt32();
                //var blockStart = br.BaseStream.Position;

                br.ReadInt32(); //magic 26 218 87 87
                var carRecordsCount = br.ReadInt32();//87/85
                var skinRecordsCount = br.ReadInt32();//66/67
                var transformCount = br.ReadInt32();//62/65
                var data2Count = br.ReadInt32();//27/28
                br.ReadInt32(); //padding

                this.Capacity += carRecordsCount;

                SkinRegion[] skinRecordsRegionPtrs = new SkinRegion[skinRecordsCount];
                SkinRegion[] data2RegionPtrs = new SkinRegion[data2Count];

                //read car records
                for (int i = 0; i < carRecordsCount; i++)
                {
                    var xname = br.ReadUInt32();

                    var skinRecordsFrom = br.ReadInt32();
                    var skinRecordsTo = br.ReadInt32();

                    var data2From = br.ReadInt32();
                    var data2To = br.ReadInt32();

                    var collection = new SkinRegion(br, this, xname);

                    var SkinCount = 0;
                    var LiveriesCount = 0;

                    for (int j = skinRecordsFrom; j < skinRecordsTo; j++)
                    {
                        skinRecordsRegionPtrs[j] = collection;
                        SkinCount++;
                    }
                    collection.Skins.Capacity = SkinCount;

                    for (int j = data2From; j < data2To; j++)
                    {
                        data2RegionPtrs[j] = collection;
                        LiveriesCount++;
                    }
                    collection.Liveries.Capacity = LiveriesCount;


                    try { this.Add(collection); }
                    catch { } // skip if exists
                }

                //read skins
                Skin[] transformRegionPtrs = new Skin[transformCount];
                for (int i = 0; i < skinRecordsCount; i++)
                {
                    Skin sk = new Skin(br.ReadUInt32());
                    skinRecordsRegionPtrs[i].Skins.Add(sk);

                    var transformRecordsFrom = br.ReadInt32();
                    var transformRecordsTo = br.ReadInt32();
                    for (int j = transformRecordsFrom; j < transformRecordsTo; j++)
                    {
                        transformRegionPtrs[j] = sk;
                    }
                }

                //read transforms
                for (int i = 0; i < transformCount; i++)
                {
                    transformRegionPtrs[i].Transforms.Add(new Transform(br));
                }

                //this part is just an enumeration from 0 to transformCount
                br.BaseStream.Position += transformCount * 4;

                //read liveries
                for (int i = 0; i < data2Count; i++)
                {
                    data2RegionPtrs[i].Liveries.Add(new Livery(br));
                }




                /*
                 * 32   XNAME int int int int float float
                 *      length 87 * 7*4
                 *      binding data ?
                 *                   index0 newindex0 index1 newindex1 
                 *                   only read when index changes
                 *      KOE_CCX_STK_06  0  1  0  0 1.0 1.0
                 *      TRF_TRA_LOG_84  1  1  0  0 1.0 1.0
                 *      LOT_ELI_111_06  1  2  0  0 1.0 1.0
                 *      BMW_M3_E46_03   2  3  0  0 1.0 1.0
                 *      BMW_M3_E92_08   3  4  0  0 1.0 1.0
                 *      DOD_CHA_CON_06  4  5  0  0 1.0 1.0
                 *      FOR_MUS_GT_06   5  6  0  1 1.0 1.0
                 *      AUD_S5_STK_08   6  7  1  1 1.0 1.0
                 *      TRF_TRK_BUS_87  7  7  1  1 1.0 1.0
                 *      ...
                 *      TOY_SUP_STK_98 62 63 26 26 1.0 1.0
                 *      FOR_GT_STK_06  63 64 26 26 1.0 1.0
                 *      BUG_VEY_164_08 64 65 26 26 1.0 1.0
                 *      BMW_M6_STK_08  65 66 26 27 1.0 1.0
                 *      
                 * 2468 DEFAULT_XNAME int int
                 *      -> depends on index0
                 *      length 66 * 3*4
                 *      
                 *      DEFAULT_KOE_CCX_STK_06  0  1
                 *      DEFAULT_LOT_ELI_111_06  1  2
                 *      ...
                 *      DEFAULT_BUG_VEY_164_08 60 61
                 *      DEFAULT_BMW_M6_STK_08  61 62
                 *      
                 * 3260 0151FCF4 float float float float float
                 *      length 62 * 6*4
                 *      
                 * 4748 int from 0 to 3D/61
                 *      length 62 * 1*4
                 *      
                 * 4996 hash DEFAULT_XNAME hash 0 0
                 *      -> depends on index1
                 *      length 27 * 5*4
                 *      
                 * 5536 end
                 */



                //                var current = br.BaseStream.Position;

            }
        }

        internal override void CreationCheck(string cname)
        {
            if (String.IsNullOrWhiteSpace(cname))
            {

                throw new ArgumentNullException("CollectionName cannot be null, empty or whitespace");

            }

            if (cname.Contains(" "))
            {

                throw new ArgumentException("CollectionName cannot contain whitespace");

            }

            if (this.Find(cname) != null)
            {

                throw new CollectionExistenceException(cname);

            }
        }

        public override void Export(string cname, BinaryWriter bw, bool serialized = true)
        {
            var index = this.IndexOf(cname);

            if (index == -1)
            {

                throw new Exception($"Collection named {cname} does not exist");

            }
            else
            {

                if (serialized) this[index].Serialize(bw);
                else throw new NotSupportedException("Collection supports only serialization and no plain export");

            }
        }

        public override void Import(SerializeType type, BinaryReader br)
        {
            var position = br.BaseStream.Position;
            var header = new SerializationHeader();
            header.Read(br);

            var collection = new SkinRegion();

            if (header.ID != BinBlockID.Nikki)
            {

                throw new Exception($"Missing serialized header in the imported collection");
                //br.BaseStream.Position = position;
                //collection.Disassemble(br);

            }
            else
            {

                if (header.Game != this.GameINT)
                {

                    throw new Exception($"Stated game inside collection is {header.Game}, while should be {this.GameINT}");

                }

                if (header.Name != this.Name)
                {

                    throw new Exception($"Imported collection is not a collection of type {this.Name}");

                }

                collection.Deserialize(br);

            }

            var index = this.IndexOf(collection);

            if (index == -1)
            {

                collection.Manager = this;
                this.Add(collection);

            }
            else
            {

                switch (type)
                {
                    case SerializeType.Negate:
                        break;

                    case SerializeType.Synchronize:
                    case SerializeType.Override:
                        collection.Manager = this;
                        this.Replace(collection, index);
                        break;

                    //case SerializeType.Override:
                    //    collection.Manager = this;
                    //    this.Replace(collection, index);
                    //    break;

                    //case SerializeType.Synchronize:
                    //    this[index].Synchronize(collection);
                    //    break;

                    default:
                        break;
                }

            }
        }





    }
}