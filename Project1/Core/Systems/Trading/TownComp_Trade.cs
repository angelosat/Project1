using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.Towns;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Trading;

public readonly record struct TradeId(ulong Value)
{
    public static readonly TradeId Null = new(0);
    public static implicit operator TradeId(ulong v) => new(v);
    public static implicit operator ulong(TradeId v) => (ulong)v.Value;
}

sealed class TradeRuntime(TradeId id, Actor giver, Actor recipient)
{
    enum States { Unstarted, Accepted, Offered, Declined, Complete, Disposed }
    internal EntityRefId
        Giver = giver.RefId,
        Recipient = recipient.RefId,
        Item;
    internal TradeId Id { get; init; } = id;
    internal SimulationTick TickInitiated = giver.World.CurrentTick;
    States State = States.Unstarted;
    internal bool IsOffered => this.State == States.Offered;
    internal bool IsAccepted => this.State == States.Accepted;
    internal bool IsDeclined => this.State == States.Declined;
    internal bool IsComplete => this.State == States.Complete;
    internal bool IsDisposed => this.State == States.Disposed;
    internal void MarkOffered()
    {
        if (this.State != States.Accepted)
            throw new InvalidOperationException();
        this.State = States.Offered;
    }
    internal void MarkAccepted()
    {
        if (this.State != States.Unstarted)
            throw new InvalidOperationException();
        this.State = States.Accepted;
    }
    internal void MarkDeclined() 
        => this.State = States.Declined;
    internal void MarkComplete()
    {
        if (this.State != States.Offered)
            throw new InvalidOperationException();
        this.State = States.Complete;
    }
    internal void MarkDisposed()
    => this.State = States.Disposed;

    internal void SetItem(Entity item)
        => this.Item = item.RefId;


}
public sealed class TownComp_Trade : TownComp
{
    public override string Name => "Trade";

    readonly Dictionary<TradeId, TradeRuntime> byId = [];
    //readonly Dictionary<EntityRefId, TradeRuntime> byGiver = [];
    //readonly Dictionary<EntityRefId, TradeRuntime> byRecipient = [];
    readonly HashSet<TradeRuntime> all = [];
    TradeId NextTradeId => ++field;
    public TownComp_Trade(Town town) : base(town)
    {
    }
    SimulationTick expirationThreshold = (ulong)Ticks.FromHours(1);
    internal override void Tick()
    {
        var current = this.Map.World.CurrentTick;
        foreach(var trade in this.byId.Values.ToArray())
        {
            if (current - trade.TickInitiated >= expirationThreshold)
                trade.MarkDeclined();

            if (!trade.IsDisposed)
                continue;

            //this.byGiver.Remove(trade.Giver);
            //this.byRecipient.Remove(trade.Recipient);
            this.byId.Remove(trade.Id);
        }
    }
  
    internal TradeRuntime Request(Actor giver, Actor recipient)
    {
        var trade = new TradeRuntime(this.NextTradeId, giver, recipient);
        //this.byGiver.Add(trade.Giver, trade);
        //this.byRecipient.Add(trade.Recipient, trade);
        this.byId.Add(trade.Id, trade);
        $"traderequest created {giver.LabelReadable} {recipient.LabelReadable}".ToConsole();
        return trade;
    }
   
    internal TradeRuntime GetTradeById(TradeId id)
       => this.byId[id];

    internal void MarkAccepted(TradeId id)
    {
        this.byId[id].MarkAccepted();
    }
    internal void MarkDeclined(TradeId id)
    {
        this.byId[id].MarkDeclined();
    }
    internal void MarkOffered(TradeId id)
    {
        this.byId[id].MarkOffered();
    }
    internal void MarkComplete(TradeId id)
    {
        this.byId[id].MarkComplete();
    }
    internal void MarkItem(TradeId id, Entity item)
    {
        this.byId[id].SetItem(item);
    }

    internal void MarkDisposed(TradeRuntime trade)
    {
        trade.MarkDisposed();
    }
}
