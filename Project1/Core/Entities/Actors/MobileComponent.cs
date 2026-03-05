using Microsoft.Xna.Framework;
using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Graphics.Particles;
using Project1.Core.Networking;
using Project1.Core.Resources;
using Project1.Core.Simulation;
using Project1.Core.Simulation.Physics;
using Project1.Core.Stats;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;

namespace Project1.Core.Entities.Actors
{
    public class MobileComponent : EntityComp
    {
        public override EntityCompDef CompDef => EntityCompDefOf.Mobile;
        public new class Spec : Spec<MobileComponent> { }

        class State(float speed, float sprintSpeed, float animationWeight, float animationSpeed, bool allowJump)
        {
            public enum Types : byte { Walking, Running, Sprinting, Blocking };
            public Types Type;
            public float Speed = speed;
            public float SprintSpeed = sprintSpeed;
            public float AnimationWeight = animationWeight;
            public float AnimationSpeed = animationSpeed;
            public bool AllowJump = allowJump;
        }

        public const float NormalWalkSpeed = .1f;// 0.08f; when i used friction wrongly

        static readonly State[] States = new State[4];
        static MobileComponent()
        {
            States[(byte)State.Types.Walking] = new(speed: .5f, sprintSpeed: 0, animationWeight: .33f, animationSpeed: 1f, allowJump: false);
            States[(byte)State.Types.Running] = new(speed: 1f, sprintSpeed: 0, animationWeight: .66f, animationSpeed: 1f, allowJump: true);
            States[(byte)State.Types.Sprinting] = new(speed: 1f, sprintSpeed: 0.5f, animationWeight: 1f, animationSpeed: 1.2f, allowJump: true);
            States[(byte)State.Types.Blocking] = new(speed: 0.5f, sprintSpeed: 0, animationWeight: 0.5f, animationSpeed: 1, allowJump: false);
        }
        public override string Name { get; } = "Mobile";

        public float Acceleration;

        Animation AnimationWalk => field ??= this.Owner.SpriteComp.GetAnimation(AnimationDefOf.Walk);
        Animation AnimationJump => field ??= this.Owner.SpriteComp.GetAnimation(AnimationDefOf.Jump);
        Animation AnimationCrouch => field ??= this.Owner.SpriteComp.GetAnimation(AnimationDefOf.Crouch);

        public bool Moving;

        public float Speed => this.CurrentState.Speed;
        State.Types _currentState;
        State CurrentState => States[(byte)this._currentState];
        int JumpCooldown;
        public bool CanJump => this.JumpCooldown == 0;

        internal bool Crouching => this.AnimationCrouch.Weight > 0;
        const float AccelerationStep = .1f;
        public MobileComponent()
        {
            this.Acceleration = 0f;
            this.Moving = false;
            this._currentState = State.Types.Running;
        }

        void Apply(State.Types state)
        {
            this._currentState = state;
            this.AnimationWalk.Weight = this.CurrentState.AnimationWeight;
            this.AnimationWalk.Speed = this.CurrentState.AnimationSpeed;
        }

        static public void OnFootstep(Entity parent)
        {
            if (parent.Velocity.Z != 0)
                return;
            parent.Map.Events.Post(new ActorFootStepEvent(parent));
        }

        internal override void InitializeOnce()
        {
            this.Owner.SpriteComp.AddAnimation(AnimationDefOf.Jump, weight: 0);
            this.Owner.SpriteComp.AddAnimation(AnimationDefOf.Walk, weight: 0);
            this.Owner.SpriteComp.AddAnimation(AnimationDefOf.Crouch, weight: 0);
        }

        public void Toggle(GameObject parent, bool toggle)
        {
            if (toggle)
                this.Start(parent);
            else
                this.Stop(parent);
        }
      
