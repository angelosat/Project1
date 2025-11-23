using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.Components;
using System;

namespace Start_a_Town_
{
    class BehaviorInteractionNew : Behavior
    {
        readonly int TargetInd;
        TargetArgs Target { get => this.TargetGetter?.Invoke() ?? this.Actor.CurrentTask.GetTarget(this.TargetInd); set { } }
        Interaction _Interaction;
        public Func<Interaction> InteractionFactory;
        readonly Func<TargetArgs> TargetGetter;
        Interaction Interaction
        {
            get
            {
                if (this._Interaction is null)
                    this._Interaction = this.InteractionFactory();
                return this._Interaction;
            }
            set => this._Interaction = value;
        }
        public BehaviorInteractionNew(TargetIndex targetInd, Interaction interaction) : this((int)targetInd, interaction)
        { }
        public BehaviorInteractionNew(TargetIndex targetInd, Func<Interaction> interactionFactory) : this((int)targetInd, interactionFactory)
        { }
        public BehaviorInteractionNew(Func<TargetArgs> targetGetter, Func<Interaction> interactionFactory)
        {
            this.InteractionFactory = interactionFactory;
            this.TargetGetter = targetGetter;
        }
        public BehaviorInteractionNew(TargetIndex targetInd)
        {
            this.TargetInd = (int)targetInd;
        }

        public BehaviorInteractionNew(int targetInd, Func<Interaction> interactionFactory)
        {
            this.TargetInd = targetInd;
            this.InteractionFactory = interactionFactory;
        }
        public BehaviorInteractionNew(int targetInd, Interaction interaction)
        {
            this.TargetInd = targetInd;
            this.Interaction = interaction;
        }
        public BehaviorInteractionNew(TargetArgs targetArgs, Interaction interaction)
        {
            this.Target = targetArgs;
            this.Interaction = interaction;
        }
        public BehaviorInteractionNew(Func<Interaction> interactionFactory)
        {
            this.InteractionFactory = interactionFactory;
        }
      
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            this.Actor = parent;
            if (parent.Velocity != Vector3.Zero)
            {
                var acceleration = parent.GetComponent<MobileComponent>().Acceleration;
                if (acceleration != 0)
                    parent.MoveToggle(false);
                return BehaviorState.Running;
            }

            TargetArgs target = this.Target;
            Interaction goal = this.Interaction;

            switch (goal.State)
            {
                case Interaction.States.Unstarted:
                    AIManager.Interact(parent, goal, target);
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
            return new BehaviorInteractionNew(this.TargetInd, this.InteractionFactory);// this.Interaction.Clone() as Interaction);
        }
    }
}
