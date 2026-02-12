using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Project1.Framework;
using Project1.Framework.Graphics;
using Project1.Framework.Helpers;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Graphics;
using Project1.Core.Simulation;

namespace Project1.Core.Helpers
{
    public static class ExtensionsNew
    {
        public static int GetAmount(this List<GameObjectSlot> list, Func<GameObject, bool> condition)
        {
            int amount = 0;
            (from slot in list
             where slot.HasValue
             where condition(slot.Object)
             select slot)
             .ToList()
             .ForEach(slot => amount += slot.StackSize);
            return amount;
        }
        public static float GetMouseoverDepth(this Vector3 worldGlobal, MapBase map, Camera camera)
        {
            Vector3 local = worldGlobal - new Vector3(map.GetOffset(), 0);
            Vector3 rotated = local.Rotate(camera);
            return rotated.X + rotated.Y + worldGlobal.Z;
        }
        public static float GetDrawDepth(this Vector3 worldGlobal, MapBase map, Camera camera)
        {
            Vector3 local = worldGlobal - new Vector3(map.GetOffset(), 0);
            Vector3 rotated = local.Rotate(camera);
            return rotated.X + rotated.Y;
        }
        public static float GetDrawDepth(this IntVec3 worldGlobal, MapBase map, Camera camera)
        {
            IntVec3 local = worldGlobal - new IntVec3(map.GetOffset(), 0);
            IntVec3 rotated = local.Rotate(camera);
            return rotated.X + rotated.Y;
        }
        public static Vector3 ToLocal(this Vector3 global)
        {
            float rx, ry;
            rx = global.X % Chunk.Size;
            rx = rx < 0 ? rx + Chunk.Size : rx;
            ry = global.Y % Chunk.Size;
            ry = ry < 0 ? ry + Chunk.Size : ry;
            return new Vector3(rx, ry, global.Z);
        }
        static public Vector3 ToGlobal(this Vector3 local, Chunk chunk)
        {
            return new Vector3(chunk.Start.X + local.X, chunk.Start.Y + local.Y, local.Z);
        }
        public static Dictionary<GameObject, int> ToDictionaryGameObjectAmount(this IEnumerable<GameObject> objList)
        {
            var dic = new Dictionary<GameObject, int>();
            foreach (var item in objList)
                dic.AddOrUpdate(item, item.StackSize, f => f + item.StackSize);
            return dic;
        }

        public static void Draw(this Vector3 global, MySpriteBatch sb, Camera cam, AtlasWithDepth.Node.Token sprite, Color color)
        {
            var bounds = cam.GetScreenBounds(global, Block.Bounds);
            var pos = new Vector2(bounds.X, bounds.Y);
            var depth = global.GetDrawDepth(Engine.Map, cam);
            sb.Draw(sprite.Atlas.Texture, pos, sprite.Rectangle, 0, Vector2.Zero, cam.Zoom, color, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, depth);
        }
        static public bool ContainsEntityFootprint(this Vector3 blockGlobal, GameObject entity)
        {
            var footprint = entity.GetFootprint();
            var blockbox = blockGlobal.GetBoundingBox();
            var containment = blockbox.Contains(footprint);
            return containment == ContainmentType.Contains;
        }
        public static List<Vector2> GetSpiral(this Vector2 center, int radius = Engine.ChunkRadius)
        {
            var list = new List<Vector2>();


            for (int i = -radius; i <= radius; i++)
                for (int j = -radius; j <= radius; j++)
                {
                    var vec2 = new Vector2(i, j);
                    if (vec2.Length() <= radius)
                        list.Add(center + vec2);
                }
            list.Sort((u, v) =>
            {
                if (u == v) return 0;
                else if (Vector2.DistanceSquared(center, u) < Vector2.DistanceSquared(center, v)) return -1;
                else return 1;
            });
            return list;
        }

    }
}
