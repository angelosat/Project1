using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Magic;
using Project1.Framework.Serialization;
using System;

namespace Project1.Core.Systems.Effects;

public abstract record EffectConfig
{
    public abstract string GetString(float scaling);

    public abstract EffectData CreateData();
}

public abstract class EffectData : ISerializable
{
    public abstract EffectDef Def { get; }

    public SpellSchoolDef School { get; init; }

    internal virtual void Perform(Actor actor)
    => this.Def.Worker.Perform(actor, this);

    internal virtual void Start(Actor actor)
        => this.Def.Worker.Start(actor, this);

    internal virtual void Finish(Actor actor)
        => this.Def.Worker.Finish(actor, this);

    public virtual ISerializable Read(IDataReader r)
    {
        throw new NotImplementedException();
    }

    public virtual void Write(IDataWriter w)
    {
        throw new NotImplementedException();
    }
}

public abstract class EffectData<T> : EffectData where T : EffectConfig
{
    internal override void Perform(Actor actor)
        => this.Def.Worker.Perform(actor, this);

    internal override void Start(Actor actor)
        => this.Def.Worker.Start(actor, this);

    internal override void Finish(Actor actor)
        => this.Def.Worker.Finish(actor, this);

    public abstract ISerializable Read(IDataReader r);

    public abstract void Write(IDataWriter w);

    
}
