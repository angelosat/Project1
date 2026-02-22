using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Graphics;
using Project1.Core.Graphics.Particles;
using Project1.Core.Materials;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Graphics;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;

namespace Project1.Core.Blocks
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
        }
        static void Trample(MapBase map, IntVec3 global)
        {
            var cell = map.GetCell(global);
            Block.Place(BlockDefOf.Soil.Block, map, global, cell.Material, 0, cell.Variation, 0);
        }

        public override void OnSteppedOn(GameObject actor, IntVec3 global)
        {
            var net = actor.Net;
            if (net is Client)
                return;
            if (actor.Map.Random.Roll(TramplingChance))
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
                PacketGrowRandomFlower = Registry.PacketHandlers.Register(GrowRandomFlower);
                PacketTrample = Registry.PacketHandlers.Register(SyncTrample);
            }
            public static void GrowRandomFlower(MapBase map, IntVec3 global)
            {
                var net = map.Net;
                if (net is Server)
                    BlockGrass.GrowRandomFlower(map, global);
                net.BeginPacket(PacketGrowRandomFlower).Write(global);
            }
            private static void GrowRandomFlower(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
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
                if (net is not Server server)
                    throw new Exception();
                Trample(map, global);
                //net.WriteToStream(PacketTrample, global);
                server.BeginPacket(PacketTrample)
                    .Write(global);
            }
            private static void SyncTrample(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var global = r.ReadIntVec3();
                if (net is Server)
                    throw new Exception();
                Trample(net.Map, global);
            }
        }
    }
}
