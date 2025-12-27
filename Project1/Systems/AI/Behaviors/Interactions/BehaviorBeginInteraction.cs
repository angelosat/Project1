using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.Components;
using System;

namespace Start_a_Town_
{
    class BehaviorBeginInteraction : Behavior
    {
        //public override string Status => $"{this.InteractionDef.Label} : {this.Target}";

        readonly int TargetInd;
        readonly TargetIndex CountInd;
        //TargetArgs Target { get => this.TargetGetter?.Invoke() ?? this.Actor.CurrentTask.GetTarget(this.TargetInd); set { } }
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
        public BehaviorBeginInteraction(TargetIndex targetInd, Interaction interaction) : this((int)targetInd, interaction)
        { }
        public BehaviorBeginInteraction(TargetIndex targetInd, Func<Interaction> interactionFactory) : this((int)targetInd, interactionFactory)
        { }
        public BehaviorBeginInteraction(InteractionDef def, TargetIndex targetInd = TargetIndex.A, TargetIndex countInd = TargetIndex.None)
        {
            this.InteractionDef = def;
            this.TargetInd = (int)targetInd;
            this.CountInd = countInd;
        }
        public BehaviorBeginInteraction(Func<TargetArgs> targetGetter, Func<Interaction> interactionFactory)
        {
            this.InteractionFactory = interactionFactory;
            this.TargetGetter = targetGetter;
        }
        public BehaviorBeginInteraction(TargetIndex targetInd)
        {
            this.TargetInd = (int)targetInd;
        }

        public BehaviorBeginInteraction(int targetInd, Func<Interaction> interactionFactory)
        {
            this.TargetInd = targetInd;
            this.InteractionFactory = interactionFactory;
        }
        public BehaviorBeginInteraction(int targetInd, Interaction interaction)
        {
            this.TargetInd = targetInd;
            this.Interaction = interaction;
        }
        public BehaviorBeginInteraction(TargetArgs targetArgs, Interaction interaction)
        {
            this.Target = targetArgs;
            this.Interaction = interaction;
        }
        public BehaviorBeginInteraction(Func<Interaction> interactionFactory)
        {
            this.InteractionFactory = interactionFactory;
        }
      
        public override BehaviorState Tick(Actor parent, AIState state)
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
            //Interaction goal = this.Interaction;
            int count = this.CountInd == TargetIndex.None ? -1 : parent.CurrentTask.GetAmount(this.CountInd);

            //var interaction = Actor.Work.Task;
            if(_interaction is null)
            {
                //_interaction = ActivatorSafe<Interaction>.CreateInstance(parent.CurrentTask.Def.Interaction.InteractionClass);
                _interaction = parent.CurrentTask.Def.Interaction.Create(parent, target);
            }

            //switch (goal.State)
            switch (_interaction.State)
            {
                case Interaction.States.Unstarted:
                    //throw new Exception();
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
            return new BehaviorBeginInteraction(this.TargetInd, this.InteractionFactory);// this.Interaction.Clone() as Interaction);
        }
    }
}
