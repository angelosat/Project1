using Project1.Core.Blocks;
using Project1.Core.Blocks.Comps;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Resources;
using Project1.Core.Systems.Inventory;
using Project1.Core.Systems.Materials;
using Project1.Core.Towns;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Quests;
sealed class QuestTracker
{
    internal QuestId QuestId;
    internal EntityRefId ActorId;
    internal int Progress;
    internal bool IsCompleted => false;
}

abstract class QuestController
{
    protected TownComp_Quests Comp;
    public void Register(TownComp_Quests comp)
    {
        this.Comp = comp;
        this.OnRegister();
    }
    protected abstract void OnRegister();
}
sealed class QuestController_Deliver : QuestController
{
    protected override void OnRegister()
    {
        this.Comp.Map.World.Events.ListenTo<InventoryItemAddedEvent>(HandleInventoryItemAdded);
        this.Comp.Map.World.Events.ListenTo<InventoryItemMergedEvent>(HandleInventoryItemMerged);
    }

    private void HandleInventoryItemMerged(InventoryItemMergedEvent e)
    {
        var actor = e.Actor;
        var item = e.Existing;
        var amount = e.MergeAmount;
        this.TryIncrementProgress(actor, item, amount);
    }

    private void HandleInventoryItemAdded(InventoryItemAddedEvent e)
    {
        var actor = e.Actor;
        var item = e.Item;
        var amount = item.StackSize;
        this.TryIncrementProgress(actor, item, amount);
    }

    private void TryIncrementProgress(Actor actor, Entity item, int amount)
    {
        var quests = this.Comp.GetAcceptedQuestsByActor<QuestRuntime_Deliver>(actor);

        foreach (var q in quests)
        {
            if (q.Matches(item))
                this.Comp.IncrementProgress(actor, q, amount);
        }
    }
}

public sealed class TownComp_Quests : TownComponent
{
    public ChangeNotifier Notifier = new();
    public override string Name => "QuestsNew";
    QuestId _nextQuestId = 1;
    QuestId GetNextQuestId() => this._nextQuestId++;
    readonly Dictionary<QuestId, QuestRuntime> AllQuestsInt = [];
    public IEnumerable<QuestRuntime> AllQuests => this.AllQuestsInt.Values;
    readonly Dictionary<(MaterialRefinementDef, MaterialDef), QuestId> FetchQuests = [];
    readonly Dictionary<QuestId, QuestRuntime_Deliver> FetchQuestsById = [];
    public Action<QuestRuntime> Added, Removed;
    readonly Dictionary<EntityRefId, HashSet<QuestId>> AcceptedQuestsByActor = [];
    readonly Dictionary<QuestId, HashSet<EntityRefId>> AcceptedQuestsByQuest = [];
    //readonly HashSet<IntVec3> _questBoards = [];
    readonly Dictionary<IntVec3, BlockQuestsComp> _questBoards = [];
    public IEnumerable<IntVec3> QuestBoards => this._questBoards.Keys;
    readonly Dictionary<(EntityRefId actorId, QuestId qId), int> Progress = [];

    //static readonly QuestController[] Controllers;
    //static TownComp_Quests()
    //{
    //    Controllers = [.. AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
    //        .Where(t => typeof(QuestController).IsAssignableFrom(t) && !t.IsAbstract)
    //        .Select(t => (QuestController)Activator.CreateInstance(t))];
    //}
    public TownComp_Quests(Town town) : base(town)
    {
        town.Map.Events.ListenTo<BlockEntityAddedEvent>(HandleBlockEntityAdded);
        town.Map.Events.ListenTo<BlockEntityRemovedEvent>(HandleBlockEntityRemoved);

        QuestController[] controllers = [.. AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
            .Where(t => typeof(QuestController).IsAssignableFrom(t) && !t.IsAbstract)
            .Select(t => (QuestController)Activator.CreateInstance(t))];

        foreach (var c in controllers)
            c.Register(this);
    }
    
