using Project1.Core.Entities;
using Project1.Core.Systems.Materials;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project1.Core.Quests;

public abstract class QuestRuntime(QuestId id, int reward) : Inspectable
{
    readonly internal QuestId Id = id;
    internal int Reward = reward;
    //internal abstract string LabelReadable { get; }
}

internal sealed class FetchQuestRuntime(QuestId id, int reward, MaterialRefinementDef refinement, MaterialDef material) : QuestRuntime(id, reward)
{
    internal readonly MaterialRefinementDef Refinement = refinement;
    internal readonly MaterialDef Material = material;

    public override string LabelReadable => $"Deliver {this.Material.LabelReadable} {this.Refinement.LabelReadable}";
}
