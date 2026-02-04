using Project1.Framework.Animations;
using Project1.Framework.Base;
using Project1.Framework.Entities.Actors;
using Project1.Framework.Skills;
using Start_a_Town_;
using System;

namespace Project1.Framework.Interactions
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

        public Interaction Create(Actor actor, TargetArgs target, int count = -1)
        {
            var interaction = ActivatorSafe<Interaction>.CreateInstance(this.InteractionClass);
            interaction.Def = this;
            interaction.Context = this.CreateContext(actor, target, count);
            interaction.Initialize();
            return interaction;
        }

        internal InteractionContext CreateContext(Actor actor, TargetArgs target, int count)
        {
            return this.Logic.CreateContext(actor, target, count);
        }
    }
    public class InteractionContext//(Actor actor, TargetArgs target)
    {
        //public Actor Actor = actor;
        //public TargetArgs Target = target;
        public Actor Actor;
        public TargetArgs Target;
        public int Count;
        public virtual float ProgressPercentage { get; }

    }
}
