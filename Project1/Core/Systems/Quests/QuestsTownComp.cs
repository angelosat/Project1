using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Interactions;
using Project1.Core.Systems.Materials;
using Project1.Core.Towns;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Quests;

sealed class InteractionAcceptQuest : InteractionLogic
{
    sealed class Context : InteractionContext { }
    protected override InteractionContext CreateContextInt() => new Context();
    internal override void OnFinish(Interaction i)
    {
        var actor = i.Actor;
        if (actor.Net.IsClient)
            return;
        var map = actor.Map;
        var manager = map.Town.QuestManagerNew;
        manager.AcceptAllQuests(actor);
    }
}
sealed class PlannerQuest : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        if (actor.IsHauling)
            return null;
        var map = actor.Map;
        var manager = map.Town.QuestManagerNew;
        var boards = manager.QuestBoards;
        bool foundBoard = false;
        IntVec3 board = default;
        foreach(var b in boards)
        {
            if (!actor.CanReachAndReserve(b))
                continue;
            foundBoard = true;
            board = b;
            break;
        }
        if (!foundBoard)
            return null;
        var allquests = manager.AllQuests;
        foreach(var q in allquests)
        {
            if (!manager.HasQuest(actor, q.Id))
                return new Plan(QuestDefOf.PlanQuest, map, board);
        }
        return null;
    }
}

public sealed class QuestsTownComp : TownComponent
{
    public ChangeNotifier Notifier = new();
    public override string Name => "QuestsNew";
    QuestId _nextQuestId = 1;
    QuestId GetNextQuestId() => this._nextQuestId++;
    readonly Dictionary<QuestId, QuestRuntime> AllQuestsInt = [];
    public IEnumerable<QuestRuntime> AllQuests => this.AllQuestsInt.Values;
    readonly Dictionary<(MaterialRefinementDef, MaterialDef), QuestId> FetchQuests = [];
    readonly Dictionary<QuestId, FetchQuestRuntime> FetchQuestsById = [];
    public Action<QuestRuntime> Added, Removed;
    readonly Dictionary<EntityRefId, HashSet<QuestId>> AcceptedQuestsByActor = [];
    readonly Dictionary<QuestId, HashSet<EntityRefId>> AcceptedQuestsByQuest = [];
    readonly HashSet<IntVec3> _questBoards = [];
    public IEnumerable<IntVec3> QuestBoards => this._questBoards;

    public QuestsTownComp(Town town) : base(town)
    {
        town.Map.Events.ListenTo<BlockEntityAddedEvent>(HandleBlockEntityAdded);
        town.Map.Events.ListenTo<BlockEntityRemovedEvent>(HandleBlockEntityRemoved);
    }

    private void HandleBlockEntityRemoved(BlockEntityRemovedEvent e)
    {
        this._questBoards.Remove(e.Entity.OriginGlobal);
    }

    private void HandleBlockEntityAdded(BlockEntityAddedEvent e)
    {
        if (e.Entity.HasComp<BlockQuestsComp>())
            this._questBoards.Add(e.Entity.OriginGlobal);
    }
    public IEnumerable<QuestRuntime> GetAcceptedQuestsByActor(Actor actor)
    {
        if (!this.AcceptedQuestsByActor.TryGetValue(actor.RefId, out var list))
            return [];
        return list.Select(qid => this.AllQuestsInt[qid]);
    }
    public IReadOnlySet<EntityRefId> GetAssignedActorsByQuest(QuestId id)
    {
        if (!this.AcceptedQuestsByQuest.TryGetValue(id, out var list))
            return new HashSet<EntityRefId>();
        return list;
    }
    public bool HasQuest(Actor actor, QuestId qid)
    {
        return this.AcceptedQuestsByQuest[qid].Contains(actor.RefId);
        //if (!this.AcceptedQuestsByActor.TryGetValue(actor.RefId, out var list))
        //    return false;
        //return list.Contains(qid);
    }
    internal override IEnumerable<(Func<string>, Action)> OnQuickMenuCreated()
    {
        yield return (() => "QuestsNew", () => new QuestsGuiNew(this).ToWindow("Quests").Show());
    }
    internal void AcceptQuest(Actor actor, QuestRuntime quest)
    {
        var actorid = actor.RefId;
        //this.AcceptedQuests.Add(actor.RefId, )
        if (!this.AcceptedQuestsByActor.TryGetValue(actorid, out var list))
            this.AcceptedQuestsByActor[actorid] = list = [];
        list.Add(quest.Id);
        this.AcceptedQuestsByQuest[quest.Id].Add(actorid);
        this.Notifier.Notify();

    }
    internal void AcceptAllQuests(Actor actor)
    {
        var actorid = actor.RefId;
        //this.AcceptedQuests.Add(actor.RefId, )
        if (!this.AcceptedQuestsByActor.TryGetValue(actorid, out var list))
            this.AcceptedQuestsByActor[actorid] = list = [];
        foreach (var q in this.AllQuests)
        {
            //if (!list.Contains(q.Id))
            list.Add(q.Id);
            this.AcceptedQuestsByQuest[q.Id].Add(actorid);
        }
        this.Map.Events.Post(new ActorAcceptedQuestsEvent(actorid));
        this.Notifier.Notify();
    }
    internal bool TryCreateQuest(MaterialRefinementDef refdef, MaterialDef matdef)
    {
        var key = (refdef, matdef);
        if (this.FetchQuests.TryGetValue(key, out _))
            return false;
        var reward = ItemDefOf.Ingredient.BaseValue * matdef.Value;
        var quest = new FetchQuestRuntime(this.GetNextQuestId(), reward, refdef, matdef);
        this.AllQuestsInt.Add(quest.Id, quest);
        this.FetchQuests[key] = quest.Id;
        this.FetchQuestsById[quest.Id] = quest;
        this.AcceptedQuestsByQuest[quest.Id] = [];
        this.Added?.Invoke(quest);
        return true;
    }
    internal void DeleteQuest(QuestId id)
    {
        var q = this.AllQuestsInt[id];
        switch (q)
        {
            case FetchQuestRuntime:
                var fq = this.FetchQuestsById[id];
                this.FetchQuests.Remove((fq.Refinement, fq.Material));
                this.FetchQuestsById.Remove(id);
                break;
        }
        this.AllQuestsInt.Remove(id);
        this.Removed?.Invoke(q);
    }
    internal override void ResolveReferences()
    {
        foreach (var be in this.Map.BlockEntities)
            if (be.HasComp<BlockQuestsComp>())
                this._questBoards.Add(be.OriginGlobal);
    }
    protected override void AddSaveData(SaveTag tag)
    {
        tag.Save("Quests", this.AllQuestsInt.Values);
    }
    public override void Load(SaveTag tag)
    {
        var quests = tag.LoadList<QuestRuntime>("Quests");
        foreach (var q in quests)
            this.AddQuestInt(q);
    }
    public override void Write(IDataWriter w)
    {
        w.Write(this.AllQuestsInt.Values);
    }
    public override void Read(IDataReader r)
    {
        var quests = r.ReadList<QuestRuntime>();
        foreach (var q in quests)
            this.AddQuestInt(q);
    }
    private void AddQuestInt(QuestRuntime q)
    {
        switch(q)
        {
            case FetchQuestRuntime fq:
                this.FetchQuests[fq.Key] = fq.Id;
                this.FetchQuestsById[fq.Id] = fq;
                break;
        }
        this.AllQuestsInt[q.Id] = q;
        this.AcceptedQuestsByQuest[q.Id] = [];
    }
}
