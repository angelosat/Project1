using Project1.Core.Helpers;
using Project1.Framework.Serialization;
using System;

namespace Project1.Core.Systems.Effects;

public abstract record EffectConfig : ISerializableNewNew<EffectConfig>
{
    public abstract EffectDef Def { get; }

    //public required EffectSchedule Schedule;// { get; init; }

    public abstract string GetString(float scaling);

    public abstract EffectData CreateData();

    public static EffectConfig Create(IDataReader r)
    {
        var def = r.ReadDef<EffectDef>();
        var config = (EffectConfig)Activator.CreateInstance(def.ConfigClass);
        return config.Read(r);
    }

    public IDataWriter Write(IDataWriter w)
    {
        w.Write(this.Def);
        this.WriteInt(w);
        return w;
    }

    protected virtual void WriteInt(IDataWriter w) { }
    
    EffectConfig Read(IDataReader r)
    {
        this.ReadInt(r);
        return this;
    }

    protected virtual void ReadInt(IDataReader r) { }
}

