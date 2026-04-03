using Project1.Core.Blocks;
using Project1.Core.Blocks.Comps;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Resources;
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
sealed class QuestTracker : ISaveableNewNew<QuestTracker>, ISerializableNewNew<QuestTracker>
{
    internal QuestId QuestId;
    internal EntityRefId ActorId;
    //internal int Progress, Count;
    internal int CountRemaining;
    internal IntVec3 SourceBoard;
    internal bool IsComplete => CountRemaining <= 0;
    internal (EntityRefId actorid, QuestId qid) Key => (this.ActorId, this.QuestId);

    internal QuestTracker(QuestId questId, EntityRefId actorId, int count, IntVec3 sourceBoard)
    {
        this.QuestId = questId;
        this.ActorId = actorId;
        this.CountRemaining = count;
        this.SourceBoard = sourceBoard;
    }

    internal void Increment(int delta) => this.CountRemaining -= delta;

    public SaveTag Save(string name = "")
    {
        var tag = new SaveTag(SaveTag.Types.Compound, name);
        tag.Save("QuestId", this.QuestId);
        tag.Save("ActorId", this.ActorId);
        tag.Save("CountRemaining", this.CountRemaining);
        tag.Save("SourceBoard", this.SourceBoard);
        return tag;
    }

    public static QuestTracker Create(SaveTag tag)
    {
        var questid = (QuestId)tag.LoadInt("QuestId");
        var actorid = (EntityRefId)tag.LoadInt("ActorId");
        var countRemaining = tag.LoadInt("CountRemaining");
        var sourceBoard = tag.LoadIntVec3("SourceBoard");
        return new(questid, actorid, countRemaining, sourceBoard);
    }

    public IDataWriter Write(IDataWriter w)
    {
        w.Write(this.QuestId);
        w.Write(this.ActorId);
        w.Write(this.CountRemaining);
        w.Write(this.SourceBoard);
        return w;
    }

