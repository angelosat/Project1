using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Systems.ItemRoles;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.AI;

internal sealed class Knowledge : Inspectable
{
    internal readonly Dictionary<EntityRefId, ItemEvaluation> Items = [];

    internal void Register(Entity item, ItemEvaluation evaluation)
        => this.Items.Add(item.RefId, evaluation);
    //{
    //    if (item.HasOwner && item.OwnerId != this.Actor.RefId)
    //        throw new InvalidOperationException("shouldn't be evaluating to register an item owned by another actor");
    //    this.Items.Add(item.RefId, evaluation);
    //}
    internal ItemEvaluation Query(Entity item)
        => this.Items.TryGetValue(item.RefId, out var entry) ? entry : default;

    internal bool TryQuery(Entity item, out ItemEvaluation entry)
        => this.Items.TryGetValue(item.RefId, out entry);
}
