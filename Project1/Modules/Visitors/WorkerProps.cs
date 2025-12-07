using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    public class WorkerProps : ISaveable, ISerializable
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
            tag.TryGetTag("Jobs", v => this.Jobs = v.LoadList<Job>().ToDictionary(j => j.Def, j => j));
            return this;
        }
        public void Write(BinaryWriter w)
        {
            w.Write(this.ActorID);
            this.Jobs.Values.Write(w);
        }
        public ISerializable Read(IDataReader r)
        {
            this.ActorID = r.ReadInt32();
            this.Jobs = r.ReadArray<Job>().ToDictionary(j => j.Def, j => j);
            return this;
        }
    }
}