    public static QuestTracker Create(IDataReader r)
    {
        var questid = (QuestId)r.ReadInt32();
        var actorid = r.ReadEntityRefId();
        var countRemaining = r.ReadInt32();
        var sourceBoard = r.ReadIntVec3();
        return new(questid, actorid, countRemaining, sourceBoard);
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
    readonly Dictionary<IntVec3, BlockQuestsComp> _questBoards = [];
    public IEnumerable<IntVec3> QuestBoards => this._questBoards.Keys;
    //readonly Dictionary<(EntityRefId actorId, QuestId qId), int> Progress = [];
    readonly Dictionary<(EntityRefId actorId, QuestId qId), QuestTracker> Trackers = [];

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
        => this.AcceptedQuestsByQuest[qid].Contains(actor.RefId);
    internal override IEnumerable<(Func<string>, Action)> OnQuickMenuCreated()
    {
        yield return (() => "QuestsNew", () => new QuestsGuiNew(this).ToWindow("Quests").Show());
    }
    
    internal void TryAcceptAllQuests(IntVec3 board, Actor actor)
    {
        var actorid = actor.RefId;
        List<QuestRuntime> accepted = [];
        var boardEntity = this.Map.GetBlockEntity(board);
        var boardResourceComp = boardEntity.GetComp<BlockResourcesComp>();
        var cash = boardResourceComp.GetValue(ResourceDefOf.Cash);
        var availableBudget = cash;
        var budgetUsed = 0;
        foreach (var q in this.AllQuests)
        {
            var reward = q.Reward;
            if (reward > availableBudget)
                continue;
            availableBudget -= reward;
            budgetUsed += reward;
            accepted.Add(q);
        }
        if (accepted.Count == 0)
            return;
        boardResourceComp.ApplyDelta(ResourceDefOf.Cash, -budgetUsed);
        AssignQuests(board, accepted, actorid);
        //if (!this.AcceptedQuestsByActor.TryGetValue(actorid, out var list))
        //    this.AcceptedQuestsByActor[actorid] = list = [];
        //foreach(var q in accepted)
        //{
        //    list.Add(q.Id);
        //    this.AcceptedQuestsByQuest[q.Id].Add(actorid);
        //    //this.Progress.Add((actor.RefId, q.Id), 0);
        //    this.Trackers.Add((actorid, q.Id), new(q.Id, actorid, q.Count, board));
        //}
        //boardResourceComp.ApplyDelta(ResourceDefOf.Cash, -budgetUsed);
        //boardEntity.GetCompOrDefault<BlockQuestsComp>().ReservedBudget += budgetUsed;
        //this.Map.Events.Post(new ActorAcceptedQuestsEvent(board, actorid, list.ToArray()));
        this.Notifier.Notify();
    }
    internal void TryAcceptAllQuestsInt(IntVec3 board, Actor actor, IEnumerable<QuestId> questIds)
    {
        var questList = questIds.Select(this.GetQuest);
        var actorid = actor.RefId;
        AssignQuests(board, questList, actorid);
    }

    private void AssignQuests(IntVec3 board, IEnumerable<QuestRuntime> questList, int actorid)
    {
        if (!this.AcceptedQuestsByActor.TryGetValue(actorid, out var list))
            this.AcceptedQuestsByActor[actorid] = list = [];
        var budgetUsed = 0;
        foreach (var q in questList)
        {
            list.Add(q.Id);
            this.AcceptedQuestsByQuest[q.Id].Add(actorid);
            this.Trackers.Add((actorid, q.Id), new(q.Id, actorid, q.Count, board));
            budgetUsed += q.Reward;
        }
        var boardEntity = this.Map.GetBlockEntity(board);
        //var boardResourceComp = boardEntity.GetComp<BlockResourcesComp>();
        //boardResourceComp.ApplyDelta(ResourceDefOf.Cash, -budgetUsed);
        boardEntity.GetCompOrDefault<BlockQuestsComp>().ReservedBudget += budgetUsed;
        this.Map.Events.Post(new ActorAcceptedQuestsEvent(board, actorid, [.. list]));
        this.Notifier.Notify();
    }

    //internal bool IsComplete(Actor actor, QuestRuntime quest)
    //    => this.Progress[(actor.RefId, quest.Id)] >= this.Required(quest.Id);
    //internal bool IsComplete(Actor actor, QuestId questId)
    //    => this.Progress[(actor.RefId, questId)] >= this.Required(questId);
    internal bool IsComplete(Actor actor, QuestRuntime quest)
       => this.Trackers[(actor.RefId, quest.Id)].IsComplete;
    internal bool IsComplete(Actor actor, QuestId questId)
        => this.Trackers[(actor.RefId, questId)].IsComplete;
    int Required(QuestId id)
        => this.AllQuestsInt[id].Count;
   
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
        var qid = q.Id;
        foreach(var actorid in this.AcceptedQuestsByQuest[qid])
        {
            var tracker = this.Trackers[(actorid, qid)];
            this.Refund(tracker);
            this.UnassignQuest(actorid, qid);
        }
        this.AcceptedQuestsByQuest.Remove(qid);
        this.AllQuestsInt.Remove(id);
        this.Removed?.Invoke(q);
    }
    void Refund(QuestTracker tracker)
    {
        var board = this._questBoards[tracker.SourceBoard].Parent;
        var q = this.AllQuestsInt[tracker.QuestId];
        board.GetComp<BlockResourcesComp>().ApplyDelta(ResourceDefOf.Cash, q.Reward);
        board.GetComp<BlockQuestsComp>().ReservedBudget -= q.Reward;
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
        => this.Trackers[(actor.RefId, q.Id)].Increment(delta);
    //{
    //    this.Progress[(actor.RefId, q.Id)] += delta;
    //}

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
        if (this.GetCompletedQuests(actor).FirstOrDefault() is not QuestRuntime quest)
            throw new InvalidOperationException();
        var reward = quest.Reward;
        var coins = ItemDefOf.Coins.Create(null, reward);
        this.UnassignQuest(actor, quest);
        return coins;
    }

    private void UnassignQuest(Actor actor, QuestRuntime quest)
        => this.UnassignQuest(actor.RefId, quest.Id);
    private void UnassignQuest(EntityRefId actorid, QuestId questid)
    {
        var list = this.AcceptedQuestsByActor[actorid];
        list.Remove(questid);
        if (list.Count == 0)
            this.AcceptedQuestsByActor.Remove(actorid);
        this.AcceptedQuestsByQuest[questid].Remove(actorid);
        this.Trackers.Remove((actorid, questid));
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
        tag.Save("Trackers", this.Trackers.Values);
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
        w.WriteNew(this.Trackers.Values);
    }
    public override void Read(IDataReader r)
    {
        var quests = r.ReadList<QuestRuntime>();
        foreach (var q in quests)
            this.AddQuestInt(q);
        var trackers = r.ReadListNewNew<QuestTracker>();
        foreach (var t in trackers)
            this.Trackers.Add(t.Key, t);
    }
}
