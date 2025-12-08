using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public enum ObjectSize { Inventoryable, Haulable, Immovable }
    public class PhysicsComponent : EntityComponent
    {
        public override string Name { get; } = "Physics";
        public ObjectSize Size => this.Parent.Def.Size;
        public bool Solid;
        public float Height;
        float? _weight;
        public float Weight => this._weight ??= this.UpdateWeight();
        public bool Enabled = true;
        const float FrictionFactor = .5f;
        public static float Jump = 0.2f;//-0.04f;// -0.05f; //35f;
        public static float Friction = 0.02f;// 0.005f; // TODO move this to blocks? 
        public bool MidAir { get; private set; } // => this.Parent.Velocity.Z != 0;// HACK because checking velocity.z == 0 returns true at the peak of the jump 
        public const int KnockbackMagnitude = 3;
        public Vector3 Force;

        public PhysicsComponent()
            : base()
        {
            //this.Initialize();
        }
        public PhysicsComponent(PhysicsComponent toCopy)
        {
            this.Height = toCopy.Height;
        }
       
        public int Reach => (int)Math.Ceiling(this.Height) + 2;

        /// <summary>
        /// make positioncomponent, global, velocity, height, map, fields instead of properties
        /// </summary>
        /// <param name="net"></param>
        /// <param name="parent"></param>
        /// <param name="chunk"></param>
        public override void Tick()
        {
            if (this.Parent.Net is Client)
            {
                this.DetectEntityCollisions(this.Parent, this.Parent.Global, this.Parent.Global + this.Parent.Velocity);
                return;
            }
            var force = this.Force;
            this.Force = Vector3.Zero;

            if (!this.Enabled)
                return;

            var parent = this.Parent;
            var map = parent.Map;
            var net = map.Net;
            var lastGlobal = parent.Transform.Global;
            if (!map.TryGetCell(lastGlobal, out var thisCell))
                return;

            var velocity = parent.Velocity + force;

            var feetposition = lastGlobal + Vector3.UnitZ * .1f;
            var feetcell = map.GetCell(feetposition);
            var feetblock = feetcell.Block;
            if (!this.Enabled)
                return;

            // TODO: make getdensity method on block that takes into account its height
            float density = feetblock.GetDensity(feetcell.BlockData, feetposition);
            velocity *= 1 - density;

            float nx, ny, nz;
            var box = this.GetBoundingBox(parent, lastGlobal);

            ///moved this after the friction application to fix velocity being larger while jumping
            //nz = this.ResolveVertical(parent, map, box, ref velocity, density); 

            if (!this.MidAir)
                velocity = new Vector3(velocity.X * FrictionFactor, velocity.Y * FrictionFactor, velocity.Z);
            if (velocity.LengthSquared() < .0001f)
                velocity = Vector3.Zero;

            ///moved this after the friction application to fix velocity being larger while jumping
            nz = this.ResolveVertical(parent, map, box, ref velocity, density);

            var blocktransform = this.GetStandingBlockTransform(map, lastGlobal);
            var origin = parent.Global + blocktransform;

            if (velocity.X != 0 || blocktransform.X != 0)
                nx = ResolveHorizontalCorners(net, parent, origin, map, box, ref velocity, Vector2.UnitX, nz, out nz).X;
            else
                nx = lastGlobal.X;

            if (velocity.Y != 0 || blocktransform.Y != 0)
                ny = ResolveHorizontalCorners(net, parent, origin, map, box, ref velocity, Vector2.UnitY, nz, out nz).Y;
            else
                ny = lastGlobal.Y;

            var next = new Vector3(nx, ny, nz);

            if (lastGlobal != next)
                parent.SetPosition(next);
            else if (velocity == Vector3.Zero)
                this.Enabled = false;
            this.DetectEntityCollisions(parent, lastGlobal, next);
            // reset speed according to new position to prevent it from accumulating
            parent.Velocity = velocity;

            // log state change 
            if (parent.Exists) // must check because entity might have despawned itself during it's update 
            {
                if (lastGlobal != parent.Global)
                {
                    // send a "step on" message on next block
                    net.LogStateChange(parent.RefId);
                    Vector3 nextRounded = next.RoundXY();
                    if (nextRounded != lastGlobal.RoundXY())
                    {
                        Vector3 blockGlobal = nextRounded - Vector3.UnitZ;
                        var bl = parent.Map.GetBlock(blockGlobal);
                        bl.OnSteppedOn(parent, blockGlobal);
                    }
                }
            }
        }
        static Vector3[] BoundingBoxCorners = { 
            new Vector3(-.25f, -.25f, 0), 
            new Vector3(-.25f, .25f, 0), 
            new Vector3(.25f, -.25f, 0), 
            new Vector3(.25f, .25f, 0) };
        public IEnumerable<Vector3> GetFootprintCorners(Vector3 global)
        {
            yield return global + BoundingBoxCorners[0];
            yield return global + BoundingBoxCorners[1];
            yield return global + BoundingBoxCorners[2];
            yield return global + BoundingBoxCorners[3];
        }
        public IEnumerable<Vector3> GetBoundingBoxCorners(Vector3 global)
        {
            yield return global + BoundingBoxCorners[0];
            yield return global + BoundingBoxCorners[1];
            yield return global + BoundingBoxCorners[2];
            yield return global + BoundingBoxCorners[3];
            for (int i = 1; i < this.Height; i++)
            {
                var zVec = new Vector3(0, 0, i);
                yield return global + BoundingBoxCorners[0] + zVec;
                yield return global + BoundingBoxCorners[1] + zVec;
                yield return global + BoundingBoxCorners[2] + zVec;
                yield return global + BoundingBoxCorners[3] + zVec;
            }
        }
        public BoundingBox GetBoundingBox(Vector3 global)
        {
            return new BoundingBox(global - new Vector3(.25f, .25f, 0), global + new Vector3(.25f, .25f, this.Height));
        }
        public BoundingBox GetBoundingBox(GameObject parent, Vector3 global)
        {
            return new BoundingBox(global - new Vector3(.25f, .25f, 0), global + new Vector3(.25f, .25f, parent.Height));
        }
        public BoundingBox GetBoundingBox(Vector3 global, float height)
        {
            return new BoundingBox(global - new Vector3(.25f, .25f, 0), global + new Vector3(.25f, .25f, height));
        }
        private Vector3 GetStandingBlockTransform(MapBase map, Vector3 lastGlobal)
        {
            Vector3 blocktransform = Vector3.Zero;

            // if feet on contact with non-air block, get velocity transform of block directly below

            var underfeet = lastGlobal + new Vector3(0, 0, map.Gravity);
            var underfeetBlockCoords = underfeet.ToBlock();
            var underfeetCell = map.GetCell(underfeet);
            var underfeetBlock = underfeetCell.Block;
            if (underfeetBlock != BlockDefOf.Air)
            {
                blocktransform = underfeetCell.Block.GetVelocityTransform(underfeetCell.BlockData, underfeetBlockCoords);
            }
            else
            {
                var tocheck = new Vector3[3];
                if (underfeetBlockCoords.X < .5f && underfeetBlockCoords.Y < .5f)
                {
                    if (underfeetBlockCoords.X < underfeetBlockCoords.Y)
                    {
                        tocheck[0] = new Vector3(-1, 0, 0);
                        tocheck[1] = new Vector3(0, -1, 0);
                    }
                    else
                    {
                        tocheck[0] = new Vector3(0, -1, 0);
                        tocheck[1] = new Vector3(-1, 0, 0);
                    }
                    tocheck[2] = new Vector3(-1, -1, 0);
                }
                else if (underfeetBlockCoords.X > .5f && underfeetBlockCoords.Y < .5f)
                {
                    if (underfeetBlockCoords.X > underfeetBlockCoords.Y)
                    {
                        tocheck[0] = new Vector3(1, 0, 0);
                        tocheck[1] = new Vector3(0, -1, 0);
                    }
                    else
                    {
                        tocheck[0] = new Vector3(0, -1, 0);
                        tocheck[1] = new Vector3(1, 0, 0);
                    }
                    tocheck[2] = new Vector3(1, -1, 0);
                }
                else if (underfeetBlockCoords.X > .5f && underfeetBlockCoords.Y > .5f)
                {
                    if (underfeetBlockCoords.X > underfeetBlockCoords.Y)
                    {
                        tocheck[0] = new Vector3(1, 0, 0);
                        tocheck[1] = new Vector3(0, 1, 0);
                    }
                    else
                    {
                        tocheck[0] = new Vector3(0, 1, 0);
                        tocheck[1] = new Vector3(1, 0, 0);
                    }
                    tocheck[2] = new Vector3(1, 1, 0);
                }
                else
                {
                    if (underfeetBlockCoords.X < underfeetBlockCoords.Y)
                    {
                        tocheck[0] = new Vector3(-1, 0, 0);
                        tocheck[1] = new Vector3(0, 1, 0);
                    }
                    else
                    {
                        tocheck[0] = new Vector3(0, 1, 0);
                        tocheck[1] = new Vector3(-1, 0, 0);
                    }
                    tocheck[2] = new Vector3(-1, 1, 0);
                }

                Cell contactCell;
                foreach (var check in tocheck)
                {
                    var cellglobal = (underfeet + check).ToRounded();
                    contactCell = map.GetCell(cellglobal);
                    if (contactCell == null || contactCell.Block == BlockDefOf.Air)
                    {
                        continue;
                    }

                    blocktransform = contactCell.Block.GetVelocityTransform(contactCell.BlockData, (underfeetBlockCoords + check * .25f).ToRounded());
                }
            }
            return blocktransform;
        }

        private float ResolveVertical(GameObject parent, MapBase map, BoundingBox box, ref Vector3 speed, float density)
        {
            var grav = map.Gravity;
            var adjustedGravity = grav;
            var global = parent.Global;
            if (speed.Z == 0)
            {
                // TODO: maybe instead of checking the corners, i check if the box intersects with nearby block boxes?
                var corners = new Vector3[] {
                    box.Min,
                    new Vector3(box.Min.X, box.Max.Y, global.Z),
                    new Vector3(box.Max.X, box.Min.Y, global.Z),
                    new Vector3(box.Max.X, box.Max.Y, global.Z)
                };
                if (corners.All(c => !map.IsSolid(c + new Vector3(0, 0, adjustedGravity))))
                {
                    this.MidAir = true;
                    speed.Z = grav;
                    return global.Z + adjustedGravity;
                }

                return global.Z;
            }
            var offset = new Vector3(0, 0, speed.Z);
            var next = global + offset;
            if (speed.Z < 0)
            {
                float fallheight = global.Z - next.Z;
                if (fallheight > 1)
                {
                    for (int i = 0; i < (int)Math.Ceiling(fallheight); i++)
                    {
                        var check = global - new Vector3(0, 0, 1 + i);
                        if (IsStandable(map, check))
                        {
                            var f = speed.Z;
                            //parent.Net.PostLocalEvent(parent, Message.Types.HitGround, Math.Abs(speed.Z));
                            speed.Z = 0;
                            // land
                            this.HitGround(parent, check, f);
                            return (int)check.Z + Block.GetBlockHeight(map, check);
                        }
                    }
                }
                else if (fallheight > 0)
                {
                    var nextbox = new BoundingBox(next - new Vector3(.25f, .25f, 0), next + new Vector3(.25f, .25f, this.Height));
                    var corners =
                        new Vector3[] {
                            nextbox.Min,
                            new Vector3(nextbox.Min.X, nextbox.Max.Y, next.Z),
                            new Vector3(nextbox.Max.X, nextbox.Min.Y, next.Z),
                            new Vector3(nextbox.Max.X, nextbox.Max.Y, next.Z)
                        };
                    if (corners.Any(c => map.IsSolid(c + new Vector3(0, 0, grav))))// GetDensity(map, c + new Vector3(0, 0, grav)) > 0)) // DONT CHECK DENSITY BECAUSE THEN THE ENTITY WONT SINK IN WATER
                    {
                        var f = speed.Z;
                        speed.Z = 0;
                        // land
                        this.HitGround(parent, next, f);

                        float blockheightbelow = 0;
                        //blockheightbelow = corners.Max(c => Block.GetBlockHeight(map, c));
                        blockheightbelow = corners.Max(c => map.GetCell(c)?.GetBlockHeight(c) ?? 0);
                        return (int)next.Z + blockheightbelow;
                    }
                }
            }
            else
            {
                this.MidAir = true;
                // hit on cieling
                if (map.IsSolid(box.Max + offset))
                {
                    // dont set velocity z to 0, because later we check if velocity is zero and we disable physics. 
                    // reflect velocity instead
                    this.HitCeiling(parent, (box.Max + offset));
                    speed.Z = -speed.Z;
                    return box.Min.Z;
                }
            }
            speed.Z += adjustedGravity;
            return next.Z;
        }

        internal void Applyforce(Vector3 nextvelocity)
        {
            this.Force += nextvelocity;
            this.Enabled = true;
        }

        private static Vector3 ResolveHorizontalCorners(INetEndpoint net, GameObject parent, Vector3 origin, MapBase map, BoundingBox boxGlobal, ref Vector3 velocity, Vector2 horAxis, float nz, out float zz)
        {
            zz = nz;
            Vector3 step = new Vector3(horAxis, 0) * velocity;
            Vector3 nextorigin = origin + step;
            var leadingCorners = new List<Vector3>()
            {
                nextorigin + new Vector3(-.25f, -.25f, 0),
                nextorigin + new Vector3(.25f, -.25f, 0),
                nextorigin + new Vector3(-.25f, .25f, 0),
                nextorigin + new Vector3(.25f, .25f, 0)
            };

            int minz = (int)boxGlobal.Min.Z, maxz = (int)boxGlobal.Max.Z;
            int dz = maxz - minz;

            foreach (var next in leadingCorners)
            {
                for (float z = 0; z < dz; z += 0.5f) // increment by 0.5f so we dont skip the solid part of a lower height block
                {
                    Vector3 check = next + new Vector3(0, 0, z);
                    if (map.IsSolid(check))
                    {
                        // check if can climb
                        if (velocity.Z == 0 && z == 0)
                        {
                            var stepUp = check + Vector3.UnitZ / 2f;
                            if (!map.IsSolid(stepUp))
                            {
                                var hbottom = map.GetSolidObjectHeight(check);
                                var htop = map.GetSolidObjectHeight(check + Vector3.UnitZ);
                                var h = hbottom + htop;
                                var floor = step + new Vector3(origin.X, origin.Y, (float)Math.Floor(check.Z));
                                var stepon = floor + Vector3.UnitZ * (h + .01f);
                                zz = stepon.Z;
                                return stepon;
                            }
                        }
                        var block = map.GetBlock(next);
                        return origin;
                    }
                }
            }
            velocity = velocity * (Vector3.One - step) + step * (step);
            return origin + step;
        }

        void DetectEntityCollisions(GameObject parent, Vector3 last, Vector3 next)
        {
            if (next == last)
            {
                return;
            }

            foreach (Chunk ch in parent.Map.GetChunks(parent.Map.GetChunk(last).MapCoords)) // TODO: optimize, search for entities on nearby chunks only if entity is on current chunk edge
            {
                foreach (GameObject obj in ch.GetObjects())
                {
                    if (obj == parent)
                    {
                        continue;
                    }

                    var lastDistance = Vector3.Distance(obj.Global, last);
                    var nextDistance = Vector3.Distance(obj.Global, next);
                    if (lastDistance >= 1 && nextDistance < 1) // changed the inequality so the item doesn't combine if freefalling on an adjacent block
                    {
                        // collision
                        //obj.PostMessage(ObjectEventArgs.Create(Message.Types.EntityCollision, [parent]));
                        //obj.Net.EventOccured(Message.Types.EntityCollision, parent, obj); //removing this because object gets disposed as a result of the above line
                        obj.Map.Events.Post(new EntityCollisionEvent(parent as Entity, obj as Entity));
                    }
                    // TODO: combine items only when an item enters another stationary item's cell?
                }
            }
        }
        public override void OnSpawn()
        {
            this.Enabled = true;
            this.Parent.Map.Events.ListenTo<EntityCollisionEvent>(this.HandleCollision);
        }
        
        void HandleCollision(EntityCollisionEvent e)
        {
            var parent = this.Parent;
            if (!parent.Net.IsServer)
                return;
                //throw new Exception("physics shouldn't run in clients");

            var otherItem = e.Target;
            if (otherItem.CanAbsorb(parent) && !parent.IsReserved)
                /// revmoved the reserved check from canabsorb and placed it here, because canabsorb is called during legit behaviors that involve the items, 
                /// which means the items are reserved but still should be absorbable
                otherItem.SyncAbsorb(parent);
        }
     
        public override object Clone()
        {
            var phys = new PhysicsComponent(this)
            {
                Height = this.Height,
                Solid = this.Solid
            };
            return phys;
        }

        public override void OnTooltipCreated(GameObject parent, UI.Control tooltip)
        {
            if (this.Size == ObjectSize.Immovable)
            {
                return;
            }

            tooltip.AddControlsBottomLeft(
                new GroupBox().AddControlsHorizontally(
                    new Label("Weight: ") { TextColor = Color.LightGray },
                    new Label(() => $"{this.Weight * parent.StackSize} kg ({this.Weight} kg x{parent.StackSize})") { TextColor = Color.LightGray })
                );
        }
      
        public override string ToString()
        {
            return "Enabled: " + this.Enabled.ToString() + "\n" + base.ToString();
        }

        public static void Enable(GameObject parent)
        {
            parent.TryGetComponent<PhysicsComponent>(f => f.Enabled = true);
        }

        /// <summary>
        /// TODO: pass cell or block in here since i fetch it (by checking if the position is solid) in the check before calling this method
        /// </summary>
        /// <param name="vector3"></param>
        private void HitGround(GameObject parent, Vector3 vector3, float force)
        {
            this.MidAir = false;
            parent.Map.EventOccured(Message.Types.EntityHitGround, parent, vector3);
            //parent.Net.PostLocalEvent(parent, Message.Types.HitGround, Math.Abs(vector3.Z));
            parent.Net.Map.Events.Post(new EntityHitGroundEvent(parent as Entity, force));
        }
        
        private void HitCeiling(GameObject parent, Vector3 vector3)
        {
            parent.Map.EventOccured(Message.Types.EntityHitCeiling, parent, vector3);
        }

        public static bool IsStanding(GameObject parent)
        {
            var global = parent.Global;
            var height = parent.Physics.Height;
            var map = parent.Map;
            var gravity = map.Gravity;
            var box = new BoundingBox(global - new Vector3(.25f, .25f, 0), global + new Vector3(.25f, .25f, height));
            var corners = new Vector3[] {
                    box.Min,
                    new Vector3(box.Min.X, box.Max.Y, global.Z),
                    new Vector3(box.Max.X, box.Min.Y, global.Z),
                    new Vector3(box.Max.X, box.Max.Y, global.Z)
                };
            return corners.Any(c => map.GetBlock(c + new Vector3(0, 0, gravity)).Density > 0);
        }

        public static Vector3 Decelerate(Vector3 velocity)
        {
            return velocity *= FrictionFactor;// .5f;
        }

        public static float GetDensity(MapBase map, Vector3 global)
        {
            var cell = map.GetCell(global);
            return cell.Block.GetDensity(cell.BlockData, global);
        }
        public static bool IsStandable(MapBase map, Vector3 global)
        {
            var gravity = map.Gravity;
            var box = new BoundingBox(global - new Vector3(.25f, .25f, 0), global + new Vector3(.25f, .25f, 1));
            var corners = new Vector3[] {
                    box.Min,
                    new Vector3(box.Min.X, box.Max.Y, global.Z),
                    new Vector3(box.Max.X, box.Min.Y, global.Z),
                    new Vector3(box.Max.X, box.Max.Y, global.Z)
                };
            return corners.Any(c => map.GetBlock(c + new Vector3(0, 0, gravity)).Density > 0);
        }

        //public override void OnObjectLoaded(GameObject parent)
        //{
        //    // update def changes
        //    this._weight = UpdateWeight();
        //}
        //public override void OnObjectCreated(GameObject parent)
        //{
        //    //this.UpdateWeight();
        //}
        private float UpdateWeight()
        {
            var parent = this.Parent;
            var bones = parent.Body.GetAllBones();
            float w = bones.Sum(b => b.Material.Density) / bones.Count();
            w = this.Size switch
            {
                ObjectSize.Immovable => w,
                ObjectSize.Inventoryable => w / 10f,
                ObjectSize.Haulable => w,
                _ => throw new Exception()
            };
            return w;
        }
        public void SetWeight(float value)
        {
            this._weight = value;
        }
        public override void Write(IDataWriter w)
        {
            w.Write(this._weight.HasValue);
            if (this._weight.HasValue)
                w.Write(this._weight.Value);
        }
        public override void Read(IDataReader r)
        {
            if (r.ReadBoolean())
                this._weight = r.ReadSingle();
        }
    }
}
