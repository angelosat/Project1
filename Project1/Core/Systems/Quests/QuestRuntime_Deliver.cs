using Project1.Core.AI.MetaRoles.Adventurer;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Systems.Materials;
using Project1.Framework;
using Project1.Framework.Serialization;

namespace Project1.Core.Systems.Quests;

internal sealed class QuestResolver_Deliver : QuestResolver
{
    public override void Tick(Actor actor, QuestRuntime quest)
    {
        var typedQ = (QuestRuntime_Deliver)quest;
        actor.AI.GetMeta<RoleAdventurerData>().NextDesiredLoot = typedQ.Key;
    }
}

internal sealed class QuestRuntime_Deliver : QuestRuntime
{
    internal override QuestDef Def => QuestDefOf.Deliver;
    //internal override QuestResolver_Deliver CreateResolver => new();

    public (MaterialRefinementDef, MaterialDef) Key => (this.Refinement, this.Material);
    internal MaterialRefinementDef Refinement { get; private set; }
    internal MaterialDef Material { get; private set; }

    public override string LabelReadable => $"Deliver {this.Count} {this.Material.LabelReadable} {this.Refinement.LabelReadable}";

    public QuestRuntime_Deliver(QuestId id, int reward, MaterialRefinementDef refinement, MaterialDef material) : base(id, reward)
    {
        Refinement = refinement;
        Material = material;
    }
    QuestRuntime_Deliver()
    {
        
    }
    protected override void OnSave(SaveTag tag)
    {
        tag.Save("Refinement", this.Refinement);
        tag.Save("Material", this.Material);
    }
    protected override void OnLoad(SaveTag tag)
    {
        this.Refinement = tag.LoadDef<MaterialRefinementDef>("Refinement");
        this.Material = tag.LoadDef<MaterialDef>("Material");
    }
    protected override void OnWrite(IDataWriter w)
    {
        w.Write(this.Refinement);
        w.Write(this.Material);
    }
    protected override void OnRead(IDataReader r)
    {
        this.Refinement = r.ReadDef<MaterialRefinementDef>();
        this.Material = r.ReadDef<MaterialDef>();
    }
}