    private void HandleBlockEntityRemoved(BlockEntityRemovedEvent e)
    {
        this._questBoards.Remove(e.Entity.OriginGlobal);
    }

    private void HandleBlockEntityAdded(BlockEntityAddedEvent e)
    {
        if (e.Entity.TryGetComp<BlockQuestsComp>(out var comp))
            this._questBoards.Add(e.Entity.OriginGlobal, comp);
    }
    public QuestRuntime GetQuest(QuestId id)
        => this.AllQuestsInt[id];
    public IEnumerable<QuestRuntime> GetAcceptedQuestsByActor(Actor actor)
    {
        if (!this.AcceptedQuestsByActor.TryGetValue(actor.RefId, out var list))
            return [];
        return list.Select(qid => this.AllQuestsInt[qid]);
    }
    public IEnumerable<T> GetAcceptedQuestsByActor<T>(Actor actor) where T : QuestRuntime
    {
        if (!this.AcceptedQuestsByActor.TryGetValue(actor.RefId, out var list))
            return [];
        return list.Select(qid => this.AllQuestsInt[qid]).OfType<T>();
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
    //internal void AcceptQuest(Actor actor, QuestRuntime quest)
    //{
    //    var actorid = actor.RefId;
    //    //this.AcceptedQuests.Add(actor.RefId, )
    //    if (!this.AcceptedQuestsByActor.TryGetValue(actorid, out var list))
    //        this.AcceptedQuestsByActor[actorid] = list = [];
    //    list.Add(quest.Id);
    //    this.AcceptedQuestsByQuest[quest.Id].Add(actorid);
    //    this.Notifier.Notify();

    //}
    internal void TryAcceptAllQuests(IntVec3 board, Actor actor)
    {
        var actorid = actor.RefId;
        //if (!this.AcceptedQuestsByActor.TryGetValue(actorid, out var list))
        //    this.AcceptedQuestsByActor[actorid] = list = [];
        List<QuestRuntime> accepted = [];
        var cash = this.Map.GetBlockEntityComp<BlockResourcesComp>(board).GetValue(ResourceDefOf.Cash);

        foreach (var q in this.AllQuests)
        {
            //if (!list.Contains(q.Id))
            var reward = q.Reward;
            if (reward > cash)
                continue;
            //list.Add(q.Id);
            //this.AcceptedQuestsByQuest[q.Id].Add(actorid);
            accepted.Add(q);
        }
        if (accepted.Count == 0)
            return;
        if (!this.AcceptedQuestsByActor.TryGetValue(actorid, out var list))
            this.AcceptedQuestsByActor[actorid] = list = [];
        foreach(var q in accepted)
        {
            list.Add(q.Id);
            this.AcceptedQuestsByQuest[q.Id].Add(actorid);
            this.Progress.Add((actor.RefId, q.Id), 0);
        }
        this.Map.Events.Post(new ActorAcceptedQuestsEvent(board, actorid));
        this.Notifier.Notify();
    }
    internal bool IsComplete(Actor actor, QuestRuntime quest)
        => this.Progress[(actor.RefId, quest.Id)] >= this.Required(quest.Id);
    internal bool IsComplete(Actor actor, QuestId questId)
        => this.Progress[(actor.RefId, questId)] >= this.Required(questId);
    int Required(QuestId id)
        => this.AllQuestsInt[id].Count;
    //internal void AcceptAllQuests(IntVec3 board, Actor actor)
    //{
    //    var actorid = actor.RefId;
    //    //this.AcceptedQuests.Add(actor.RefId, )
    //    if (!this.AcceptedQuestsByActor.TryGetValue(actorid, out var list))
    //        this.AcceptedQuestsByActor[actorid] = list = [];
    //    List<QuestId> accepted = [];
    //    foreach (var q in this.AllQuests)
    //    {
    //        //if (!list.Contains(q.Id))
    //        list.Add(q.Id);
    //        this.AcceptedQuestsByQuest[q.Id].Add(actorid);
    //    }
    //    this.Map.Events.Post(new ActorAcceptedQuestsEvent(actorid));
    //    this.Notifier.Notify();
    //}
    internal bool IsQuestAvailable(IntVec3 board, QuestId id)
    {
        var cash = this.Map.GetBlockEntityComp<BlockResourcesComp>(board).GetValue(ResourceDefOf.Cash);
        var reward = this.AllQuestsInt[id].Reward;
        if (cash < reward)
            return false;
        return true;
    }
    internal IEnumerable<QuestRuntime> GetAvailableQuests(IntVec3 board)
    {
        var cash = this.Map.GetBlockEntityComp<BlockResourcesComp>(board).GetValue(ResourceDefOf.Cash);
        return this.AllQuests.Where(q => q.Reward <= cash);
        //foreach(var q in this.AllQuests)
        //{
        //    if (q.Reward > cash)
        //        continue;
        //    yield return q.Id;
        //}
    }
    internal bool TryCreateQuest(MaterialRefinementDef refdef, MaterialDef matdef)
    {
        var key = (refdef, matdef);
        if (this.FetchQuests.TryGetValue(key, out _))
            return false;
        var reward = ItemDefOf.Ingredient.BaseValue * matdef.Value;
        var quest = new QuestRuntime_Deliver(this.GetNextQuestId(), reward, refdef, matdef);
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
            case QuestRuntime_Deliver:
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
            if (be.TryGetComp<BlockQuestsComp>(out var comp))
                this._questBoards.Add(be.OriginGlobal, comp);
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
            case QuestRuntime_Deliver fq:
                this.FetchQuests[fq.Key] = fq.Id;
                this.FetchQuestsById[fq.Id] = fq;
                break;
        }
        this.AllQuestsInt[q.Id] = q;
        this.AcceptedQuestsByQuest[q.Id] = [];
    }

