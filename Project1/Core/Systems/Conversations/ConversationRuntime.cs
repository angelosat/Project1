using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Serialization;
using System.Diagnostics;

namespace Project1.Core.Systems.Conversations;

sealed class ConversationRuntime(SimulationTick tick, EntityRefId initiator, EntityRefId target) 
    : ISaveableNewNew<ConversationRuntime>
{
    enum States { Requested, Accepted, Running, Finished }
    internal EntityRefId Initiator = initiator, Target = target;
    States State;
    internal EntityRefId CurrentTalker { get; private set; } = initiator;
    internal EntityRefId CurrentListener => this.CurrentTalker == this.Initiator ? this.Target : this.Initiator;
    internal SimulationTick TickRequested { get; private set; } = tick;

    internal int InitiatorRelationshipAtStart { get; private set; }
    internal int TargetRelationshipAtStart { get; private set; }

    int InitiatorMinOffset, InitiatorMaxOffset,
        TargetMinOffset, TargetMaxOffset;

    internal bool IsRequested => this.State == States.Requested;
    internal bool IsFinished => this.State == States.Finished;

    public ConvoIntentRuntime NextIntent { get; internal set; }
    //public ConversationRuntime(Actor initiator, Actor target) : this(initiator.World.CurrentTick, initiator.RefId, target.RefId)
    //{
    //    this.InitiatorRelationshipAtStart = initiator.Relationships.Get(target);
    //    this.TargetRelationshipAtStart = target.Relationships.Get(initiator);
    //}
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
    internal void SwapRoles()
        =>   this.CurrentTalker = this.CurrentTalker == this.Initiator ? this.Target : this.Initiator;

    internal void OnRelationshipUpdate(EntityRefId actorId, int current)
    {
        if(actorId == this.Initiator)
            UpdateOffsets(current - this.InitiatorRelationshipAtStart, ref this.InitiatorMinOffset, ref this.InitiatorMaxOffset);
        else
            UpdateOffsets(current - this.TargetRelationshipAtStart, ref this.TargetMinOffset, ref this.TargetMaxOffset);
    }
    static void UpdateOffsets(int offset, ref int min, ref int max)
    {
        if (offset < min) min = offset;
        if (offset > max) max = offset;
    }

    public SaveTag Save(string name = "")
    {
        var tag = new SaveTag(SaveTag.Types.Compound, name);
        tag.Save("Tick", this.TickRequested);
        tag.Save("Initiator", this.Initiator);
        tag.Save("Target", this.Target);
        tag.Save("State", (int)this.State);
        tag.Save(nameof(InitiatorRelationshipAtStart), this.InitiatorRelationshipAtStart);
        tag.Save(nameof(TargetRelationshipAtStart), this.TargetRelationshipAtStart);
        tag.Save(nameof(InitiatorMinOffset), this.InitiatorMinOffset);
        tag.Save(nameof(InitiatorMaxOffset), this.InitiatorMaxOffset);
        tag.Save(nameof(TargetMinOffset), this.TargetMinOffset);
        tag.Save(nameof(TargetMaxOffset), this.TargetMaxOffset);
        tag.Save("Intent", this.NextIntent);

        return tag;
    }

    public static ConversationRuntime Create(SaveTag tag)
    {
        var tick = (SimulationTick)tag.LoadUlong("Tick");
        var initiator = tag.LoadEntityRefId("Initiator");
        var target = tag.LoadEntityRefId("Target");
        var state = (States)tag.LoadInt("State");

        var initRel = tag.LoadInt(nameof(InitiatorRelationshipAtStart));
        var targetRel = tag.LoadInt(nameof(TargetRelationshipAtStart));
        var initMin = tag.LoadInt(nameof(InitiatorMinOffset));
        var initMax = tag.LoadInt(nameof(InitiatorMaxOffset));
        var targetMin = tag.LoadInt(nameof(TargetMinOffset));
        var targetMax = tag.LoadInt(nameof(TargetMaxOffset));

        var intent = tag.Load<ConvoIntentRuntime>("Intent");

        var runtime = new ConversationRuntime(tick, initiator, target);

        runtime.InitiatorRelationshipAtStart = initRel;
        runtime.TargetRelationshipAtStart = targetRel;
        runtime.InitiatorMinOffset = initMin;
        runtime.InitiatorMaxOffset = initMax;
        runtime.TargetMinOffset = targetMin;
        runtime.TargetMaxOffset = targetMax;

        runtime.State = state;
        runtime.NextIntent = intent;

        return runtime;
    }
}
