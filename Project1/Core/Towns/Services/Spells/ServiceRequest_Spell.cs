using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Magic;

namespace Project1.Core.Towns.Services.Spells;

public sealed class ServiceRequest_Spell : ServiceRequest
{
    internal SpellDef Spell;

    public ServiceRequest_Spell(Actor actor, SpellDef spell, int price) : base(actor, price)
    {
        Spell = spell;
    }

    public ServiceRequest_Spell()
    {
    }

    internal bool IsTargetReady { get; private set; }
    internal bool IsCasterReady { get; private set; }

    internal override TownServiceDef Service => TownServiceDefOf.Healing;

    public ulong PaymentId { get; internal set; }

    internal void MarkTargetReady()
        => this.IsTargetReady = true;

    internal void MarkCasterReady()
        => this.IsCasterReady = true;   
}
