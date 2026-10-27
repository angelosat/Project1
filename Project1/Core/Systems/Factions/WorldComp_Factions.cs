using Project1.Core.AI.MetaRoles;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using System;
using System.Linq;

namespace Project1.Core.Systems.Factions;

internal class WorldComp_Factions : WorldComp
{
    FactionId NextFactionId => ++field;

    FactionId ColonyId, AdventurersId, BanditsId;

    Faction Colony => this.World.FactionRegistry[this.ColonyId];

    Faction Adventurers => this.World.FactionRegistry[this.AdventurersId];

    Faction Bandits => this.World.FactionRegistry[this.BanditsId];

    public int AdventurerCount { get; private set; }

    public int BanditCount { get; private set; }

    const int BanditFactionCapacity = 3;

    public WorldComp_Factions(WorldBase world) : base(world)
    {
        world.Events.ListenTo<EntityRegisteredEvent>(OnEntityRegistered);
    }

    private void OnEntityRegistered(EntityRegisteredEvent e)
    {
        if (e.Entity is not Actor actor)
            return;
        var faction = this.GetFactionForRole(actor.AI.Meta.Def);
        AddToFaction(actor, faction.Value);
    }

    public Faction GenerateFaction(FactionDef def)
    {
        var id = NextFactionId;
        var name = $"Faction {id}";
        var faction = new Faction(def, id, name);
        this.World.FactionRegistry.Add(faction.Id, faction);
        return faction;
    }

    internal override void Scan(Entity entity)
    {
        if (entity is not Actor actor)
            return;
        if (this.World.FactionRegistry.Values.Any(f => f.IsMember(actor)))
            return;
        // TODO repair faction members here?
        var metaRole = actor.AI.Meta.Def;
        var factionId = this.GetFactionForRole(metaRole);
        //actor.FactionId = factionId;
        if (factionId.HasValue && factionId != FactionId.Null)
            //this.World.FactionRegistry[factionId.Value].AddMember(actor);
            this.AddToFaction(actor, factionId.Value);
    }

    void AddToFaction(Actor actor, FactionId factionId)
    {
        actor.FactionId = factionId;
        // TODO: remove from old faction
        var faction = this.World.FactionRegistry[factionId];
        if (faction.Def == FactionDefOf.Adventurers)
            AdventurerCount++;
        else if (faction.Def == FactionDefOf.Bandits)
            BanditCount++;
        faction.AddMember(actor);
    }

    FactionId? GetFactionForRole(RoleMetaDef role)
    {
        if (role == RoleMetaDefOf.TownMember)
            return this.ColonyId;
        if(role ==  RoleMetaDefOf.Adventurer) 
            return this.AdventurersId;
        if (role == RoleMetaDefOf.Npc)
            return this.BanditsId;
        return null;
    }

    internal override void EnsureInitialized()
    {
        if (this.ColonyId == FactionId.Null)
            this.ColonyId = this.GenerateFaction(FactionDefOf.Colony).Id;
        if (this.AdventurersId == FactionId.Null)
            this.AdventurersId = this.GenerateFaction(FactionDefOf.Adventurers).Id;
        if (this.BanditsId == FactionId.Null)
        {
            var bandits = this.GenerateFaction(FactionDefOf.Bandits);
            this.BanditsId = bandits.Id;
            SetRelation(this.Colony, bandits, -100);
            SetRelation(this.Adventurers, bandits, -100);
        }
        //this.EnsureBanditFactionCount();
    }

    private void EnsureBanditFactionCount()
    {
        var count = this.World.FactionRegistry.Values.Count(f => f.Def == FactionDefOf.Bandits);
        for (var i = count; i < BanditFactionCapacity; i++)
        {
            var bandits = this.GenerateFaction(FactionDefOf.Bandits);
            SetRelation(this.Colony, bandits, -100);
            SetRelation(this.Adventurers, bandits, -100);
        }
    }

    static void SetRelation(Faction first, Faction second, float value)
    {
        first.Relations[second.Id] = value;
        second.Relations[first.Id] = value;
    }
}
