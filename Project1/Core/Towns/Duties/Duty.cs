using Project1.Core.Helpers;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Serialization;

namespace Project1.Core.Towns.Duties
{
    public class Duty : Observable, ISerializableNewNew<Duty>, ISaveableNewNew<Duty>
    {
        public DutyDef Def;
        const byte InitialPriority = 5, MaxPriority = 10;
        byte _priority = InitialPriority;
        public byte Priority
        {
            get => this._priority;
            private set => this._priority = (byte)(value >= 0 ? value % MaxPriority : MaxPriority + (value % MaxPriority));
        }
        public bool Enabled => this._priority != 0;
        Duty()
        {

        }
        public Duty(DutyDef def)
        {
            this.Def = def;
        }
        public void Toggle()
        {
            this.Priority = (byte)(this.Priority == 0 ? InitialPriority : 0);
            this.NotifyUpdated();
        }
        public void ApplyPriorityDelta(int delta)
        {
            this.Priority = (byte)(this._priority + delta);
            this.NotifyUpdated();
        }
        public Observable asObservable => this as Observable;
        public override string ToString()
        {
            return $"{this.Def.Name}: {this.Priority}";
        }
        public IDataWriter Write(IDataWriter w)
        {
            w.Write(this.Def);
            w.Write(this._priority);
            return w;
        }

        public Duty Read(IDataReader r)
        {
            this.Def = r.ReadDef<DutyDef>();
            this.Priority = r.ReadByte();
            return this;
        }
        static public Duty Create(IDataReader r) 
        {
            var def = r.ReadDef<DutyDef>();
            var prio = r.ReadByte();
            return new Duty(def) { Priority = prio };
        }
            
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            //this.Priority.Save(tag, "Priority");
            tag.Save("Priority", this.Priority);
            this.Def.Name.Save(tag, "Def");
            return tag;
        }

        public static Duty Create(SaveTag tag)
        {
            var prio = tag.GetValue<byte>("Priority");
            var def = tag.LoadDef<DutyDef>("Def");
            return new(def) { Priority = prio };
        }
    }
}
