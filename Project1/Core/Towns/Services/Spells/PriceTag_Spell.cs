using Project1.Core.Systems.Magic;
using Project1.Framework.Events;

namespace Project1.Core.Towns.Services.Spells;

sealed class PriceTag_Spell(SpellDef spell, int price, bool enabled)
{
    internal ChangeNotifier Notifier = new();

    internal SpellDef Spell = spell;
    internal int Price = price;
    internal bool Enabled
    {
        get => field; private set
        {
            field = value;
            this.Notifier.Notify();
        }
    } = enabled;
    internal void Toggle()
        => this.Enabled = !this.Enabled;
}
