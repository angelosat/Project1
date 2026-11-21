using Project1.Core.Events;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework.Serialization;

#nullable enable

namespace Project1.Core.World.Combat;

[NetworkReplicate]
internal record struct CombatBeginEvent(StaticWorld World,
                                        //int CombatSessionId,
                                        EntityRefId Attacker,
                                        EntityRefId Defender)
    : IWorldEvent, ISerializableNewNew<CombatBeginEvent>
{
    public static CombatBeginEvent Create(IDataReader r)
    {
        return new()
        {
            //CombatSessionId = r.ReadInt32(),
            Attacker = r.ReadEntityRefId(),
            Defender = r.ReadEntityRefId()
        };
    }

    public readonly IDataWriter Write(IDataWriter w)
    {
        //w.Write(this.CombatSessionId);
        w.Write(this.Attacker);
        w.Write(this.Defender);
        return w;
    }
}