        public void Start(GameObject parent)//, State state)
        {
            if (this.Moving)
                return;
            this.Acceleration = AccelerationStep;

            this.AnimationWalk.Weight = 1;
            this.AnimationWalk.WeightChange = 0;
            this.AnimationWalk.Restart();
            this.AnimationWalk.Enabled = true;

            this.Apply(this._currentState);
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
            if (parent.Net is Server)
            {
                var force = Vector3.Zero;
                var feetposition = parent.Global + Vector3.UnitZ * 0.1f;
                var cell = parent.Net.Map.GetCell(feetposition);
                var block = cell.Block;// parent.Net.Map.GetBlock(parent.Global + Vector3.UnitZ * 0.1f); // to check if entity is in water
                var isStanding = PhysicsComp.IsStanding(parent);
                if (!isStanding)
                    return;
                if (block == BlockDefOf.Fluid.Block)
                {
                    if (parent.Velocity.Z <= 0)// only allow jumping in water when sinking
                    {
                        force = Vector3.UnitZ * PhysicsComp.Jump;// * (1 + StatsComponent.GetStatOrDefault(parent, Stat.Types.JumpHeight, 0f));
                        var density = BlockDefOf.Fluid.Block.GetDensity(cell.BlockData, feetposition);
                        force *= (1 + 3 * density);
                    }
                }
                else if (parent.Velocity.Z == 0 && isStanding)// parent.Net.Map.IsSolid(parent.Global - Vector3.UnitZ * 0.1f)) // TODO: FIX: doesnt jump if on block edge
                    force = Vector3.UnitZ * PhysicsComp.Jump;// * (1 + StatsComponent.GetStatOrDefault(parent, Stat.Types.JumpHeight, 0f));

                if (force == Vector3.Zero)
                    return;
                parent.Physics.Applyforce(force);
                //parent.Velocity += force;
            }
        }

        public void ToggleWalk(bool toggle)
        {
            if (this._currentState != State.Types.Blocking)
                this.Apply(toggle ? State.Types.Walking : State.Types.Running);
        }
        public void ToggleSprint(bool toggle)
        {
            if (this._currentState != State.Types.Blocking)
                this.Apply(toggle ? State.Types.Sprinting : State.Types.Running);
        }
        public void ToggleBlock(bool toggle)
        {
            this.Apply(toggle ? State.Types.Blocking : State.Types.Running);
        }
        public MobileComponent ToggleCrouch(bool enabled)
        {
            this.AnimationCrouch.Weight = enabled ? 1 : 0;
            return this;
        }

        public override void OnSpawn(MapBase newMap)
        {
            newMap.Events.ListenTo<EntityHitGroundEvent>(HandleEntityHitGround);
        }

        private void HandleEntityHitGround(EntityHitGroundEvent e)
        {
            if (e.Entity != this.Owner)
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
            var parent = this.Owner;
            var midair = parent.Physics.MidAir;
            if (this.JumpCooldown > 0)
                this.JumpCooldown--;

            if (!this.Moving)
                return;

            //don't change direction midair, or change it by a smaller factor?
            if (midair)
            {
                //this.AnimationJump.Weight = 1;
                return;
            }
            Vector2 direction = parent.Transform.Direction;
            this.Acceleration = Math.Min(1, this.Acceleration + AccelerationStep);

            var resources = parent.GetComponent<ResourcesComponent>();
            var newwalk = StatDefOf.WalkSpeed.CalculateFor(parent);
            var walkSpeed = newwalk * Acceleration * NormalWalkSpeed * (this.CurrentState.Speed + this.CurrentState.SprintSpeed * resources.GetPercentage(ResourceDefOf.Stamina));
            if (this._currentState == State.Types.Sprinting)
                resources.ApplyDelta(ResourceDefOf.Stamina, -0.01f);

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
            parent.Physics.Applyforce(new Vector3(walkX, walkY, 0) * .5f);
        }

        /// <summary>
        /// Prevents falling off edges when walking.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="walkX"></param>
        /// <param name="walkY"></param>
        private void PreventFall(GameObject parent, ref float walkX, ref float walkY)
        {
            if (this._currentState != State.Types.Walking)
                return;
            // WE DONT WANT TO STOP MOVING WHEN JUMPING
            if (parent.Physics.MidAir)// parent.Velocity.Z != 0) 
                return;
            var actor = parent as Actor;
            if (actor.AI.State.Path is not null)
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
        }
        public override void Read(IDataReader r)
        {
            base.Read(r);
            this.Moving = r.ReadBoolean();
            this.Acceleration = r.ReadSingle();
            //this.CurrentState = State.States[(State.Types)r.ReadInt32()];
            this._currentState = (State.Types)r.ReadInt32();
        }

