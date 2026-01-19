using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
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

        //public LootTable Byproducts;
        //public List<ConsumableEffect> Effects = [];
        public List<EntityEffectWrapper> EffectsNew = [];
        public GameObject Seeds;
        public ItemMaterialAmount[] Ingredients;

        public bool HasEffectTarget(Def target) => this.EffectsNew.Any(f => f.Target == target);

        //public ConsumableComponent InitIngredients(params ItemMaterialAmount[] ingredients)
        //{
        //    this.Ingredients = ingredients;
        //    return this;
        //}

        //public ConsumableComponent()
        //{

        //}
        //public ConsumableComponent(ConsumableComponent toCopy)
        //{
        //    this.Effects = toCopy.Effects;
        //}
        
        public override void OnTooltipCreated(GameObject parent, UI.Control tooltip)
        {
            //foreach (var effect in this.Effects)
            //    tooltip.Controls.Add(
            //        new Label(effect.ToString()) { Location = tooltip.Controls.BottomLeft, TextColorFunc = () => Color.ForestGreen }
            //        );
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
            //foreach (var effect in this.Effects)
            //    effect.Apply(actor);

            //if (this.Byproducts == null)
            //    return;
            //actor.Net.PopLoot(this.Byproducts, actor.Global, actor.Velocity);
        }

        public override void GetInventoryTooltip(GameObject parent, Control tooltip)
        {
            this.OnTooltipCreated(parent, tooltip);
            //var label = new Label("Use: " + new Interactions.InteractionConsume(this).Name) { Font = UIManager.FontBold, TextColorFunc = () => Color.Lime, Location = tooltip.Controls.BottomLeft };
            //tooltip.Controls.Add(label);
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

                //var seeds = consumable.Def.ConsumableProperties.Byproduct?.Invoke(consumable);
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
            //public NeedEffect[] Effects = [];
            public FoodClass[] FoodClasses = [];
            Func<Entity, Entity> Byproduct;
            public Spec()
            {

            }
            protected override void ApplyDefaultsTo(ConsumableComponent comp)
            {
                //comp.Effects = [.. this.Effects];
            }
        }
    }
}
