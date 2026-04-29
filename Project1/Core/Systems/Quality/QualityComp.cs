using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using Project1.Framework.UI.Primitives;
using System;
using System.Collections.Generic;

namespace Project1.Core.Systems.Quality;

public class QualityComp : EntityComp
{
    readonly ProgressInt LevelInt = new(100, 0);
    public QualityDef Tier = QualityDefOf.Common;

    public override EntityCompDef CompDef => QualitiesDefOf.Comp;

    public override string Name => "Quality";

    public int Level => this.LevelInt.Value;

    public void SetLevel(int level) => this.LevelInt.SetValue(level);

    internal override IEnumerable<Control> GetTooltipControls()
    {
        yield return new LabelNew(()=>this.Tier.LabelReadable) { Fill = Color.Gold, TextColorFunc = () => Color.Gold };
    }

    internal override void CopyFrom(EntityComp source)
    {
        this.LevelInt.SetValue(new Random().Next(100));
        var typed = (QualityComp)source;
        //this.LevelInt.SetValue(typed.LevelInt.Value);
        this.Tier = typed.Tier;
    }

    public override void Randomize(GameObject parent, RandomThreaded random)
    {
        this.Tier = QualitySystem.Random;
    }

    public override void Write(IDataWriter w)
    {
        this.LevelInt.Write(w);
        w.Write(this.Tier);
    }
    public override void Read(IDataReader r)
    {
        this.LevelInt.Read(r);
        this.Tier = r.ReadDef<QualityDef>();
    }
}
