using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    enum ConsumableType { Food, Drink }
    class Verbs
    {
        static public string Consume { get { return "Consume"; } }
        static public string Eat { get { return "Eat"; } }
        static public string Drink { get { return "Drink"; } }
    }

    public class ConsumableComponent : EntityComponent
    {
        public override string Name { get; } = "Consumable";

        public LootTable Byproducts;
        public List<ConsumableEffect> Effects = new List<ConsumableEffect>();
        public GameObject Seeds;
        public ItemMaterialAmount[] Ingredients;

        public ConsumableComponent InitIngredients(params ItemMaterialAmount[] ingredients)
        {
            this.Ingredients = ingredients;
            return this;
        }

        public ConsumableComponent()
        {

        }
        public ConsumableComponent(ConsumableComponent toCopy)
        {
            this.Effects = toCopy.Effects;
        }
        
        public override void OnTooltipCreated(GameObject parent, UI.Control tooltip)
        {
            foreach (var effect in this.Effects)
                tooltip.Controls.Add(
                    new Label(effect.ToString()) { Location = tooltip.Controls.BottomLeft, TextColorFunc = () => Color.ForestGreen }
                    );
        }

        public override object Clone()
        {
            return new ConsumableComponent(this);
        }

        internal void Consume(GameObject actor)
        {
            foreach (var effect in this.Effects)
                effect.Apply(actor);

            if (this.Byproducts == null)
                return;
            actor.Net.PopLoot(this.Byproducts, actor.Global, actor.Velocity);
        }

        public override void GetInventoryTooltip(GameObject parent, Control tooltip)
        {
            this.OnTooltipCreated(parent, tooltip);
            var label = new Label("Use: " + new Interactions.InteractionConsume(this).Name) { Font = UIManager.FontBold, TextColorFunc = () => Color.Lime, Location = tooltip.Controls.BottomLeft };
            tooltip.Controls.Add(label);
        }

        public override void GetInteractions(GameObject parent, List<Interaction> actions)
        {
            actions.Add(new InteractionConsume());
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

                var seeds = consumable.Def.ConsumableProperties.Byproduct?.Invoke(consumable);
                if (seeds != null)
                    actor.Net.PopLoot(seeds, actor.Global, actor.Velocity);

                consumable.SetStackSize(target.Object.StackSize - 1);
                actor.AddMoodlet(MoodletDef.JustAte.Create());
            }
            public override object Clone()
            {
                return new InteractionConsume();
            }
        }

        public class Props : ComponentProps
        {
            public NeedEffect[] Effects;
            public override Type CompClass => typeof(ConsumableComponent);
            public Props()
            {
                this.Effects = new NeedEffect[] { };
            }
            public Props(NeedEffect[] needEffects)
            {
                this.Effects = needEffects;
            }
        }

        internal override void Initialize(ComponentProps componentProps)
        {
            var props = componentProps as Props;
            this.Effects = new List<ConsumableEffect>(props.Effects);
        }
    }
}
