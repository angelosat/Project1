using Project1.Core.Entities;
using Project1.Framework;

namespace Project1.Core.Systems.Quality;

[EnsureStaticCtorCall]
public static class QualitiesDefOf
{
    public static readonly EntityCompDef Comp = new("Quality", typeof(QualityComp));

    static QualitiesDefOf()
    {
        Def.Register(typeof(QualitiesDefOf));
    }
}

public static class QualityHelpers
{
    extension(GameObject item)
    {
        public QualityComp QualityComp => item.GetComponent<QualityComp>();
    }
    //public static QualityDef MapQualityToTier(int quality)
    //{
    //    QualityDefOf
    //}
}
