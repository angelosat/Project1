using Project1.Core.Entities.Actors;
using Project1.Framework;
using Project1.Framework.Serialization;
using System.Collections.Generic;
using Project1.Core.Helpers;
using System;

namespace Project1.Core.Systems.Factions;

public sealed class Faction(FactionDef def, FactionId id, string name) : ISerializableNewNew<Faction>
{
    public FactionDef Def = def;
    public FactionId Id { get; private set; } = id;

    internal readonly Dictionary<Faction, float> Relations = [];

    readonly HashSet<EntityRefId> Members = [];

    public string Name { get; private set; } = name;

    public void AddMember(Actor actor)
        => this.Members.Add(actor.RefId);

    public void RemoveMember(Actor actor)
        => this.Members.Remove(actor.RefId);

    public IDataWriter Write(IDataWriter w)
    {
        w.Write(this.Id);
        w.Write(this.Def);
        w.Write(this.Name);
        w.Write(this.Members);
        return w;
    }

    public static Faction Create(IDataReader r)
    {
        var id = r.ReadId<FactionId>();
        var def = r.ReadDef<FactionDef>();
        var name = r.ReadString();
        var faction = new Faction(def, id, name);
        var members = r.ReadListEntityRefId();
        foreach (var memberId in members)
            faction.Members.Add(memberId);
        return faction;
    }

    internal bool IsMember(Actor actor)
        => this.Members.Contains(actor.RefId);
}
