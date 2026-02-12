using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Core.Towns;

namespace Project1.Core.AI.Labors
{
    public class Job : ISerializableNew<Job>, ISaveable//, ISerializable
    {
        public JobDef Def;
        const byte InitialPriority = 5, MaxPriority = 10;
        byte _Priority = InitialPriority;
        public byte Priority
        {
            get => this._Priority;
            set => this._Priority = (byte)(value >= 0 ? value % MaxPriority : MaxPriority + (value % MaxPriority));
        }
        public bool Enabled => this._Priority != 0;
        public Job()
        {

        }
        public Job(JobDef def)
        {
            this.Def = def;
        }
        public void Toggle()
        {
            this.Priority = (byte)(this.Priority == 0 ? InitialPriority : 0);
        }
        public override string ToString()
        {
            return $"{this.Def.Name}: {this.Priority}";
        }
        public void Write(IDataWriter w)
        {
            w.Write(this.Def.Name);
            w.Write(this._Priority);
        }

        public Job Read(IDataReader r)
        {
            this.Def = Core.Def.GetDef<JobDef>(r.ReadString());
            this.Priority = r.ReadByte();
            return this;
        }
        static public Job Create(IDataReader r) => new Job().Read(r);
            
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Priority.Save(tag, "Priority");
            this.Def.Name.Save(tag, "Def");
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            this.Priority = tag.GetValue<byte>("Priority");
            tag.TryGetTagValue<string>("Def", v => this.Def = Core.Def.GetDef<JobDef>(v));
            return this;
        }

        //public static ISerializableNew Create(IDataReader r) => new Job().Read(r);
    }
}
