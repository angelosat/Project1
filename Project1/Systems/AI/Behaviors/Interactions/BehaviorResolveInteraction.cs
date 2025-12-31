using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.Components;
using System;

namespace Start_a_Town_
{
    class BehaviorResolveInteraction : Behavior
    {
        readonly int TargetInd;
        readonly TargetIndex CountInd;
        TargetArgs Target { get => this.TargetGetter?.Invoke() ?? (this.TargetInd != (int)TargetIndex.None ? this.Actor.CurrentTask.GetTarget(this.TargetInd) : null); set { } }
        Interaction _interaction;
        public InteractionDef InteractionDef;
        public Func<Interaction> InteractionFactory;
        readonly Func<TargetArgs> TargetGetter;
        Interaction Interaction
        {
            get
            {
                if (this._interaction is null)
                    this._interaction = this.InteractionFactory?.Invoke() ?? ActivatorSafe<Interaction>.CreateInstance(this.InteractionDef.InteractionClass);
                return this._interaction;
            }
            set => this._interaction = value;
        }
        public BehaviorResolveInteraction(TargetIndex targetInd, Interaction interaction) : this((int)targetInd, interaction)
        { }
        public BehaviorResolveInteraction(TargetIndex targetInd, Func<Interaction> interactionFactory) : this((int)targetInd, interactionFactory)
        { }
        public BehaviorResolveInteraction(InteractionDef def, TargetIndex targetInd = TargetIndex.A, TargetIndex countInd = TargetIndex.None)
        {
            this.InteractionDef = def;
            this.TargetInd = (int)targetInd;
            this.CountInd = countInd;
        }
        public BehaviorResolveInteraction(Func<TargetArgs> targetGetter, Func<Interaction> interactionFactory)
        {
            this.InteractionFactory = interactionFactory;
            this.TargetGetter = targetGetter;
        }
        public BehaviorResolveInteraction(TargetIndex targetInd)
        {
            this.TargetInd = (int)targetInd;
        }
        public BehaviorResolveInteraction()
        {
            this.TargetInd = (int)TargetIndex.A;
        }
        public BehaviorResolveInteraction(int targetInd, Func<Interaction> interactionFactory)
        {
            this.TargetInd = targetInd;
            this.InteractionFactory = interactionFactory;
        }
        public BehaviorResolveInteraction(int targetInd, Interaction interaction)
        {
            this.TargetInd = targetInd;
            this.Interaction = interaction;
        }
        public BehaviorResolveInteraction(TargetArgs targetArgs, Interaction interaction)
        {
            this.Target = targetArgs;
            this.Interaction = interaction;
        }
        public BehaviorResolveInteraction(Func<Interaction> interactionFactory)
        {
            this.InteractionFactory = interactionFactory;
        }
        public override BehaviorState Tick(Actor actor, AIState state)
        {
            this.Actor = actor;
            if (actor.Velocity != Vector3.Zero)
            {
                var acceleration = actor.GetComponent<MobileComponent>().Acceleration;
                if (acceleration != 0)
                    actor.MoveToggle(false);
                return BehaviorState.Running;
            }

            var target = this.Target;
            int count = this.CountInd == TargetIndex.None ? -1 : actor.CurrentTask.GetAmount(this.CountInd);

            this._interaction ??= actor.Work.Perform(actor.CurrentTask.Def.Interaction, target);
            //_interaction ??= actor.CurrentTask.Def.Interaction.Create(actor, target);

            if(this._interaction.IsFinished)
                return BehaviorState.Success;
            return BehaviorState.Running;

            switch (_interaction.State)
            {
                case Interaction.States.Unstarted:
                    AIManager.Interact(actor, _interaction, target, count);
                    return BehaviorState.Running;

                case Interaction.States.Running:
                    return BehaviorState.Running;

                case Interaction.States.Finished:
                    this.Interaction = null; // THIS IS REQUIRED because when ths behavior is run again (in loops) it needs to create a new instance of the interaction
                    // WHY HAVE I COMMENTED IT OUT?
                    return BehaviorState.Success;

                case Interaction.States.Failed:
                    return BehaviorState.Fail;

                default:
                    break;
            }
            return BehaviorState.Running;
        }
        public BehaviorState TickOld(Actor parent, AIState state)
        {
            this.Actor = parent;
            if (parent.Velocity != Vector3.Zero)
            {
                var acceleration = parent.GetComponent<MobileComponent>().Acceleration;
                if (acceleration != 0)
                    parent.MoveToggle(false);
                return BehaviorState.Running;
            }

            var target = this.Target;
            int count = this.CountInd == TargetIndex.None ? -1 : parent.CurrentTask.GetAmount(this.CountInd);

            _interaction ??= parent.CurrentTask.Def.Interaction.Create(parent, target);

            switch (_interaction.State)
            {
                case Interaction.States.Unstarted:
                    AIManager.Interact(parent, _interaction, target, count);
                    return BehaviorState.Running;

                case Interaction.States.Running:
                    return BehaviorState.Running;

                case Interaction.States.Finished:
                    this.Interaction = null; // THIS IS REQUIRED because when ths behavior is run again (in loops) it needs to create a new instance of the interaction
                    // WHY HAVE I COMMENTED IT OUT?
                    return BehaviorState.Success;

                case Interaction.States.Failed:
                    return BehaviorState.Fail;

                default:
                    break;
            }
            return BehaviorState.Running;
        }
        internal override void ObjectLoaded(GameObject parent)
        {
            // TODO: don't replace fresh stored interaction with null, if parent isn't currently interacting? very hacky
            this.Interaction = parent.GetComponent<WorkComponent>().Task ?? this.Interaction;
        }
        public override object Clone()
        {
            return new BehaviorResolveInteraction(this.TargetInd, this.InteractionFactory);// this.Interaction.Clone() as Interaction);
        }
    }
}
