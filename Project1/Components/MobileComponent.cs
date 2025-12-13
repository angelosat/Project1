using System;
using System.Collections.Generic;
using Start_a_Town_.Animations;
using Start_a_Town_.Particles;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    public class MobileComponent : EntityComp
    {
        public class State
        {
            public enum Types { Walking, Running, Sprinting, Blocking };
            public Types Type;
            public string Name;
            public float Speed;
            public float SprintSpeed;
            public float AnimationWeight;
            public float AnimationSpeed;
            public bool AllowJump;
            public State(Types type, float speed, float sprintSpeed, float animationWeight, float animationSpeed, bool allowJump)
            {
                this.Type = type;
                this.Name = type.ToString();
                this.Speed = speed;
                this.SprintSpeed = sprintSpeed;
                this.AnimationWeight = animationWeight;
                this.AnimationSpeed = animationSpeed;
                this.AllowJump = allowJump;
            }
            public void Apply(MobileComponent component)
            {
                if (!component.Moving)
                    component.AnimationWalk.Frame = 0;
                component.AnimationWalk.Weight = this.AnimationWeight;
                component.AnimationWalk.Speed = this.AnimationSpeed;
            }
            public override string ToString()
            {
                return this.Name;
            }
            static readonly State Walking = new(Types.Walking, speed: 0.66f, sprintSpeed: 0, animationWeight: 0.5f, animationSpeed: 1, allowJump: false);
            static readonly State Running = new(Types.Running, speed: 1f, sprintSpeed: 0, animationWeight: 0.75f, animationSpeed: 1f, allowJump: true);
            static readonly State Sprinting = new(Types.Sprinting, speed: 1f, sprintSpeed: 0.5f, animationWeight: 1f, animationSpeed: 1.5f, allowJump: true);
            static readonly State Blocking = new(Types.Blocking, speed: 0.5f, sprintSpeed: 0, animationWeight: 0.5f, animationSpeed: 1, allowJump: false);

            static public Dictionary<Types, State> States = new()
            {
                {Types.Walking, Walking },
                {Types.Running, Running },
                {Types.Sprinting, Sprinting },
                {Types.Blocking, Blocking }
            };
        }

        public const float NormalWalkSpeed = .1f;// 0.08f; when i used friction wrongly

        public override string Name { get; } = "Mobile"; 
        public override object Clone()
        {
            return new MobileComponent();
        }

        public float Acceleration;

        readonly Animation AnimationWalk;
        readonly Animation AnimationJump;
        readonly Animation AnimationCrouch;

        public bool Moving;

        State CurrentState;
        int JumpCooldown;
        public bool CanJump => this.JumpCooldown == 0;

        internal bool Crouching => this.AnimationCrouch.Weight > 0;
        const float AccelerationStep = .1f;
        public MobileComponent()
        {
            this.AnimationWalk = new(AnimationDef.Walk);
            this.AnimationJump = new(AnimationDef.Jump);
            this.AnimationCrouch = new(AnimationDef.Crouch);

            this.Acceleration = 0f;
            this.Moving = false;
            this.CurrentState = State.States[State.Types.Running];
            this.CurrentState.Apply(this);

            this.AnimationJump.Weight = 0;
            this.AnimationWalk.Weight = 0;
            this.AnimationCrouch.Weight = 0;
        }
        
        static public void OnFootDown(GameObject parent)
        {
            EmitDust(parent);
        }

        public override void AttachTo(GameObject parent)
        {
            parent.AddAnimation(this.AnimationJump);
            parent.AddAnimation(this.AnimationWalk); // why not add it on creation?
            parent.AddAnimation(this.AnimationCrouch);
        }

        public void Toggle(GameObject parent, bool toggle)
        {
            if (toggle)
                this.Start(parent);
            else
                this.Stop(parent);
        }
        public void Start(GameObject parent)
        {
            this.Start(parent, this.CurrentState);
        }
        void Start(GameObject parent, State state)
        {
            if (this.Moving)
                return;
            this.CurrentState = state;
            this.Acceleration = AccelerationStep;

            this.AnimationWalk.Weight = 1;
            this.AnimationWalk.WeightChange = 0;
            this.AnimationWalk.Restart();
            this.AnimationWalk.Enabled = true;

            this.CurrentState.Apply(this);
            var actor = parent as Actor;
            actor.Work.Interrupt();
            this.Moving = true;
        }
        public void Stop(GameObject parent)
        {
            if (this.Acceleration == 0)
                return;
            this.Acceleration = 0;
            this.AnimationWalk.FadeOut();
            this.Moving = false;
        }

        public void Jump(GameObject parent)
        {
            if (parent.Net is Net.Server)
            {
                var force = Vector3.Zero;
                var feetposition = parent.Global + Vector3.UnitZ * 0.1f;
                var cell = parent.Net.Map.GetCell(feetposition);
                var block = cell.Block;// parent.Net.Map.GetBlock(parent.Global + Vector3.UnitZ * 0.1f); // to check if entity is in water
                var isStanding = PhysicsComponent.IsStanding(parent);
                if (!isStanding)
                    return;
                if (block == BlockDefOf.Fluid)
                {
                    if (parent.Velocity.Z <= 0)// only allow jumping in water when sinking
                    {
                        force = Vector3.UnitZ * PhysicsComponent.Jump;// * (1 + StatsComponent.GetStatOrDefault(parent, Stat.Types.JumpHeight, 0f));
                        var density = BlockDefOf.Fluid.GetDensity(cell.BlockData, feetposition);
                        force *= (1 + 3 * density);
                    }
                }
                else if (parent.Velocity.Z == 0 && isStanding)// parent.Net.Map.IsSolid(parent.Global - Vector3.UnitZ * 0.1f)) // TODO: FIX: doesnt jump if on block edge
                    force = Vector3.UnitZ * PhysicsComponent.Jump;// * (1 + StatsComponent.GetStatOrDefault(parent, Stat.Types.JumpHeight, 0f));

                if (force == Vector3.Zero)
                    return;
                parent.Physics.Applyforce(force);
                //parent.Velocity += force;
            }
            parent.Net.PostLocalEvent(parent, Message.Types.Jumped);
        }

        public void ToggleWalk(bool toggle)
        {
            if (this.CurrentState.Type != State.Types.Blocking)
                this.CurrentState = toggle ? State.States[State.Types.Walking] : State.States[State.Types.Running];
            this.CurrentState.Apply(this);
            
        }
        public void ToggleSprint(bool toggle)
        {
            if (this.CurrentState.Type != State.Types.Blocking)
                this.CurrentState = toggle ? State.States[State.Types.Sprinting] : State.States[State.Types.Running];
            this.CurrentState.Apply(this);
        }
        public void ToggleBlock(bool toggle)
        {
            this.CurrentState = toggle ? State.States[State.Types.Blocking] : State.States[State.Types.Running];
            this.CurrentState.Apply(this);

        }
        public MobileComponent ToggleCrouch(bool enabled)
        {
            this.AnimationCrouch.Weight = enabled ? 1 : 0;
            return this;
        }

        //public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        //{
        //    switch (e.Type)
        //    {
        //        case Message.Types.HitGround:
        //        case Message.Types.EntityHitGround:
        //            this.AnimationWalk.Frame = 0;
        //            this.OnLanded();
        //            break;

        //        default:
        //            break;
        //    }
        //    return false;
        //}
        public override void OnSpawn(MapBase newMap)
        {
            newMap.Events.ListenTo<EntityHitGroundEvent>(HandleEntityHitGround);
        }

        private void HandleEntityHitGround(EntityHitGroundEvent e)
        {
            if (e.Entity != this.Parent)
                return;
            this.AnimationWalk.Frame = 0;
            this.OnLanded();
        }

        private void OnLanded()
        {
            this.JumpCooldown = 1; // added a jump cooldown because the way it was set up, the ai can't correct its direction between consecutive jumps in behaviorgetat
        }

        public override void Tick()
        {
            var parent = this.Parent;
            var midair = parent.Physics.MidAir;
            if (this.JumpCooldown > 0)
                this.JumpCooldown--;

            this.AnimationJump.Weight = midair ? 1 : 0;

            if (!this.Moving)
                return;

            //don't change direction midair, or change it by a smaller factor?
            if (midair)
                return;

            Vector2 direction = parent.Transform.Direction;
            this.Acceleration = Math.Min(1, this.Acceleration + AccelerationStep);

            var stamina = parent.GetResource(ResourceDefOf.Stamina);

            var newwalk = StatDefOf.WalkSpeed.GetValue(parent);
            var walkSpeed = newwalk * Acceleration * NormalWalkSpeed * (this.CurrentState.Speed + this.CurrentState.SprintSpeed * stamina.Percentage);

            if (this.CurrentState.Type == State.Types.Sprinting)
                stamina.Adjust(-0.01f);

            //apply stamina
            // TODO: make stamina resource change walkspeed instead of fetching stamina from here

            if (walkSpeed == 0)
                Log.Enqueue(Log.EntryTypes.System, "Warning! " + parent.Name + " is trying to move but their movement speed is zero!");

            // if in mid-air, move at half speed
            if (midair)
            {
                this.AnimationJump.Weight = 1;
            }

            float walkX = direction.X * walkSpeed;
            float walkY = direction.Y * walkSpeed;

            PreventFall(parent, ref walkX, ref walkY);
            parent.Physics.Applyforce(new Vector3(walkX, walkY, 0)*.5f);
        }

        /// <summary>
        /// Prevents falling off edges when walking.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="walkX"></param>
        /// <param name="walkY"></param>
        private void PreventFall(GameObject parent, ref float walkX, ref float walkY)
        {
            if (this.CurrentState.Type != State.Types.Walking)
                return;
            // WE DONT WANT TO STOP MOVING WHEN JUMPING
            if (parent.Physics.MidAir)// parent.Velocity.Z != 0) 
                return;
            if (parent.GetPath() != null)
                return;
            var global = parent.Global;
            var g = parent.Map.Gravity;
            var map = parent.Map;

            /// code beloe prevents any fall by checking footprint corners instead of center position
            //if (parent.Physics.GetFootprintCorners(new Vector3(global.X + walkX, global.Y, global.Z + g)).All(p => !map.IsSolid(p)))
            //    walkX = 0;
            //if (parent.Physics.GetFootprintCorners(new Vector3(global.X, global.Y + walkY, global.Z + g)).All(p => !map.IsSolid(p)))
            //    walkY = 0;
            //if (parent.Physics.GetFootprintCorners(new Vector3(global.X + walkX, global.Y + walkY, global.Z + g)).All(p => !map.IsSolid(p)))
            //    walkY = walkX = 0;

            /// code below only prevents fall if the fall distance will be greater than a half block. allows stepping down from half blocks
            var halfBlock = Vector3.UnitZ * .5f;

            var walkXvec = new Vector3(global.X + walkX, global.Y, global.Z + g);
            if (!map.IsSolid(walkXvec) && !map.IsSolid(walkXvec - halfBlock))//.Below()))
                walkX = 0;

            var walkYvec = new Vector3(global.X, global.Y + walkY, global.Z + g);
            if (!map.IsSolid(walkYvec) && !map.IsSolid(walkYvec - halfBlock))//.Below()))
                walkY = 0;

            var walkXYvec = new Vector3(global.X + walkX, global.Y + walkY, global.Z + g);
            if (!map.IsSolid(walkXYvec) && !map.IsSolid(walkXYvec - halfBlock))//.Below()))
                walkY = walkX = 0;

            /// code below prevents any fall, even from half blocks
            //if (!map.IsSolid(new Vector3(global.X + walkX, global.Y, global.Z + g)))
            //    walkX = 0;
            //if (!map.IsSolid(new Vector3(global.X, global.Y + walkY, global.Z + g)))
            //    walkY = 0;
            //if (!map.IsSolid(new Vector3(global.X + walkX, global.Y + walkY, global.Z + g)))
            //    walkY = walkX = 0;
        }

        static public void EmitDust(GameObject parent)
        {
            if (parent.Net is Net.Client)
            {
                if (parent.Velocity.Z != 0)
                    return;
                //parent.Map.EventOccured(Message.Types.EntityFootStep, parent);
                parent.Map.World.Events.Post(new EntityFootStepEvent(parent as Entity));
            }
        }

        private static ParticleEmitterSphere CreateDust(GameObject parent)
        {
            var emitter = new ParticleEmitterSphere()
            {
                Lifetime = Ticks.PerSecond / 2f,
                Offset = Vector3.Zero,
                Rate = 0,
                ParticleWeight = 0f,//1f,
                ColorEnd = Color.White * .5f,
                ColorBegin = Color.White,
                SizeEnd = 1,
                SizeBegin = 3,
                Force = .01f
            };
            return emitter;
        }
        private static ParticleEmitterSphere CreateDirt(GameObject parent)
        {
            var block = parent.Map.GetBlock(parent.Global - Vector3.UnitZ * 0.1f);
            var dustcolor = block.DirtColor;
            var emitter = new ParticleEmitterSphere()
            {
                Lifetime = Ticks.PerSecond / 2f,
                Offset = Vector3.Zero,
                Rate = 0,
                ParticleWeight = 1f,
                ColorEnd = dustcolor * .5f,
                ColorBegin = dustcolor,
                SizeEnd = 1,
                SizeBegin = 1,
                Force = .05f
            };
            return emitter;
        }
        static readonly ParticleEmitterSphere DustEmitter = new ParticleEmitterSphere()
        {
            Lifetime = Ticks.PerSecond,
            Offset = Vector3.Zero,
            Rate = 0,
            ParticleWeight = 1f,
            ColorEnd = Color.SaddleBrown,
            ColorBegin = Color.SaddleBrown,
            SizeEnd = 1,
            SizeBegin = 1,
            Force = .05f
        };

        public override void Write(IDataWriter w)
        {
            base.Write(w);
            w.Write(this.Moving);
            w.Write(this.Acceleration);
            w.Write((int)this.CurrentState.Type);
            this.AnimationWalk.Write(w);
            this.AnimationJump.Write(w);
        }
        public override void Read(IDataReader r)
        {
            base.Read(r);
            this.Moving = r.ReadBoolean();
            this.Acceleration = r.ReadSingle();
            this.CurrentState = State.States[(State.Types)r.ReadInt32()];
            this.AnimationWalk.Read(r);
            this.AnimationJump.Read(r);
        }
        
        internal override void SaveExtra(SaveTag tag)
        {
            tag.Add(this.Moving.Save("Moving"));
            tag.Add(this.Acceleration.Save("Acceleration"));
            tag.Add(((int)this.CurrentState.Type).Save("State"));

            tag.Add(this.AnimationWalk.Save("Walking"));
            tag.Add(this.AnimationJump.Save("Jumping"));

        }
        internal override void LoadExtra(SaveTag tag)
        {
            tag.TryGetTagValueOrDefault<bool>("Moving", out this.Moving);
            tag.TryGetTagValueOrDefault<float>("Acceleration", out this.Acceleration);
            tag.TryGetTagValue<int>("State", v =>
            {
                this.CurrentState = State.States[(State.Types)v];
                this.CurrentState.Apply(this);
            });

            tag.TryGetTag("Walking", this.AnimationWalk.Load);
            tag.TryGetTag("Jumping", this.AnimationJump.Load);
        }
    }
}
