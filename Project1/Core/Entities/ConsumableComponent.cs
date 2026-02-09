using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Project1.Core.Effects;
using Project1.Core.Mood;
using Project1.Core.Needs;
using Project1.Core.Helpers;
using Project1.Core.Interactions;
using Project1.Core.Legacy;
using Project1.Core.Legacy.Properties;
using Project1.Framework.UI;
using Project1.Framework.Serialization;
using Project1.Framework;

namespace Project1.Core.Entities
{
    enum ConsumableType { Food, Drink }
    class Verbs
    {
        static public string Consume { get { return "Consume"; } }
        static public string Eat { get { return "Eat"; } }
        static public string Drink { get { return "Drink"; } }
    }

    public class ConsumableComponent : EntityComp
    {
        public override string Name { get; } = "Consumable";

        public List<EntityEffectWrapper> EffectsNew = [];
        public GameObject Seeds;
        public ItemMaterialAmount[] Ingredients;

        public bool HasEffectTarget(Def target) => this.EffectsNew.Any(f => f.Target == target);
        public override void OnTooltipCreated(GameObject parent, Control tooltip)
        {
            foreach (var effect in this.EffectsNew)
                tooltip.Controls.Add(
                    new Label(effect) { Location = tooltip.Controls.BottomLeft, TextColorFunc = () => Color.ForestGreen }
                    );
        }
        internal override void CopyFrom(EntityComp source)
        {
            var comp = source as ConsumableComponent;
            foreach (var f in comp.EffectsNew)
                this.EffectsNew.Add(new EntityEffectWrapper(f.Def,  f.Target, f.Budget, f.Rate));
        }

        internal void Consume(GameObject actor)
        {
        }

        public override void GetInventoryTooltip(GameObject parent, Control tooltip)
        {
            this.OnTooltipCreated(parent, tooltip);
        }

        public override void GetInteractions(GameObject parent, List<Interaction> actions)
        {
            actions.Add(new InteractionConsume());
        }

        public override void Write(IDataWriter w)
        {
            w.Write(this.EffectsNew);
        }
        public override void Read(IDataReader r)
        {
            this.EffectsNew = r.ReadList<EntityEffectWrapper>();
        }
        internal override void SaveExtra(SaveTag tag)
        {
            tag.Save("Effects", this.EffectsNew);
        }
        internal override void LoadExtra(SaveTag tag)
        {
            this.EffectsNew = tag.LoadList<EntityEffectWrapper>("Effects");
        }
        public class InteractionConsume : Interaction
        {
            public InteractionConsume()
                : base("Consume", 4)
            {
                this.Verb = "Eating";
            }

            static readonly Dictionary<Need.Types, float> needs = new() { { Need.Types.Hunger, 50 } };
            
            public override Dictionary<Need.Types, float> NeedSatisfaction
            {
                get
                {
                    return needs;
                }
            }

            public override void Perform()
            {
                var actor = this.Actor;
                var target = this.Target;
                var consumable = target.Object as Entity;

                var comp = consumable.GetComponent<ConsumableComponent>();
                comp.Consume(actor);

                throw new NotImplementedException();
                Entity seeds = null;
                if (seeds != null)
                    actor.Net.PopLoot(seeds, actor.Global, actor.Velocity);

                consumable.SetStackSize(target.Object.StackSize - 1);
                actor.AddMoodlet(MoodLetDefOf.JustAte.Create());
            }
        }
        
        public new class Spec : Spec<ConsumableComponent>
        {
            public FoodClass[] FoodClasses = [];
            Func<Entity, Entity> Byproduct;
            public Spec()
            {

            }
            protected override void ApplyDefaultsTo(ConsumableComponent comp)
            {
            }
        }
    }
}
