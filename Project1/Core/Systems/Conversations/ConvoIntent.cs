using Project1.Core.AI.Personality;
using Project1.Core.Entities.Actors;
using Project1.Core.Skills;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using System;

namespace Project1.Core.Systems.Conversations;

public sealed class ConvoIntentDef(string name, Type workerType) : Def(name)
{
    internal readonly ConvoIntent Worker = ActivatorSafe<ConvoIntent>.CreateInstance(workerType);
}

[EnsureStaticCtorCall]
public static class ConvoIntentDefOf
{
    public static readonly ConvoIntentDef Compliment = new("Compliment", typeof(ConvoIntent_Compliment));
    public static readonly ConvoIntentDef Insult = new("Insult", typeof(ConvoIntent_Insult));
    static ConvoIntentDefOf()
    {
        Def.Register(typeof(ConvoIntentDefOf));
    }
}

sealed class ConvoIntent_Compliment : ConvoIntent
{
    //internal override ConvoIntentDef Def => ConvoIntentDefOf.Compliment;

    protected override ConvoDeltas OnCalculate(ConvoInputs inputs, float magnitude)
    {
        var sign = magnitude > 0 ? 1 : -1;
        var finalmagnitude = (int)Math.Ceiling(Math.Abs(inputs.TalkerSkill * magnitude));
        var xp = 10 + finalmagnitude;
        //var talkerNeedDelta = (1 - inputs.TalkerSelflessness) * magnitude / 2;
        //var listenerNeedDelta = Math.Max(0, sign * (1 - inputs.ListenerResilience) * magnitude / 2); 
        var talkerNeedDelta = (1 - inputs.TalkerSelflessness) * xp;
        var listenerNeedDelta = Math.Max(0, sign * (1 - inputs.ListenerResilience) * xp);
        //var listenerRel = sign * magnitude;
        var listenerRel = sign * (int)Math.Ceiling(finalmagnitude / 33f);
        var talkerRel = 0;
        if (sign < 0)
        {
            float harshness = 1 - inputs.TalkerManner;
            talkerRel = -(int)Math.Ceiling(finalmagnitude * harshness / 50f);
        }
        return new(talkerNeedDelta, listenerNeedDelta, xp, talkerRel, listenerRel);
    }
}
sealed class ConvoIntent_Insult : ConvoIntent
{
    //internal override ConvoIntentDef Def => ConvoIntentDefOf.Insult;
    protected override ConvoDeltas OnCalculate(ConvoInputs inputs, float magnitude)
    {
        throw new NotImplementedException();
    }
}

internal sealed class ConvoIntentRuntime(ConvoIntentDef def, float magnitude) : ISaveableNewNew<ConvoIntentRuntime>
{
    internal ConvoIntentDef Def = def;
    internal float Magnitude = magnitude;

    internal ConvoDeltas Calculate(Actor talker, Actor listener)
       => this.Def.Worker.Calculate(talker, listener, this.Magnitude);


    public SaveTag Save(string name = "")
    {
        var tag = new SaveTag(SaveTag.Types.Compound, name);
        tag.Save("Def", this.Def);
        tag.Save("Magnitude", this.Magnitude);
        return tag;
    }

    public static ConvoIntentRuntime Create(SaveTag tag)
    {
        var def = tag.LoadDef<ConvoIntentDef>("Def");
        var mag = tag.LoadSingle("Magnitude");
        return new(def, mag);
    }
}

internal abstract class ConvoIntent
{
    //internal abstract ConvoIntentDef Def { get; }
    //internal float Magnitude = magnitude;
    int Skill(Actor actor) => actor.Skills.GetLevel(SkillDefOf.Social);
    float Manner(Actor actor) => actor.Personality.GetPercentage(TraitDefOf.Manners);
    float Selflessness(Actor actor) => actor.Personality.GetPercentage(TraitDefOf.Selflessness);
    float Resilience(Actor actor) => actor.Personality.GetPercentage(TraitDefOf.Resilience);
    protected ConvoInputs Deconstruct(Actor talker, Actor listener)
    {
        var talkerSkill = this.Skill(talker);
        var talkerManner = this.Manner(talker);
        var talkerSelflessness = this.Selflessness(talker);
        var listenerResilience = this.Resilience(listener);
        return new(talkerSkill, talkerManner, talkerSelflessness, listenerResilience);
    }
    internal ConvoDeltas Calculate(Actor talker, Actor listener, float magnitude)
        => this.OnCalculate(this.Deconstruct(talker, listener), magnitude);
    protected abstract ConvoDeltas OnCalculate(ConvoInputs inputs, float magnitude);

}
