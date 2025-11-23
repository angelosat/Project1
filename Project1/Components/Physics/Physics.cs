using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Physics
{
    class Physics
    {
        static public float Friction { get { return PhysicsComponent.Friction; } }

        static public void Update(float weight, float friction, MapBase map, Vector3 global, Vector3 velocity, out Vector3 nextglobal, out Vector3 nextvelocity)
        {
            if (!map.TryGetCell(global, out var thisCell))
            {
                nextglobal = global;
                nextvelocity = velocity;
                return;
            }
            Block block = thisCell.Block;
            float density = 1 - (block.IsSolid(thisCell, global.ToBlock()) ? block.Density : 0);

            velocity *= density;

            float nx, ny, nz;

            BoundingBox box = new BoundingBox(global, global);

            var fric = Math.Min(1, 2 * Friction * friction);
            nz = ResolveVertical(map, box, weight, ref velocity, density);
            if (nz == global.Z)
            {
                if (velocity.X > 0)
                    velocity.X = Math.Max(0, velocity.X - fric);
                else
                    velocity.X = Math.Min(0, velocity.X + fric);
                if (velocity.Y > 0)
                    velocity.Y = Math.Max(0, velocity.Y - fric);
                else
                    velocity.Y = Math.Min(0, velocity.Y + fric);
            }


            if (velocity.X != 0)
                nx = ResolveHorizontal(fric, map, box, ref velocity, Vector2.UnitX, nz, out nz).X;
            else
                nx = global.X;

            if (velocity.Y != 0)
                ny = ResolveHorizontal(fric, map, box, ref velocity, Vector2.UnitY, nz, out nz).Y;
            else
                ny = global.Y;

            Vector3 next = ResolveCorner(map, global, new Vector3(nx, ny, nz));

            nextvelocity = velocity;
            nextglobal = next;

        }
        static private Vector3 ResolveCorner(MapBase map, Vector3 current, Vector3 next)
        {
            if (next == current)
                return current;
            if (!Block.IsBlockSolid(map, next))
                return next;
            Vector3 newNext;
            Vector3 nextRound = next.RoundXY();
            // TODO: do something without square root for speed
            float dx = Math.Abs(next.X - nextRound.X);
            float dy = Math.Abs(next.Y - nextRound.Y);
            if (dx < dy)
                newNext = new Vector3(next.X, current.Y, next.Z);
            else
                newNext = new Vector3(current.X, next.Y, next.Z);
            return newNext;
        }
        static private float ResolveVertical(MapBase map, BoundingBox box, float weight,  ref Vector3 speed, float density)
        {
            var grav = map.Gravity * weight;
            Vector3 global = box.Min;
            if (speed.Z == 0)
            {
                if (!Block.IsBlockSolid(map, global + new Vector3(0, 0, grav)))
                {
                    speed.Z = grav;
                    return global.Z + density * grav;
                }
                return global.Z;
            }
            Vector3 offset = new Vector3(0, 0, speed.Z);
            Vector3 next = global + offset;
            if (speed.Z < 0)
            {
                float height = global.Z - next.Z;
                if (height > 1)
                {
                    for (int i = 0; i < (int)Math.Ceiling(height); i++)
                    {
                        Vector3 check = global - new Vector3(0, 0, 1 + i);
                        if (Block.IsBlockSolid(map, check))
                        {
                            speed.Z = 0;
                            return (int)check.Z + Block.GetBlockHeight(map, check);
                        }
                    }
                }
                else if (height > 0)
                {
                    if (Block.IsBlockSolid(map, next))
                    {
                        speed.Z = 0;
                        var blockheightbelow = Block.GetBlockHeight(map, next);
                        return (int)next.Z + blockheightbelow;
                    }
                }
            }
            else
            {
                if (Block.IsBlockSolid(map, box.Max + offset))
                {
                    speed.Z = 0;
                    return box.Min.Z;
                }
            }
            speed.Z += grav;
            return next.Z;
        }
        private static Vector3 ResolveHorizontal(float friction, MapBase map, BoundingBox boxGlobal, ref Vector3 velocity, Vector2 horAxis, float nz, out float zz)
        {
            zz = nz;
            Vector3 unit = new Vector3(horAxis, 0) * velocity;
            Vector3 next = boxGlobal.Min + unit;
            int min = (int)boxGlobal.Min.Z, max = (int)boxGlobal.Max.Z;
            int d = max - min;
            for (float z = 0; z < 1; z+=0.5f)//d; z += 0.5f) // increment by 0.5f so we dont miss the solid part of a less than full height block
            {
                Vector3 check = next;
                if(Block.IsBlockSolid(map, check))
                {
                    return boxGlobal.Min;
                }
            }
            velocity = velocity * (Vector3.One - unit) + unit * (next - boxGlobal.Min);
            return next;
        }
    }
}
