using Project1.Core.AI.Personality;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Screens;
using Project1.Core.Serialization;
using Project1.Core.Systems.Materials;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Input;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.AI
{
    public enum ReactionType { Friendly, Hostile }
    public class PersonalityComponent : EntityComp, IGui
    {
        public override EntityCompDef CompDef => EntityCompDefOf.Personality;
        static readonly Random Randomizer = new();

        public ReactionType Reaction;
        public List<string> Hatelist;
        HashSet<MaterialDef> Favorites = new();
        public Dictionary<TraitDef, Trait> Traits = [];

        public override string Name { get; } = "Personality";

        public PersonalityComponent()
        {

        }
        internal override void CopyFrom(EntityComp source)
        {
            var traits = ((PersonalityComponent)source).Traits;
            foreach (var trait in traits.Keys)
                this.AddTrait(trait);
            this.Randomize();
        }
        public void AddTrait(TraitDef def)
        {
            this.Traits.Add(def, new Trait(def));
        }
        public Control GetCreationGui()
        {
            var box = new GroupBox();
            foreach (var t in this.Traits.Values)
                box.AddControlsBottomLeft(t.GetListControlGui());
            return box;
        }
        public Trait GetTrait(TraitDef def)
        {
            return this.Traits[def];
        }
        public IEnumerable<MaterialDef> GetFavorites()
        {
            foreach (var i in this.Favorites)
                yield return i;
        }
        /// <summary>
        /// https://softwareengineering.stackexchange.com/questions/254301/algorithm-to-generate-n-random-numbers-between-a-and-b-which-sum-up-to-x
        /// </summary>
        public PersonalityComponent Randomize()
        {
            RandomizeTraits();
            RandomizeFavorites();
            return this;
        }

        private void RandomizeFavorites()
        {
            this.Favorites = GenerateMaterialPreferences();
        }

        private void RandomizeTraits()
        {
            int budget = 0; //placeholder
            var random = Randomizer;
            var snapshot = this.Traits.Values.ToList();
            var count = snapshot.Count;
            double sum = 0;
            double[] values = new double[count];
            double min = -1, max = 1;
            for (int i = 0; i < count - 1; i++)
            {
                var rest = count - (i + 1);
                double restmin = min * rest;
                double restmax = max * rest;
                min = Math.Max(min, sum - restmax);
                max = Math.Min(max, sum - restmin);

                var v = getV(min, max);
                if (Math.Abs(v) > Trait.ValueRange)
                    throw new Exception();
                sum -= v;
                values[i] = v;
            }
            values[count - 1] = budget + sum;

            var totalSum = values.Sum();
            if (totalSum != budget)
                throw new Exception();


            for (int i = 0; i < count; i++)
            {
                var value = values[i];
                snapshot[i].Value = (int)(value * Trait.ValueRange);
                if (Math.Abs(value) > Trait.ValueRange)
                    throw new Exception();
            }

            static double getV(double minimum, double maximum)
            {
                return RandomHelper.NextNormal(minimum, maximum);
            }
        }

        internal override void Resolve()
        {
            var dna = (ActorDnaDef)this.Owner.Profile;
            foreach (var t in dna.Traits)
                this.AddTrait(t);
        }

        public override void Write(IDataWriter w)
        {
            //w.WriteValues(this.Traits);
            w.Write(this.Traits.Values);
            this.Favorites.WriteDefs(w);
        }
        public override void Read(IDataReader r)
        {
            //r.ReadDefWrappers(this.Traits);
            this.Traits = r.ReadList<Trait>().ToDictionary(t => t.Def, t => t);
            this.Favorites.Clear();
            this.Favorites.ReadDefs(r);
        }
        internal override void SaveExtra(SaveTag tag)
        {
            //tag.SaveDefWrappers("Traits", this.Traits);
            tag.Save("Traits", this.Traits.Values);
            this.Favorites.SaveDefs(tag, "Favorites");
        }
        internal override void LoadExtra(SaveTag tag)
        {
            //tag.LoadDefWrappers("Traits", this.Traits);
            Dictionary<TraitDef, Trait> temp = [];
            tag.LoadDefWrappers("Traits", temp);
            foreach(var (def, runtime) in this.Traits)
                if(temp.TryGetValue(def, out var load))
                    runtime.Value = load.Value;
            this.Favorites.Clear();
            if (!this.Favorites.TryLoadDefs(tag, "Favorites"))
                this.Favorites = GenerateMaterialPreferences();
        }
        static Control getFavoritesUI(PersonalityComponent p, int width)
        {
            var box = UIHelper.Wrap(p.Favorites.Select(m => new Button(m.LabelReadable) { TextColorFunc = () => m.Color }), width);
            return box.ToPanelLabeled("Favorite Materials");
        }
        

        static public HashSet<MaterialDef> GenerateMaterialPreferences()
        {
            var list = new HashSet<MaterialDef>();
            foreach (var type in Def.Get<MaterialTypeDef>())
            {
                if (type.SubTypes.Any())
                    list.Add(type.SubTypes.SelectRandom(Randomizer));
            }
            return list;
        }

        public void NewGui(GroupBox box)
        {
            var actor = this.Owner as Actor;
            var p = actor.Personality;
            var boxtraits = new GroupBox();
            foreach (var t in p.Traits.Values)
            {
                var bar = t.GetListControlGui() as ButtonBase;
                bar.LeftClickAction = () => {
                    if (!InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
                        return;
                    "todo: request trait change from server".ToConsole();
                    var posClicked = UIManager.MouseScaled.X - (bar.ScreenLocation.X + bar.Width / 2);
                    var val = posClicked;// * 2;
                    Ingame.Instance.Events.Post(new PlayerChangeTraitValueEvent(actor, t.Def, val));
                };
                
                boxtraits.AddControlsBottomLeft(bar);
            }
            box.AddControlsVertically(
                boxtraits.ToPanelLabeled("Traits"), 
                getFavoritesUI(p, boxtraits.Width));
        }

        internal void SetValue(TraitDef trait, float value)
        {
            this.Traits[trait].Value = value;
            this.Owner.World.Events.Post(new TraitValueChangedEvent(this.Owner as Actor, trait, value));
        }

        public float GetValue(TraitDef trait)
            => this.Traits[trait].Value;
        public float GetPercentage(TraitDef trait)
          => this.Traits[trait].Percentage;


        public new class Spec : Spec<PersonalityComponent>
        {
            public TraitDef[] Items;
            public Spec(params TraitDef[] defs)
            {
                this.Items = defs;
            }
            protected override void ApplyDefaultsTo(PersonalityComponent comp)
            {
                //foreach (var trait in this.Items)
                //    comp.AddTrait(trait);
                comp.Randomize();
            }
        }
    }
}
