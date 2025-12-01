using Microsoft.Xna.Framework;
using Start_a_Town_.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.Particles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    class BlockGrass : Block
    {
        public override bool IsMinable => true;
        public override Color DirtColor => Color.DarkOliveGreen;
        public override ParticleEmitterSphere GetEmitter()
        {
            return base.GetDirtEmitter();
        }

        readonly List<AtlasDepthNormals.Node.Token> Overlays = new(3);
        public static List<AtlasDepthNormals.Node.Token> FlowerOverlays = new();

        public static readonly double TramplingChance = 0.1f;

        public BlockGrass()
            : base("Grass", 0, 1, true, true)
        {
            //this.LootTable = new LootTable(
            //            new Loot(() => ItemFactory.CreateFrom(RawMaterialDef.Bags, MaterialDefOf.Soil), 1f, 1, RawMaterialDef.Bags.StackCapacity)
            //            );
            this.BreakProduct = RawMaterialDefOf.Bags;

            this.LoadVariations("grass/grass1", "grass/grass2", "grass/grass3", "grass/grass4");

            foreach (var item in new AtlasDepthNormals.Node.Token[] {
                Atlas.Load("blocks/grass/grass1-overlay", BlockDepthMap, BlockMouseMap.Texture),
                Atlas.Load("blocks/grass/grass2-overlay", BlockDepthMap, BlockMouseMap.Texture),
                Atlas.Load("blocks/grass/grass3-overlay", BlockDepthMap, BlockMouseMap.Texture)})
                this.Overlays.Add(item);

            FlowerOverlays.Add(Atlas.Load("blocks/grass/flowersoverlayred", BlockDepthMap, NormalMap));
            FlowerOverlays.Add(Atlas.Load("blocks/grass/flowersoverlayyellow", BlockDepthMap, NormalMap));
            FlowerOverlays.Add(Atlas.Load("blocks/grass/flowersoverlaywhite", BlockDepthMap, NormalMap));
            FlowerOverlays.Add(Atlas.Load("blocks/grass/flowersoverlaypurple", BlockDepthMap, NormalMap));
            this.DrawMaterialColor = false;
        }

        internal static void GrowRandomFlower(MapBase map, IntVec3 global)
        {
            var net = map.Net;
            if (net is Client)
                throw new Exception();
            byte data = (byte)(map.Random.Next(FlowerOverlays.Count) + 1);
            map.SyncSetCellData(global, data);
        }

        public override byte ParseData(string data)
        {
            return byte.Parse(data);
        }

        AtlasDepthNormals.Node.Token GetFlowerOverlay(byte data)
        {
            var flowerIndex = data - 1; //because 0 is no flowers
            return FlowerOverlays[flowerIndex];
        }
        public override IEnumerable<MaterialDef> GetEditorVariations()
        {
            yield return MaterialDefOf.Human;
            //return Enumerable.Range(0, 5).Select(i => (byte)i);
        }
        static void Trample(MapBase map, IntVec3 global)
        {
            var cell = map.GetCell(global);
            Block.Place(BlockDefOf.Soil, map, global, cell.Material, 0, cell.Variation, 0);
        }

        public override void OnSteppedOn(GameObject actor, IntVec3 global)
        {
            var net = actor.Net;
            if (net is Client)
                return;
            if (actor.Map.Random.Chance(TramplingChance))
                Packets.SyncTrample(actor.Map, global);
        }

        internal override float GetFertility(Cell cell)
        {
            if (cell.BlockData > 0) // if there are flowers grown, dont grow anything else (return fertility = 0)
                return 0;
            return base.GetFertility(cell);
        }
       
        public override MyVertex[] Draw(Chunk chunk, IntVec3 global, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data, MaterialDef mat)
        {
            return base.Draw(chunk, global, camera, screenBounds, sunlight, blocklight, fog, tint, depth, variation, orientation, data, mat);
        }
        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, IntVec3 global, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data, MaterialDef mat)
        {
            base.Draw(canvas, chunk, global, camera, screenBounds, sunlight, blocklight, fog, tint, depth, variation, orientation, data, mat);
            if (data == 0)
                return null;
            var fl = this.GetFlowerOverlay(data);
            return canvas.Opaque.DrawBlock(fl.Atlas.Texture, screenBounds, fl, camera.Zoom, fog, tint, Color.White, sunlight, blocklight, Vector4.Zero, depth, this, global);
        }
        public class Packets
        {
            static readonly int PacketGrowRandomFlower, PacketTrample;
            static Packets()
            {
                PacketGrowRandomFlower = Network.RegisterPacketHandler(GrowRandomFlower);
                PacketTrample = Network.RegisterPacketHandler(SyncTrample);
            }
            public static void GrowRandomFlower(MapBase map, IntVec3 global)
            {
                var net = map.Net;
                if (net is Server)
                    BlockGrass.GrowRandomFlower(map, global);
                net.GetOutgoingStream().Write(PacketGrowRandomFlower, global);
            }
            private static void GrowRandomFlower(INetEndpoint net, BinaryReader r)
            {
                var map = net.Map;
                var global = r.ReadIntVec3();
                if (net is Client)
                    BlockGrass.GrowRandomFlower(map, global);
                else
                    GrowRandomFlower(map, global);
            }
            public static void SyncTrample(MapBase map, IntVec3 global)
            {
                var net = map.Net;
                if (net is Client)
                    throw new Exception();
                Trample(map, global);
                net.WriteToStream(PacketTrample, global);
            }
            private static void SyncTrample(INetEndpoint net, BinaryReader r)
            {
                var global = r.ReadIntVec3();
                if (net is Server)
                    throw new Exception();
                Trample(net.Map, global);
            }
        }
    }
}
