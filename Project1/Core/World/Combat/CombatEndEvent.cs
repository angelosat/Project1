using Project1.Core.Events;
using Project1.Core.Simulation;
using Project1.Framework.Serialization;

#nullable enable

namespace Project1.Core.World.Combat;

[NetworkReplicate]
internal record struct CombatEndEvent(StaticWorld World,
                                      int CombatSessionId) 
    : IWorldEvent, ISerializableNewNew<CombatEndEvent>
{
    public static CombatEndEvent Create(IDataReader r)
        => new()
        {
            CombatSessionId = r.ReadInt32()
        };
        
    public readonly IDataWriter Write(IDataWriter w)
    {
        w.Write(this.CombatSessionId);
        return w;
    }
}
