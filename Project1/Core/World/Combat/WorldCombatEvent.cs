using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework.Events;
using Project1.Framework.Serialization;

namespace Project1.Core.World.Combat;

record struct WorldCombatEvent(StaticWorld World, int CombatId, EntityRefId AttackerId, AttackResult Result) : IEventPayload, ISerializableNewNew<WorldCombatEvent>
{
    public static WorldCombatEvent Create(IDataReader r)
    {
        return new()
        {
            CombatId = r.ReadInt32(),
            AttackerId = r.ReadEntityRefId(),
            Result = AttackResult.Create(r)
        };
    }

    public IDataWriter Write(IDataWriter w)
    {
        w.Write(this.CombatId);
        w.Write(this.AttackerId);
        this.Result.Write(w);
        return w;
    }
}
