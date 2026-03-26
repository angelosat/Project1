using Project1.Core.Animations;
using Project1.Core.Entities.Actors;
using Project1.Core.Skills;
using Project1.Core.Systems.Tools;
using Project1.Framework.Helpers;
using System;

namespace Project1.Core.Interactions
{
    public enum InteractionRange
    {
        Touching,        // actor just needs to be adjacent
        Exact,           // actor must occupy the exact tile
        Any,             // actor can be anywhere “reasonable”
        InteractionSpot  // actor must reach a special spot (counter, workstation)
    }
    public class InteractionDef : Def
    {
        public readonly Type InteractionClass;
        public readonly InteractionLogic Logic;
        public AnimationDef Animation;
        public IInteractionProgressHandler ProgressHandler;
        public SkillDef Skill;
        public ToolUseDef ToolUse;
        public InteractionRange Range = InteractionRange.Touching;
        public InteractionDef(string name, Type workerType) : base(name)
        {
            this.InteractionClass = typeof(Interaction);
            this.Logic = ActivatorSafe<InteractionLogic>.CreateInstance(workerType ?? typeof(InteractionLogic));
        }
        public InteractionDef(string name, Type workerType, IInteractionProgressHandler progressHandler) : this(name, workerType)
        {
            this.ProgressHandler = progressHandler;
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
            //interaction.Initialize();
            return interaction;
        }

        internal InteractionContext CreateContext(Actor actor, TargetArgs target, int count)
        {
            return this.Logic.CreateContext(actor, target, count);
        }
    }
}