        internal override void SaveExtra(SaveTag tag)
        {
            tag.Add(this.Moving.Save("Moving"));
            tag.Add(this.Acceleration.Save("Acceleration"));
            tag.Add(((int)this.CurrentState.Type).Save("State"));
        }
        internal override void LoadExtra(SaveTag tag)
        {
            tag.TryGetTagValueOrDefault<bool>("Moving", out this.Moving);
            tag.TryGetTagValueOrDefault<float>("Acceleration", out this.Acceleration);
            tag.TryGetTagValue<int>("State", v =>
            {
                //this.CurrentState = State.States[(State.Types)v];
                this._currentState = (State.Types)v;
            });
        }
    }
    //public class MobileComponent : EntityComp
    //{
    //    public override EntityCompDef CompDef => EntityCompDefOf.Mobile;
    //    public new class Spec : Spec<MobileComponent> { }

    //    class State
    //    {
    //        public enum Types { Walking, Running, Sprinting, Blocking };
    //        public Types Type;
    //        public string Name;
    //        public float Speed;
    //        public float SprintSpeed;
    //        public float AnimationWeight;
    //        public float AnimationSpeed;
    //        public bool AllowJump;
    //        public State(Types type, float speed, float sprintSpeed, float animationWeight, float animationSpeed, bool allowJump)
    //        {
    //            this.Type = type;
    //            this.Name = type.ToString();
    //            this.Speed = speed;
    //            this.SprintSpeed = sprintSpeed;
    //            this.AnimationWeight = animationWeight;
    //            this.AnimationSpeed = animationSpeed;
    //            this.AllowJump = allowJump;
    //        }
    //        public void Apply(MobileComponent component)
    //        {
    //            //if (!component.Moving)
    //            //    component.AnimationWalk.Frame = 0;
    //            component.AnimationWalk.Weight = this.AnimationWeight;
    //            component.AnimationWalk.Speed = this.AnimationSpeed;
    //        }
    //        public override string ToString()
    //        {
    //            return this.Name;
    //        }
    //        //static readonly State Walking = new(Types.Walking, speed: 0.66f, sprintSpeed: 0, animationWeight: 0.5f, animationSpeed: 1, allowJump: false);
    //        static readonly State Walking = new(Types.Walking, speed: .5f, sprintSpeed: 0, animationWeight: .33f, animationSpeed: 1f, allowJump: false);
    //        //static readonly State Running = new(Types.Running, speed: 1f, sprintSpeed: 0, animationWeight: 0.75f, animationSpeed: 1f, allowJump: true);
    //        static readonly State Running = new(Types.Running, speed: 1f, sprintSpeed: 0, animationWeight: .66f, animationSpeed: 1f, allowJump: true);
    //        static readonly State Sprinting = new(Types.Sprinting, speed: 1f, sprintSpeed: 0.5f, animationWeight: 1f, animationSpeed: 1.2f, allowJump: true);
    //        static readonly State Blocking = new(Types.Blocking, speed: 0.5f, sprintSpeed: 0, animationWeight: 0.5f, animationSpeed: 1, allowJump: false);

    //        static public Dictionary<Types, State> States = new()
    //        {
    //            {Types.Walking, Walking },
    //            {Types.Running, Running },
    //            {Types.Sprinting, Sprinting },
    //            {Types.Blocking, Blocking }
    //        };
    //    }

    //    public const float NormalWalkSpeed = .1f;// 0.08f; when i used friction wrongly

    //    public override string Name { get; } = "Mobile";

    //    public float Acceleration;

    //    Animation AnimationWalk => field ??= this.Owner.SpriteComp.GetAnimation(AnimationDefOf.Walk);
    //    Animation AnimationJump => field ??= this.Owner.SpriteComp.GetAnimation(AnimationDefOf.Jump);
    //    Animation AnimationCrouch => field ??= this.Owner.SpriteComp.GetAnimation(AnimationDefOf.Crouch);

    //    public bool Moving;

    //    public float Speed => this.CurrentState.Speed;
    //    State CurrentState;
    //    int JumpCooldown;
    //    public bool CanJump => this.JumpCooldown == 0;

    //    internal bool Crouching => this.AnimationCrouch.Weight > 0;
    //    const float AccelerationStep = .1f;
    //    public MobileComponent()
    //    {
    //        this.Acceleration = 0f;
    //        this.Moving = false;
    //        this.CurrentState = State.States[State.Types.Running];
    //    }

    //    static public void OnFootstep(Entity parent)
    //    {
    //        if (parent.Velocity.Z != 0)
    //            return;
    //        parent.Map.Events.Post(new ActorFootStepEvent(parent));
    //    }

