using Microsoft.Xna.Framework;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.AI
{
    public sealed class AIState : Inspectable
    {
        public SortedSet<Threat> Threats = new();
        public PathingSync PathFinder = new();
        public bool Autonomy = true;
        public Queue<AITask> TaskQueue = new();

        private AITask _currentTask;
        public AITask CurrentTask
        {
            get => this._currentTask;
            set => this._currentTask = value;
        }

        private BehaviorPerformTask _currentTaskBehavior;
        public BehaviorPerformTask CurrentTaskBehavior
        {
            get => this._currentTaskBehavior;
            set
            {
                if (this.Parent.Net is Server)
                    PacketTaskUpdate.Send(this.Parent.Net as Net.Server, this.Parent.RefId, value?.GetType().Name ?? "Idle");
                this._currentTaskBehavior = value;
            }
        }

        public string TaskString = "none";
        public override string ToString()
        {
            return this.CurrentTask != null ? "Task: " + this.CurrentTask.ToString() : this.TaskString;
        }

        readonly Dictionary<JobDef, Job> Jobs = JobDefOf.All.ToDictionary(i => i, i => new Job(i));

        public Dictionary<string, object> Blackboard = new();
       
        public static AIConversationManager ConversationManager = new();
        public AIConversationManager.Conversation CurrentConversation;
        public IItemPreferencesManager ItemPreferences;
        public Actor Parent; //use this?
        public Dictionary<string, object> Properties = new();
        public Knowledge Knowledge;
        public AILog History = new();
        public GameObject Target;
        public List<GameObject> NearbyEntities { get; set; }
        public bool InSync;
        public PathFinding.Path Path;
        public Vector3 Leash;
        public int JobFindTimer;
        public GameObject Talker;
        public Progress Attention = new();
        public Actor ConversationPartner, TradingPartner;
        public Dictionary<Actor, ConversationTopic> CommunicationPending = new();
        public float AttentionDecayDefault = 1;
        public float AttentionDecay = 1;

        public AIState(Actor actor)
        {
            this.Parent = actor;
            this.NearbyEntities = new List<GameObject>();
            this.ItemPreferences = new ItemPreferencesManager(actor);
        }
        public static bool TryGetState(GameObject entity, out AIState state)
        {
            if (entity.TryGetComponent(out AIComponent ai))
                state = ai.State;
            else
                state = null;
            return state != null;
        }
        public static AIState GetState(GameObject entity)
        {
            return entity.GetComponent<AIComponent>().State;
        }

        public bool HasJob(JobDef job)
        {
            return this.Jobs.TryGetValue(job, out var j) && j.Enabled;
        }
        public void ToggleJob(JobDef job)
        {
            this.Jobs[job].Toggle();
        }
        public bool IsJobEnabled(JobDef job)
        {
            return this.Jobs[job].Enabled;
        }
        public Job GetJob(JobDef def)
        {
            return this.Jobs[def];
        }
        public IEnumerable<Job> GetJobs()
        {
            foreach (var j in this.Jobs.Values)
                yield return j;
        }
        internal void Generate(GameObject npc, RandomThreaded random)
        {
        }

        public void Write(IDataWriter w)
        {
            this.Jobs.Sync(w);
            this.ItemPreferences.Write(w); // sync to clients?
        }
        public void Read(IDataReader r)
        {
            this.Jobs.Sync(r);
            //this.Jobs.Write(r);
            this.ItemPreferences.Read(r); // sync to clients?
        }
        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(new SaveTag(SaveTag.Types.Vector3, "Leash", this.Leash));

            tag.Add((this.CurrentTask != null).Save("HasTask"));
            if(this.CurrentTask != null)
                tag.Add(this.CurrentTask.Save("Task"));
            if (this.CurrentTaskBehavior != null)
            {
                var bhavtag = this.CurrentTaskBehavior.Save("Behavior");
                bhavtag.Add(this.CurrentTaskBehavior.GetType().FullName.Save("TypeName"));
                tag.Add(bhavtag);
            }
            this.Path.TrySave(tag, "Path");
            this.Jobs.Save(tag, "Jobs", SaveTag.Types.String, key => key.Name);
            this.ItemPreferences.Save(tag, "ItemPreferences");
            return tag;
        }
        public void Load(SaveTag tag)
        {
            this.Leash = tag.GetValue<Vector3>("Leash");
            tag.TryGetTagValueOrDefault<bool>("HasTask", out var hastask);
            if (hastask)
            {
                var task = AITask.Load(tag["Task"]);
                this.CurrentTask = task;
            }
            tag.TryGetTag("Behavior", t =>
            {
                var bhavtype = (string)t["TypeName"].Value;
                this.CurrentTaskBehavior = Activator.CreateInstance(Type.GetType(bhavtype)) as BehaviorPerformTask;
                this.CurrentTaskBehavior.Load(t);
            });
            tag.TryLoad("Path", out this.Path);
            this.Jobs.TrySync(tag, "Jobs", keyTag => Def.TryGetDef<JobDef>((string)keyTag.Value));

            tag.TryGetTag("ItemPreferences", t => this.ItemPreferences.Load(t));
        }

        internal void Reset()
        {
            this.CurrentTask = null;
            this.LastBehavior = null;
            this.Path = null;
            this.CurrentTaskBehavior = null;
        }

        internal T1 GetBlackboardValue<T1>(string p)
        {
            return (T1)this.Blackboard[p];
        }
        internal T1 GetBlackboardValueOrDefault<T1>(string p, T1 defValue)
        {
            if (this.Blackboard.ContainsKey(p))
                return (T1)this.Blackboard[p];
            else return defValue;
        }

        /// <summary>
        /// TODO: very hacky, find better way
        /// </summary>
        /// <param name="parent"></param>
        internal void MapLoaded(Actor parent)
        {
            var targets = from v in this.Blackboard.Values
                          where v is TargetArgs
                          select v as TargetArgs;
            /// i dont need this anymore after phasing to targetargs lazily resolving entity id and passing the provider (client or server) at targetargs initialization
            //foreach (var t in targets)
            //    t.Map = parent.Map;
            this.CurrentTask?.MapLoaded(parent);
            if(this.CurrentTaskBehavior is not null)
                this.CurrentTaskBehavior.Actor = parent;
        }

        internal void ResolveReferences()
        {
            this.ItemPreferences.ResolveReferences();
        }

        internal void ObjectLoaded(GameObject parent)
        {
            this.CurrentTask?.ObjectLoaded(parent);
            this.CurrentTaskBehavior?.ObjectLoaded(parent);
        }

        public BehaviorPerformTask LastBehavior;

        public Queue<TargetArgs> MoveOrders = new();
        public AITask ForcedTask;

        public TargetArgs MoveOrder => this.MoveOrders.Any() ? this.MoveOrders.Peek() : TargetArgs.Null;

        internal void AddMoveOrder(TargetArgs target, bool enqueue)
        {
            this.Parent.EndCurrentTask();
            if (!enqueue)
                this.MoveOrders.Clear();
            this.MoveOrders.Enqueue(target);
        }

        internal void ForceTask(AITask task)
        {
            this.ForcedTask = task;
        }
    }
}
