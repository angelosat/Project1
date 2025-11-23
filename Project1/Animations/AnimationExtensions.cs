using System;
using System.Linq;
using Start_a_Town_.Animations;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    static public class AnimationExtensions
    {
        static public void AddAnimation(this GameObject obj, Animation animation)
        {
            var anicomp = obj.GetComponent<SpriteComponent>().Animations;// obj.Animations;
            if (anicomp.FirstOrDefault(a => a.Def == animation.Def) is Animation existing)
            {
                if (existing.WeightChange >= 0 && existing.State != AnimationStates.Removed)
                    throw new Exception(); // ANIMATION MIGHT STILL BE FADING OUT WHEN THE NEXT BEHAVIOR BEGINS AND ADDS THE SAME TYPE OF ANIMATION!
            }
            if (anicomp.Any(a => a == animation))
                throw new Exception();
            anicomp.Add(animation);
        }
        static public void CrossFade(this GameObject obj, Animation animation, bool preFade, int fadeLength, Func<float, float, float, float> fadeInterpolation)
        {
            animation.FadeIn(preFade, fadeLength, fadeInterpolation);
            obj.AddAnimation(animation);
        }
        static public void CrossFade(this GameObject obj, Animation animation, bool preFade, int fadeLength)
        {
            obj.CrossFade(animation, preFade, fadeLength, Interpolation.Lerp);
        }
    }
}
