using Start_a_Town_.Animations;
using System;

namespace Start_a_Town_
{
    public class InteractionDef : Def
    {
        public readonly Type InteractionClass;
        public readonly InteractionLogic Logic;
        public AnimationDef Animation;
        public IInteractionProgressHandler ProgressHandler;
        public SkillDef Skill;
        public ToolUseDef ToolUse;
        public InteractionDef(string name, Type workerType) : base(name)
        {
            this.InteractionClass = typeof(Interaction);
            this.Logic = ActivatorSafe<InteractionLogic>.CreateInstance(workerType ?? typeof(InteractionLogic));
        }
        public InteractionDef(string name, Type interactionClass, Type workerType) : base(name)
        {
            this.InteractionClass = interactionClass;
            this.Logic = ActivatorSafe<InteractionLogic>.CreateInstance(workerType ?? typeof(InteractionLogic));
        }

        public Interaction Create(Actor actor, TargetArgs target)
        {
            var interaction = ActivatorSafe<Interaction>.CreateInstance(this.InteractionClass);
            interaction.Def = this;
            interaction.Context = this.CreateContext(actor, target);
            interaction.Initialize();
            return interaction;
        }

        internal InteractionContext CreateContext(Actor actor, TargetArgs target)
        {
            return this.Logic.CreateContext(actor, target);
        }
    }
    public class InteractionContext//(Actor actor, TargetArgs target)
    {
        //public Actor Actor = actor;
        //public TargetArgs Target = target;
        public Actor Actor;
        public TargetArgs Target;
        public virtual float ProgressPercentage { get; }

    }
}
