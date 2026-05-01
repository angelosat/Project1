using Project1.Core.Screens;
using Project1.Core.Systems.Effects;
using Project1.Core.Systems.Magic;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using System;
using System.Linq;

namespace Project1.Core.Towns.Services.Spells;

sealed class Gui_TownSpellList : GroupBox
{
    readonly Table<(SpellDef spell, PriceTag_Spell tag)> Table;
    public Gui_TownSpellList()
    {
        var shops = Ingame.Net.MainView.Map.Town.Spells;

        this.Table = new Table<(SpellDef spell, PriceTag_Spell tag)>()
                    .AddColumn("item", 256, a => new LabelNew(a.spell) { HoverText = getHoverText(a.spell) })
                    .AddColumn("price", 48, a => new LabelNew(() => a.tag.Price.ToString()))
                    .AddColumn("tick", 32, a => new CheckBoxFinalNew(() => ToggleSpell(a.spell), () => a.tag.Enabled).InvalidateOn(a.tag.Notifier));
        this.Table.AddItems(shops.GetPriceList());

        var scrollbox = ScrollableBoxNewNewNew.FromWidth(this.Table, this.Table.RowWidth, UIManager.DefaultLabelHeight * 16);
        this.Controls.Add(scrollbox.ToPanelLabeled("Price list"));

        string getHoverText(SpellDef spell)
        {
            var effects = spell.Effects;
            //var lines = effects.Select(fx => $"{fx.effect.Verb} {fx.target.LabelReadable} for {fx.effect.BaseDuration}");
            return string.Join(Environment.NewLine, effects.Select(fx => EffectsUtils.GetString(fx.effect, fx.target)));
        }
    }
    static void ToggleSpell(SpellDef spell)
        => Ingame.Instance.Events.Post(new PlayerTownSpellToggledEvent(Ingame.Net.MainView.Map, spell));
}
