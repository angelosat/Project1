using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;
using System.IO;

namespace Project1.Core.AI.Reservations;

public sealed class Reservation
{
    public EntityRefId Actor;
    public InteractionTarget Target;
    int _amount;
    public int Amount
    {
        get => this._amount;
        set => this._amount = value;
    }
    public override string ToString()
        => $"Actor: {this.Actor} Target: {this.Target} Amount: {this.Amount}";
    public Reservation(GameObject actor, InteractionTarget target, int stackcount)
    {
        if (stackcount == -1)
            throw new Exception();
        this.Actor = actor.RefId;
        this.Target = target;
        this.Amount = stackcount;
        if (target.HasObject && stackcount > target.Object.StackSize)
            throw new InvalidOperationException($"reservation quantity request exceeded target item's current stacksize");
    }
    public void Write(BinaryWriter w)
    {
        w.Write(this.Actor);
        this.Target.Write(w);
        w.Write(this.Amount);
    }
    public Reservation(MapBase map, Packet pck)
    {
        var r = pck.PacketReader;
        this.Actor = r.ReadInt32();
        this.Target = InteractionTarget.Read(map.World, r);
        this.Amount = r.ReadInt32();
    }
    public SaveTag Save()
    {
        var tag = new SaveTag(SaveTag.Types.Compound);
        //tag.Add(this.Actor.Save("ActorID"));
        tag.Save("ActorID", this.Actor);
        tag.Add(this.Target.Save("Target"));
        tag.Add(this.Amount.Save("Amount"));
        return tag;
    }
    public Reservation(MapBase map, SaveTag tag)
    {
        //this.Actor = tag.GetValue<int>("ActorID");
        this.Actor = tag.LoadEntityRefId("ActorID");
        this.Target = new InteractionTarget(map.World, tag["Target"]);
        this.Amount = tag.GetValue<int>("Amount");
    }
}