    internal void IncrementProgress(Actor actor, QuestRuntime q, int delta)
    {
        this.Progress[(actor.RefId, q.Id)] += delta;
    }

    internal bool HasCompletedQuests(Actor actor)
        => this.AcceptedQuestsByActor[actor.RefId].Any(q => this.IsComplete(actor, q));

    IEnumerable<QuestRuntime> GetCompletedQuests(Actor actor)
        => this.AcceptedQuestsByActor[actor.RefId].Where(q => this.IsComplete(actor, q)).Select(q => this.AllQuestsInt[q]);

    internal QuestRuntime GetNextCompletedQuest(Actor actor)
    {
        if (!this.AcceptedQuestsByActor.TryGetValue(actor.RefId, out var list))
            return null;
        return list.Where(q => this.IsComplete(actor, q)).Select(q => this.AllQuestsInt[q]).FirstOrDefault();
    }
    internal Entity CompleteNextQuest(Actor actor, IntVec3 board)
    {
        var boardEntity = this._questBoards[board].Parent;
        if (this.GetCompletedQuests(actor).FirstOrDefault() is not QuestRuntime quest)
            throw new InvalidOperationException();
        var resComp = boardEntity.GetComp<BlockResourcesComp>();
        var reward = quest.Reward;
        resComp.ApplyDelta(ResourceDefOf.Cash, -reward);
        var coins = ItemDefOf.Coins.Create(null, reward);
        this.UnassignQuest(actor, quest);
        return coins;
    }

    private void UnassignQuest(Actor actor, QuestRuntime quest)
    {
        var actorid = actor.RefId;
        var questid = quest.Id;
        var list = this.AcceptedQuestsByActor[actorid];
        list.Remove(questid);
        if (list.Count == 0)
            this.AcceptedQuestsByActor.Remove(actorid);
        this.AcceptedQuestsByQuest[questid].Remove(actorid);
        this.Progress.Remove((actorid, questid));
    }
}
