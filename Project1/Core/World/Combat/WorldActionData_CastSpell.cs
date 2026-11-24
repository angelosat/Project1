using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Systems.Magic;
using Project1.Core.World.Actions;
using Project1.Framework.Serialization;

namespace Project1.Core.World.Combat;

internal sealed record WorldActionData_CastSpell(Actor Actor) : WorldActionData(Actor)
{
    internal required EntityRefId TargetId;

    internal required SpellDef SpellDef;

    public override ISerializable Read(IDataReader r)
    {
        this.TargetId = r.ReadEntityRefId();
        this.SpellDef = r.ReadDef<SpellDef>();
        return this;
    }

    public override void Write(IDataWriter w)
    => w.Write(this.TargetId)
        .Write(this.SpellDef);
}
