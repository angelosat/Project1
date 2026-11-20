using Project1.Core.Entities.Stats;
using Project1.Core.Gear;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework.Serialization;

namespace Project1.Core.World.Combat;

record struct AttackResult(SimulationTick Tick, EntityRefId ActorId, AttackResultTypes Type, DamageInfo Damage, GearSlotDef BodyPart, float DamagePrevented) : ISerializableNewNew<AttackResult>
{
    public bool Connected => this.Damage.Value > 0;

    public static AttackResult Create(IDataReader r)
    {
        var tick = (SimulationTick)r.ReadUInt64();
        var actorid = r.ReadEntityRefId();
        var type = (AttackResultTypes)r.ReadInt32();
        var damageval = r.ReadInt32();
        var damagedef = r.ReadDef<DamageDef>();
        var damageinfo = new DamageInfo(damageval, damagedef);
        var bodypart = r.ReadDef<GearSlotDef>();
        var damagePrevented = r.ReadSingle();
        return new(tick, actorid, type, damageinfo, bodypart, damagePrevented);
    }

    public IDataWriter Write(IDataWriter w)
    {
        w.Write(this.Tick);
        w.Write(this.ActorId);
        w.Write((int)this.Type);
        w.Write(this.Damage.Value);
        w.Write(this.Damage.Type);
        w.Write(this.BodyPart);
        w.Write(this.DamagePrevented);
        return w;
    }
}
