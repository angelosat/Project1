using Project1.Core.AI.Labors;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Interfaces;
using Project1.Framework.IO;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns
{
    public class WorkerProps : ISaveable, ISerializableNew<WorkerProps>
    {
        public int ActorID;
        public Dictionary<JobDef, Job> Jobs = new();
        public WorkerProps()
        {

        }
        public WorkerProps(Actor actor, params JobDef[] jobDefs) : this(actor.RefId, jobDefs)
        {
        }
        public WorkerProps(int actorID, params JobDef[] jobDefs)
        {
            this.ActorID = actorID;
            foreach (var j in jobDefs)
                this.Jobs.Add(j, new Job(j));
        }
        public Job GetJob(JobDef def)
        {
            return this.Jobs[def];
        }
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.ActorID.Save(tag, "ActorID");
            this.Jobs.Values.SaveNewBEST(tag, "Jobs");
            return tag;
        }
        public ISaveable Load(SaveTag tag)
        {
            this.ActorID = tag.GetValue<int>("ActorID");
            tag.TryGetTag("Jobs", v => this.Jobs = v.LoadArray<Job>().ToDictionary(j => j.Def, j => j));
            return this;
        }
        public void Write(IDataWriter w)
        {
            w.Write(this.ActorID);
            this.Jobs.Values.Write(w);
        }
        public WorkerProps Read(IDataReader r)
        {
            this.ActorID = r.ReadInt32();
            this.Jobs = r.ReadArrayNew<Job>().ToDictionary(j => j.Def, j => j);
            return this;
        }

        public static WorkerProps Create(IDataReader r) => new WorkerProps().Read(r);
    }
}