    //    internal override void InitializeOnce()
    //    {
    //        this.Owner.SpriteComp.AddAnimation(AnimationDefOf.Jump, weight: 0);
    //        this.Owner.SpriteComp.AddAnimation(AnimationDefOf.Walk, weight: 0);
    //        this.Owner.SpriteComp.AddAnimation(AnimationDefOf.Crouch, weight: 0);
    //    }

    //    public void Toggle(GameObject parent, bool toggle)
    //    {
    //        if (toggle)
    //            this.Start(parent);
    //        else
    //            this.Stop(parent);
    //    }
    //    public void Start(GameObject parent)
    //    {
    //        this.Start(parent, this.CurrentState);
    //    }
    //    void Start(GameObject parent, State state)
    //    {
    //        if (this.Moving)
    //            return;
    //        this.CurrentState = state;
    //        this.Acceleration = AccelerationStep;

    //        this.AnimationWalk.Weight = 1;
    //        this.AnimationWalk.WeightChange = 0;
    //        this.AnimationWalk.Restart();
    //        this.AnimationWalk.Enabled = true;

    //        this.CurrentState.Apply(this);
    //        var actor = parent as Actor;
    //        actor.Work.Interrupt();
    //        this.Moving = true;
    //    }
    //    public void Stop(GameObject parent)
    //    {
    //        if (this.Acceleration == 0)
    //            return;
    //        this.Acceleration = 0;
    //        this.AnimationWalk.FadeOut();
    //        this.Moving = false;
    //    }

    //    public void Jump(GameObject parent)
    //    {
    //        if (parent.Net is Server)
    //        {
    //            var force = Vector3.Zero;
    //            var feetposition = parent.Global + Vector3.UnitZ * 0.1f;
    //            var cell = parent.Net.Map.GetCell(feetposition);
    //            var block = cell.Block;// parent.Net.Map.GetBlock(parent.Global + Vector3.UnitZ * 0.1f); // to check if entity is in water
    //            var isStanding = PhysicsComp.IsStanding(parent);
    //            if (!isStanding)
    //                return;
    //            if (block == BlockDefOf.Fluid.Block)
    //            {
    //                if (parent.Velocity.Z <= 0)// only allow jumping in water when sinking
    //                {
    //                    force = Vector3.UnitZ * PhysicsComp.Jump;// * (1 + StatsComponent.GetStatOrDefault(parent, Stat.Types.JumpHeight, 0f));
    //                    var density = BlockDefOf.Fluid.Block.GetDensity(cell.BlockData, feetposition);
    //                    force *= (1 + 3 * density);
    //                }
    //            }
    //            else if (parent.Velocity.Z == 0 && isStanding)// parent.Net.Map.IsSolid(parent.Global - Vector3.UnitZ * 0.1f)) // TODO: FIX: doesnt jump if on block edge
    //                force = Vector3.UnitZ * PhysicsComp.Jump;// * (1 + StatsComponent.GetStatOrDefault(parent, Stat.Types.JumpHeight, 0f));

    //            if (force == Vector3.Zero)
    //                return;
    //            parent.Physics.Applyforce(force);
    //            //parent.Velocity += force;
    //        }
    //        //parent.Net.PostLocalEvent(parent, Message.Types.Jumped);
    //    }

    //    public void ToggleWalk(bool toggle)
    //    {
    //        if (this.CurrentState.Type != State.Types.Blocking)
    //            this.CurrentState = toggle ? State.States[State.Types.Walking] : State.States[State.Types.Running];
    //        this.CurrentState.Apply(this);

    //    }
    //    public void ToggleSprint(bool toggle)
    //    {
    //        if (this.CurrentState.Type != State.Types.Blocking)
    //            this.CurrentState = toggle ? State.States[State.Types.Sprinting] : State.States[State.Types.Running];
    //        this.CurrentState.Apply(this);
    //    }
    //    public void ToggleBlock(bool toggle)
    //    {
    //        this.CurrentState = toggle ? State.States[State.Types.Blocking] : State.States[State.Types.Running];
    //        this.CurrentState.Apply(this);

    //    }
    //    public MobileComponent ToggleCrouch(bool enabled)
    //    {
    //        this.AnimationCrouch.Weight = enabled ? 1 : 0;
    //        return this;
    //    }

    //    public override void OnSpawn(MapBase newMap)
    //    {
    //        newMap.Events.ListenTo<EntityHitGroundEvent>(HandleEntityHitGround);
    //    }

