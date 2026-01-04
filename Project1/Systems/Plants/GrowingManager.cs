using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class GrowingManager : TownComponent
    {
        readonly List<GrowingZone> GrowZones = [];
        public ILookup<PlantSpeciesDef, GrowingZone> BySpecies => this.GrowZones.ToLookup(z => z.Plant);
        public IReadOnlyList<GrowingZone> AllGrowingZones => this.GrowZones;
        public GrowingManager(Town town) : base(town)
        {
            town.Map.Events.ListenTo<ZoneCreatedEvent>(OnZoneCreated);
            town.Map.Events.ListenTo<ZoneDeletedEvent>(OnZoneDeleted);
        }
        internal override void ResolveReferences()
        {
            var growzones = this.Town.ZoneManager.GetZones<GrowingZone>();
            this.GrowZones.AddRange(growzones);
        }
        private void OnZoneCreated(ZoneCreatedEvent e)
        {
            if (e.Zone is not GrowingZone growzone)
                return;
            this.GrowZones.Add(growzone);
        }
        private void OnZoneDeleted(ZoneDeletedEvent e)
        {
            if (e.Zone is not GrowingZone growzone)
                return;
            this.GrowZones.Remove(growzone);
        }
        public IEnumerable<IntVec3> GetNextTillingPos()
        {
            return this.AllGrowingZones
                .SelectMany(z => z.GetTillingPositions());
        }
        public IEnumerable<IntVec3> GetSowingTargets(Entity preferredEntity)// = null)
        {
            if (preferredEntity.Profile is not PlantSpeciesDef seed)
                throw new ArgumentException($"{nameof(preferredEntity)} not a seed");
            return this.GetSowingTargets(seed);
        }
        //public IEnumerable<(GrowingZone zone, IEnumerable<IntVec3> targets)> GetSowingTargetsAll(Entity preferredEntity)// = null)
        //{
        //    if (preferredEntity.Profile is not PlantSpeciesDef seed)
        //        throw new ArgumentException($"{nameof(preferredEntity)} not a seed");
        //    return this.GetSowingTargetsAll(seed);
        //}
        public IEnumerable<SowingBatch> GetSowingTargetsAll(Entity preferredEntity)
        {
            if (preferredEntity.Profile is not PlantSpeciesDef seed)
                throw new ArgumentException($"{nameof(preferredEntity)} not a seed");
            return this.GetSowingTargetsAll(seed);
        }
        public IEnumerable<SowingBatch> GetSowingTargetsAll(PlantSpeciesDef species)
        {
            var validZones = this.AllGrowingZones.Where(z => z.Plant == species);
                                                                    
            foreach (var zone in validZones)
            {
                // return all positions and let the caller use the first or all
                var targets = zone.GetSowingPositions().ToList();
                if (targets.Count == 0)
                    continue;
                yield return new(targets, zone);
            }
        }
        public IEnumerable<IntVec3> GetSowingTargets(PlantSpeciesDef species)// = null)
        {
            var validZones = //preferredEntity is Entity seed ?
                this.AllGrowingZones.Where(z => z.Plant == species); //:
                                                                     //this.AllGrowingZones;
            foreach (var zone in validZones)
            {
                // return first sowing position of each zone
                var nextPos = zone.GetNextSowingPosition();
                if (nextPos.HasValue)
                    yield return nextPos.Value;
            }
            //yield break;
        }

        internal bool IsValidTillingTarget(IntVec3 global)
        {
            var growzone = this.Town.ZoneManager.GetZoneAt<GrowingZone>(global);
            if (growzone is null)
                return false;
            return growzone.IsValidTilling(global);
        }

        public override string Name => "GrowingManager";
    }

    public record SowingBatch(IReadOnlyList<IntVec3> Positions, Zone Zone) { }
}
