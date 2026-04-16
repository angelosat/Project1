using Project1.Core.Entities.Actors;
using Project1.Core.Skills;

namespace Project1.Core.Entities.Stats.Resolvers;


//public abstract class ActorStatResolver : StatResolver
//{
//    //public float CalculateStat(Actor obj)
//    //{
//    //    throw new System.NotImplementedException();
//    //}

//}
public abstract class StatResolver
{
    public abstract float CalculateStat(Entity obj);
    public virtual float CalculateStat(Skill skill) => skill.Level;
}
