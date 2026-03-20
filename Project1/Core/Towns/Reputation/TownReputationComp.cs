using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System.Collections.Generic;

namespace Project1.Core.Towns.Reputation
{
    sealed class AgentReputationEntry(Actor actor, ulong tick)
    {
        internal EntityRefId AgentId = actor.RefId;
        internal ulong TickDiscovered = tick;
    }
    public sealed class TownReputationComp : TownComponent
    {
        public override string Name => "Reputation";
        readonly Dictionary<Actor, AgentReputationEntry> _table = [];

        public TownReputationComp(Town town) : base(town)
        {
            town.Map.Events.ListenTo<EntitySpawnedEvent>(HandleEntitySpawned);
        }

        private void HandleEntitySpawned(EntitySpawnedEvent e)
        {
            if (e.Entity is not Actor agent)
                return;
            if (this._table.ContainsKey(agent))
                return;
            if (this.Town.Members.Contains(agent))
                return;
            this._table.Add(agent, new(agent, this.Town.Map.World.CurrentTick));
        }
    }
}
