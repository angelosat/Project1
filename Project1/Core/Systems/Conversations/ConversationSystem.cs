using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Needs;
using Project1.Core.Skills;
using Project1.Core.Towns;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Conversations;
public sealed class ConversationSystem : TownComp
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
    internal ConversationRuntime GetConversationByTarget(Actor actor)
        => this.ActiveConversationsByTarget[actor.RefId];
    internal ConversationRuntime GetConversationByActor(Actor actor)
        //=> this.ActiveConversationsByActor[actor.RefId];
    {
        if (this.ActiveConversationsByActor.TryGetValue(actor.RefId, out var convo))
            return convo;
        return null;
    }
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

    internal override void Tick()
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

            Finish(convo);

            $"{this.World.Net} convo between {convo.Initiator} and {convo.Target} finished and removed".ToConsole();
        }
    }

    private void Finish(ConversationRuntime convo)
    {
        var initiator = this.Map.World.Get<Actor>(convo.Initiator);
        var target = this.Map.World.Get<Actor>(convo.Target);
        var initiatorRelDiff = initiator.Relationships.Get(target) - convo.InitiatorRelationshipAtStart;
        var targetRelDiff = target.Relationships.Get(initiator) - convo.TargetRelationshipAtStart;
        initiator.AI.State.Log.Write($"I had a {(initiatorRelDiff >= 0 ? "positive" : "negative")} conversation with {target.Name}");
        target.AI.State.Log.Write($"I had a {(targetRelDiff >= 0 ? "positive" : "negative")} conversation with {initiator.Name}");
    }

    internal bool TryStartConversation(Actor initiator, Actor target)
    {
        var conversation = new ConversationRuntime(this.World.CurrentTick, initiator.RefId, target.RefId);
        this.ActiveConversationsByInitiator.Add(initiator.RefId, conversation);
        this.ActiveConversationsByTarget.Add(target.RefId, conversation);
        this.ActiveConversationsByActor.Add(initiator.RefId, conversation);
        this.ActiveConversationsByActor.Add(target.RefId, conversation);
        this._availableActors.Remove(initiator.RefId);
        this._availableActors.Remove(target.RefId);
        return true;
    }

    internal override void Scan(Entity entity)
    {
        if (entity is not Actor actor)
            return;
        // because conversations are loaded before scanning/resolving references
        if (!this.ActiveConversationsByActor.ContainsKey(actor.RefId))
            this._availableActors.Add(actor.RefId);
    }

    internal void Advance(Actor actor)
    {
        var convo = this.ActiveConversationsByActor[actor.RefId];
        if (convo.CurrentTalker != actor.RefId)
            throw new Exception();
        var talker = this.World.Get<Actor>(convo.CurrentTalker);
        var listener = this.World.Get<Actor>(convo.CurrentListener);
     

        var intent = convo.NextIntent;
        var deltas = intent.Calculate(talker, listener);

        //var talkerSkill = talker.Skills.GetLevel(SkillDefOf.Social);
        //var delta = talkerSkill;
        //receiver.Needs.ApplyAccumulatorDelta(NeedDefOf.Social, delta + 10);
        //talker.Skills.ApplyXp(SkillDefOf.Social, delta);
        //talker.Relationships.ApplyDelta(receiver, delta);
        //receiver.Relationships.ApplyDelta(talker, delta);

        talker.Needs.ApplyAccumulatorDelta(NeedDefOf.Social, deltas.TalkerNeed);
        listener.Needs.ApplyAccumulatorDelta(NeedDefOf.Social, deltas.ListenerNeed);
        talker.Skills.ApplyXp(SkillDefOf.Social, deltas.TalkerXp);
        listener.Relationships.ApplyDelta(talker, deltas.ListenerRel);

        convo.OnRelationshipUpdate(listener.RefId, listener.Relationships.Get(talker));

        convo.SwapRoles();
    }

    internal void SetNextIntent(Actor actor, ConvoIntentRuntime intent)
        => this.ActiveConversationsByActor[actor.RefId].NextIntent = intent;

    protected override void SaveExtra(SaveTag tag)
    {
        var convos = this.ActiveConversationsByInitiator.Values.ToList();
        tag.Save("Convos", convos);
    }

    public override void Load(SaveTag tag)
    {
        if(tag.TryLoadList<ConversationRuntime>("Convos", out var convos))
        {
            foreach(var c in convos)
            {
                this.ActiveConversationsByInitiator.Add(c.Initiator, c);
                this.ActiveConversationsByTarget.Add(c.Target, c);
                this.ActiveConversationsByActor.Add(c.Initiator, c);
                this.ActiveConversationsByActor.Add(c.Target, c);
                // because conversations are loaded before scanning/resolving references
                //this._availableActors.Remove(c.Initiator);
                //this._availableActors.Remove(c.Target);
            }
        }
    }
}