    //    private void HandleEntityHitGround(EntityHitGroundEvent e)
    //    {
    //        if (e.Entity != this.Owner)
    //            return;
    //        this.AnimationWalk.Frame = 0;
    //        this.OnLanded();
    //    }

    //    private void OnLanded()
    //    {
    //        this.JumpCooldown = 1; // added a jump cooldown because the way it was set up, the ai can't correct its direction between consecutive jumps in behaviorgetat
    //    }

    //    public override void Tick()
    //    {
    //        var parent = this.Owner;
    //        var midair = parent.Physics.MidAir;
    //        if (this.JumpCooldown > 0)
    //            this.JumpCooldown--;

    //        //this.AnimationJump.Weight = midair ? 1 : 0;
    //        //this.AnimationJump.Weight = 0; // midair ? 1 : 0;

    //        if (!this.Moving)
    //            return;

    //        //don't change direction midair, or change it by a smaller factor?
    //        if (midair)
    //        {
    //            //this.AnimationJump.Weight = 1;
    //            return;
    //        }
    //        Vector2 direction = parent.Transform.Direction;
    //        this.Acceleration = Math.Min(1, this.Acceleration + AccelerationStep);

    //        //var stamina = parent.GetResource(ResourceDefOf.Stamina);
    //        //var newwalk = StatDefOf.WalkSpeed.CalculateFor(parent);
    //        //var walkSpeed = newwalk * Acceleration * NormalWalkSpeed * (this.CurrentState.Speed + this.CurrentState.SprintSpeed * stamina.Percentage);
    //        //if (this.CurrentState.Type == State.Types.Sprinting)
    //        //    stamina.ApplyDelta(-0.01f);


    //        var resources = parent.GetComponent<ResourcesComponent>();
    //        var newwalk = StatDefOf.WalkSpeed.CalculateFor(parent);
    //        var walkSpeed = newwalk * Acceleration * NormalWalkSpeed * (this.CurrentState.Speed + this.CurrentState.SprintSpeed * resources.GetPercentage(ResourceDefOf.Stamina));
    //        if (this.CurrentState.Type == State.Types.Sprinting)
    //            resources.ApplyDelta(ResourceDefOf.Stamina , - 0.01f);

    //        //apply stamina
    //        // TODO: make stamina resource change walkspeed instead of fetching stamina from here

    //        if (walkSpeed == 0)
    //            Log.Enqueue(Log.EntryTypes.System, "Warning! " + parent.Name + " is trying to move but their movement speed is zero!");

    //        // if in mid-air, move at half speed
    //        if (midair)
    //        {
    //            this.AnimationJump.Weight = 1;
    //        }

    //        float walkX = direction.X * walkSpeed;
    //        float walkY = direction.Y * walkSpeed;

    //        PreventFall(parent, ref walkX, ref walkY);
    //        parent.Physics.Applyforce(new Vector3(walkX, walkY, 0) * .5f);
    //    }

    //    /// <summary>
    //    /// Prevents falling off edges when walking.
    //    /// </summary>
    //    /// <param name="parent"></param>
    //    /// <param name="walkX"></param>
    //    /// <param name="walkY"></param>
    //    private void PreventFall(GameObject parent, ref float walkX, ref float walkY)
    //    {
    //        if (this.CurrentState.Type != State.Types.Walking)
    //            return;
    //        // WE DONT WANT TO STOP MOVING WHEN JUMPING
    //        if (parent.Physics.MidAir)// parent.Velocity.Z != 0) 
    //            return;
    //        var actor = parent as Actor;
    //        if (actor.AI.State.Path is not null)
    //            return;
    //        var global = parent.Global;
    //        var g = parent.Map.Gravity;
    //        var map = parent.Map;

    //        /// code beloe prevents any fall by checking footprint corners instead of center position
    //        //if (parent.Physics.GetFootprintCorners(new Vector3(global.X + walkX, global.Y, global.Z + g)).All(p => !map.IsSolid(p)))
    //        //    walkX = 0;
    //        //if (parent.Physics.GetFootprintCorners(new Vector3(global.X, global.Y + walkY, global.Z + g)).All(p => !map.IsSolid(p)))
    //        //    walkY = 0;
    //        //if (parent.Physics.GetFootprintCorners(new Vector3(global.X + walkX, global.Y + walkY, global.Z + g)).All(p => !map.IsSolid(p)))
    //        //    walkY = walkX = 0;

