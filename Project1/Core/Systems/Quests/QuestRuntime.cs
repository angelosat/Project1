using Project1.Core.Helpers;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using System;
using System.Reflection;

namespace Project1.Core.Systems.Quests;

public readonly record struct QuestId(int Value)
{
    internal static readonly QuestId Null = new(0);
    public static implicit operator QuestId(int v) => new(v);
    public static implicit operator int(QuestId v) => v.Value;
}

public abstract class QuestRuntime : Inspectable, ISaveableNewNew<QuestRuntime>, ISerializableNew<QuestRuntime>
{
    internal abstract QuestDef Def { get; }
    internal QuestId Id { get; private set; }

    internal int Count = 10;
    internal int Reward;

    public QuestRuntime(QuestId id, int reward)
    {
        this.Id = id;
        this.Reward = reward;
    }

    protected QuestRuntime()
    {
        
    }
    public static QuestRuntime Create(SaveTag tag)
    {
        var def = tag.LoadDef<QuestDef>("Def");
        //var runtime = (QuestRuntime)Activator.CreateInstance(def.RuntimeType);
        var runtime = (QuestRuntime)Activator.CreateInstance(
            def.RuntimeType,
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            args: null,
            culture: null
            );
        runtime.Load(tag);
        return runtime;
    }

    public SaveTag Save(string name = "")
    {
        var tag = new SaveTag(SaveTag.Types.Compound, name);
        tag.Save("Def", this.Def);
        tag.Save("Id", (int)this.Id);
        tag.Save("Reward", this.Reward);
        tag.Save("Count", this.Count);
        this.OnSave(tag);
        return tag;
    }
    void Load(SaveTag tag)
    {
        this.Id = (QuestId)tag.LoadInt("Id");
        this.Reward = tag.LoadInt("Reward");
        if (tag.TryLoadInt("Count", out var count)) this.Count = count;
        this.OnLoad(tag);
    }
    protected virtual void OnSave(SaveTag tag) { }
    protected virtual void OnLoad(SaveTag tag) { }

    public void Write(IDataWriter w)
    {
        w.Write(this.Def);
        w.Write(this.Id);
        w.Write(this.Reward);
        w.Write(this.Count);
        this.OnWrite(w);
    }
    public static QuestRuntime Create(IDataReader r)
    {
        var def = r.ReadDef<QuestDef>();
        var runtime = (QuestRuntime)Activator.CreateInstance(
            def.RuntimeType,
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            args: null,
            culture: null
            );
        runtime.Read(r);
        return runtime;
    }
    public QuestRuntime Read(IDataReader r)
    {
        this.Id = r.ReadInt32();
        this.Reward = r.ReadInt32();
        this.Count = r.ReadInt32();
        this.OnRead(r);
        return this;
    }


    protected virtual void OnWrite(IDataWriter w) { }
    protected virtual void OnRead(IDataReader r) { }

   
}
