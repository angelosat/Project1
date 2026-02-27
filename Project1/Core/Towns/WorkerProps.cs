using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Duties;
using Project1.Framework;
using Project1.Framework.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns
{
    public class WorkerProps : ISaveable, ISerializableNew<WorkerProps>
    {
        public int ActorID;
        public Dictionary<DutyDef, Duty> Jobs = [];
        public WorkerProps()
        {

        }
        public WorkerProps(Actor actor, params DutyDef[] jobDefs) : this(actor.RefId, jobDefs)
        {
        }
        public WorkerProps(int actorID, params DutyDef[] jobDefs)
        {
            this.ActorID = actorID;
            foreach (var j in jobDefs)
                this.Jobs.Add(j, new Duty(j));
        }
        public Duty GetJob(DutyDef def)
        {
            return this.Jobs[def];
        }
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.ActorID.Save(tag, "ActorID");
            tag.Save("Jobs", this.Jobs.Values);
            return tag;
        }
        public ISaveable Load(SaveTag tag)
        {
            this.ActorID = tag.GetValue<int>("ActorID");
            if(tag.TryLoadListOut<Duty>("Jobs", out var jobsList))
                foreach(var job in jobsList)
                    this.Jobs[job.Def] = job;
            return this;
        }
        public void Write(IDataWriter w)
        {
            w.Write(this.ActorID);
            w.WriteNew(this.Jobs.Values);
        }
        public WorkerProps Read(IDataReader r)
        {
            this.ActorID = r.ReadInt32();
            this.Jobs = r.ReadListNewNew<Duty>().ToDictionary(i => i.Def, i => i);
            return this;
        }

        public static WorkerProps Create(IDataReader r) => new WorkerProps().Read(r);
    }
}