    //        /// code below only prevents fall if the fall distance will be greater than a half block. allows stepping down from half blocks
    //        var halfBlock = Vector3.UnitZ * .5f;

    //        var walkXvec = new Vector3(global.X + walkX, global.Y, global.Z + g);
    //        if (!map.IsSolid(walkXvec) && !map.IsSolid(walkXvec - halfBlock))//.Below()))
    //            walkX = 0;

    //        var walkYvec = new Vector3(global.X, global.Y + walkY, global.Z + g);
    //        if (!map.IsSolid(walkYvec) && !map.IsSolid(walkYvec - halfBlock))//.Below()))
    //            walkY = 0;

    //        var walkXYvec = new Vector3(global.X + walkX, global.Y + walkY, global.Z + g);
    //        if (!map.IsSolid(walkXYvec) && !map.IsSolid(walkXYvec - halfBlock))//.Below()))
    //            walkY = walkX = 0;

    //        /// code below prevents any fall, even from half blocks
    //        //if (!map.IsSolid(new Vector3(global.X + walkX, global.Y, global.Z + g)))
    //        //    walkX = 0;
    //        //if (!map.IsSolid(new Vector3(global.X, global.Y + walkY, global.Z + g)))
    //        //    walkY = 0;
    //        //if (!map.IsSolid(new Vector3(global.X + walkX, global.Y + walkY, global.Z + g)))
    //        //    walkY = walkX = 0;
    //    }

    //    private static ParticleEmitterSphere CreateDust(GameObject parent)
    //    {
    //        var emitter = new ParticleEmitterSphere()
    //        {
    //            Lifetime = Ticks.PerSecond / 2f,
    //            Offset = Vector3.Zero,
    //            Rate = 0,
    //            ParticleWeight = 0f,//1f,
    //            ColorEnd = Color.White * .5f,
    //            ColorBegin = Color.White,
    //            SizeEnd = 1,
    //            SizeBegin = 3,
    //            Force = .01f
    //        };
    //        return emitter;
    //    }
    //    private static ParticleEmitterSphere CreateDirt(GameObject parent)
    //    {
    //        var block = parent.Map.GetBlock(parent.Global - Vector3.UnitZ * 0.1f);
    //        var dustcolor = block.DirtColor;
    //        var emitter = new ParticleEmitterSphere()
    //        {
    //            Lifetime = Ticks.PerSecond / 2f,
    //            Offset = Vector3.Zero,
    //            Rate = 0,
    //            ParticleWeight = 1f,
    //            ColorEnd = dustcolor * .5f,
    //            ColorBegin = dustcolor,
    //            SizeEnd = 1,
    //            SizeBegin = 1,
    //            Force = .05f
    //        };
    //        return emitter;
    //    }
    //    static readonly ParticleEmitterSphere DustEmitter = new ParticleEmitterSphere()
    //    {
    //        Lifetime = Ticks.PerSecond,
    //        Offset = Vector3.Zero,
    //        Rate = 0,
    //        ParticleWeight = 1f,
    //        ColorEnd = Color.SaddleBrown,
    //        ColorBegin = Color.SaddleBrown,
    //        SizeEnd = 1,
    //        SizeBegin = 1,
    //        Force = .05f
    //    };

    //    public override void Write(IDataWriter w)
    //    {
    //        base.Write(w);
    //        w.Write(this.Moving);
    //        w.Write(this.Acceleration);
    //        w.Write((int)this.CurrentState.Type);
    //    }
    //    public override void Read(IDataReader r)
    //    {
    //        base.Read(r);
    //        this.Moving = r.ReadBoolean();
    //        this.Acceleration = r.ReadSingle();
    //        this.CurrentState = State.States[(State.Types)r.ReadInt32()];
    //    }

    //    internal override void SaveExtra(SaveTag tag)
    //    {
    //        tag.Add(this.Moving.Save("Moving"));
    //        tag.Add(this.Acceleration.Save("Acceleration"));
    //        tag.Add(((int)this.CurrentState.Type).Save("State"));
    //    }
    //    internal override void LoadExtra(SaveTag tag)
    //    {
    //        tag.TryGetTagValueOrDefault<bool>("Moving", out this.Moving);
    //        tag.TryGetTagValueOrDefault<float>("Acceleration", out this.Acceleration);
    //        tag.TryGetTagValue<int>("State", v =>
    //        {
    //            this.CurrentState = State.States[(State.Types)v];
    //        });
    //    }
    //}
}
