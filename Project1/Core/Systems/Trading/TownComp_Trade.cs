using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Trading;

sealed class TradeRuntime(Actor giver, Actor recipient)
{
    enum States { Unstarted, Accepted, Offered, Declined, Complete, Disposed }
    internal EntityRefId
        Giver = giver.RefId,
        Recipient = recipient.RefId,
        Item;
    internal Tick TickInitiated = giver.World.CurrentTick;
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

    readonly Dictionary<EntityRefId, TradeRuntime> byGiver = [];
    readonly Dictionary<EntityRefId, TradeRuntime> byRecipient = [];
    readonly HashSet<TradeRuntime> all = [];

    public TownComp_Trade(Town town) : base(town)
    {
    }
    int expirationThreshold = Ticks.FromHours(1);
    public override void Tick()
    {
        var current = this.Map.World.CurrentTick;
        foreach(var trade in this.byGiver.Values.ToArray())
        {
            if (current - trade.TickInitiated >= expirationThreshold)
                trade.MarkDeclined();

            if (!trade.IsDisposed)
                continue;

            this.byGiver.Remove(trade.Giver);
            this.byRecipient.Remove(trade.Recipient);
        }
    }
    internal TradeRuntime RequestPayment(Actor giver, Actor recipient, int price)
    {
        var trade = new TradeRuntime(giver, recipient);
        return trade;
    }
    internal TradeRuntime Request(Actor giver, Actor recipient)
    {
        var trade = new TradeRuntime(giver, recipient);
        this.byGiver.Add(trade.Giver, trade);
        this.byRecipient.Add(trade.Recipient, trade);
        $"traderequest created {giver.LabelReadable} {recipient.LabelReadable}".ToConsole();
        return trade;
    }

    internal void Decline(Actor target)
    {
        this.byRecipient[target.RefId].MarkDeclined();
    }

    internal void Accept(Actor target)
    {
        this.byRecipient[target.RefId].MarkAccepted();
    }

    internal TradeRuntime GetTradeByGiver(Actor actor)
    {
        return this.byGiver[actor.RefId];
    }
    internal TradeRuntime GetTradeByRecipient(Actor actor)
    {
        return this.byRecipient[actor.RefId];
    }
    internal bool TryGetTradeByRecipient(Actor actor, out TradeRuntime trade)
    {
        return this.byRecipient.TryGetValue(actor.RefId, out trade);
    }
    internal bool TryGetTradeByGiver(Actor actor, out TradeRuntime trade)
    {
        return this.byGiver.TryGetValue(actor.RefId, out trade);
    }
    internal void MarkAccepted(Actor target)
    {
        this.byRecipient[target.RefId].MarkAccepted();
    }
    internal void MarkDeclined(Actor target)
    {
        this.byRecipient[target.RefId].MarkDeclined();
    }
    internal void MarkOffered(Actor initiator)
    {
        this.byGiver[initiator.RefId].MarkOffered();
    }
    internal void MarkComplete(Actor target)
    {
        this.byRecipient[target.RefId].MarkComplete();
    }
    internal void MarkItem(Actor giver, Entity item)
    {
        this.byGiver[giver.RefId].SetItem(item);
    }

    internal void MarkDisposed(TradeRuntime trade)
    {
        trade.MarkDisposed();
    }
}
