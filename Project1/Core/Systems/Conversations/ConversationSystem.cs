using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Needs;
using Project1.Core.Skills;
using Project1.Core.Towns;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Project1.Core.Systems.Conversations;

sealed class ConversationRuntime(EntityRefId initiator, EntityRefId target)
{
    enum States { Requested, Accepted, Running, Finished }
    internal EntityRefId Initiator = initiator, Target = target;
    States State;
    internal EntityRefId CurrentTalker { get; private set; } = initiator;
    internal EntityRefId CurrentReceiver => this.CurrentTalker == this.Initiator ? this.Target : this.Initiator;

    internal bool IsRequested => this.State == States.Requested;
    internal bool IsFinished => this.State == States.Finished;

    internal void MarkAccepted()
    {
        Debug.Assert(this.State == States.Requested);
        this.State = States.Running;
    }
   
    internal void MarkFinished()
    {
        Debug.Assert(this.State == States.Running);
        this.State = States.Finished;
    }
    internal void CycleTalker()
        =>   this.CurrentTalker = this.CurrentTalker == this.Initiator ? this.Target : this.Initiator;
}
public class ConversationSystem : TownComponent
{
    readonly Dictionary<EntityRefId, ConversationRuntime> ActiveConversationsByInitiator = [];
    readonly Dictionary<EntityRefId, ConversationRuntime> ActiveConversationsByTarget = [];
    readonly Dictionary<EntityRefId, ConversationRuntime> ActiveConversationsByActor = [];
    readonly HashSet<EntityRefId> _availableActors = [];

    public override string Name => "Conversations";

    internal IEnumerable<Actor> GetAvailableActors() => this.Map.World.GetEntities<Actor>(this._availableActors);

    internal bool TryGetConversation(Actor actor, out ConversationRuntime convo)
        => this.ActiveConversationsByActor.TryGetValue(actor.RefId, out convo);
    internal bool TryGetConversationByInitiator(Actor actor, out ConversationRuntime convo)
    => this.ActiveConversationsByInitiator.TryGetValue(actor.RefId, out convo);
    internal bool TryGetConversationByTarget(Actor actor, out ConversationRuntime convo)
        => this.ActiveConversationsByTarget.TryGetValue(actor.RefId, out convo);
    internal ConversationRuntime GetConverationByTarget(Actor actor)
        => this.ActiveConversationsByTarget[actor.RefId];
    internal ConversationRuntime GetConverationByActor(Actor actor)
       => this.ActiveConversationsByActor[actor.RefId];
    public ConversationSystem(Town town) : base(town)
    {
        var map = town.Map;
        map.Events.ListenTo<EntitySpawnedEvent>(OnEntitySpawn);
        map.Events.ListenTo<EntityDespawnedEvent>(OnEntityDespawn);
    }

    private void OnEntityDespawn(EntityDespawnedEvent e)
    {
        if (e.Entity is Actor actor)
            this._availableActors.Add(actor.RefId);
    }

    private void OnEntitySpawn(EntitySpawnedEvent e)
    {
        this._availableActors.Remove(e.Entity.RefId);
    }

    public override void Tick()
    {
        foreach(var convo in this.ActiveConversationsByInitiator.Values.ToArray())
        {
            if (!convo.IsFinished)
                continue;
            this.ActiveConversationsByInitiator.Remove(convo.Initiator);
            this.ActiveConversationsByTarget.Remove(convo.Target);
            this.ActiveConversationsByActor.Remove(convo.Initiator);
            this.ActiveConversationsByActor.Remove(convo.Target);
            this._availableActors.Add(convo.Initiator);
            this._availableActors.Add(convo.Target);
        }
    }

    internal bool TryStartConversation(Actor initiator, Actor target)
    {
        var conversation = new ConversationRuntime(initiator.RefId, target.RefId);
        this.ActiveConversationsByInitiator.Add(initiator.RefId, conversation);
        this.ActiveConversationsByTarget.Add(target.RefId, conversation);
        this.ActiveConversationsByActor.Add(initiator.RefId, conversation);
        this.ActiveConversationsByActor.Add(target.RefId, conversation);
        this._availableActors.Remove(initiator.RefId);
        this._availableActors.Remove(target.RefId);
        return true;
    }
    internal override void ResolveReferences()
    {
        foreach (var actor in this.Map.Entities.OfType<Actor>())
            this._availableActors.Add(actor.RefId);
    }

    internal void Advance(Actor actor)
    {
       
        var convo = this.ActiveConversationsByActor[actor.RefId];
        if (convo.CurrentTalker != actor.RefId)
            throw new Exception();
        var talker = this.World.Get<Actor>(convo.CurrentTalker);
        var receiver = this.World.Get<Actor>(convo.CurrentReceiver);
        var talkerSkill = talker.Skills.GetLevel(SkillDefOf.Social);
        receiver.Needs.ApplyAccumulatorDelta(NeedDefOf.Social, talkerSkill);
        convo.CycleTalker();
    }
}
