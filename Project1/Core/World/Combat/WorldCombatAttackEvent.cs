using Project1.Core.Events;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework.Serialization;

namespace Project1.Core.World.Combat;

[NetworkReplicate]
record struct WorldCombatFleeEvent(StaticWorld World, EntityRefId ActorId)
    : IWorldEvent, ISerializableNewNew<WorldCombatFleeEvent>
{
    public static WorldCombatFleeEvent Create(IDataReader r)
    {
        return new()
        {
            ActorId = r.ReadEntityRefId()
        };
    }

    public readonly IDataWriter Write(IDataWriter w)
    {
        w.Write(this.ActorId);
        return w;
    }
}

[NetworkReplicate]
record struct WorldCombatAttackEvent(StaticWorld World,
                               int CombatId,
                               EntityRefId AttackerId,
                               AttackResult Result) 
    : IWorldEvent, ISerializableNewNew<WorldCombatAttackEvent>
{
    public static WorldCombatAttackEvent Create(IDataReader r)
    {
        return new()
        {
            CombatId = r.ReadInt32(),
            AttackerId = r.ReadEntityRefId(),
            Result = AttackResult.Create(r)
        };
    }

    public readonly IDataWriter Write(IDataWriter w)
    {
        w.Write(this.CombatId);
        w.Write(this.AttackerId);
        this.Result.Write(w);
        return w;
    }
}
