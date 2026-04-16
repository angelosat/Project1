using Project1.Core.Entities.Actors;
using System.Diagnostics;

namespace Project1.Core.Systems.Conversations;

sealed class ConversationRuntime(EntityRefId initiator, EntityRefId target)
{
    enum States { Requested, Accepted, Running, Finished }
    internal EntityRefId Initiator = initiator, Target = target;
    States State;
    internal EntityRefId CurrentTalker { get; private set; } = initiator;
    internal EntityRefId CurrentListener => this.CurrentTalker == this.Initiator ? this.Target : this.Initiator;

    internal int InitiatorRelationshipAtStart { get; private set; }
    internal int TargetRelationshipAtStart { get; private set; }
    int InitiatorMinOffset, InitiatorMaxOffset,
        TargetMinOffset, TargetMaxOffset;

    internal bool IsRequested => this.State == States.Requested;
    internal bool IsFinished => this.State == States.Finished;

    public ConvoIntent NextIntent { get; internal set; }
    public ConversationRuntime(Actor initiator, Actor target) : this(initiator.RefId, target.RefId)
    {
        this.InitiatorRelationshipAtStart = initiator.Relationships.Get(target);
        this.TargetRelationshipAtStart = target.Relationships.Get(initiator);
    }
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
}
