using Microsoft.Xna.Framework;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.Conversation;
using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors.Reserve;
using Project1.Core.AI.Labors;
using Project1.Core.AI.Net.Packets;
using Project1.Core.AI.Planners;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Pathing;
using Project1.Core.Towns;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.AI
{
    public sealed class AIState : Inspectable
    {
        public static AIConversationManager ConversationManager = new();
        readonly Dictionary<JobDef, Job> Jobs = JobDefOf.All.ToDictionary(i => i, i => new Job(i));
        public Progress Attention = new();
        public float AttentionDecay = 1;
        public float AttentionDecayDefault = 1;
        public bool Autonomy = true;
        public Dictionary<string, object> Blackboard = new();
        public Dictionary<Actor, ConversationTopic> CommunicationPending = new();
        public Actor ConversationPartner, TradingPartner;
        public AIConversationManager.Conversation CurrentConversation;
        public PlannerDef CurrentPlanner;
        public Plan ForcedTask;
        public AILog Log;
        public bool InSync;
        public ItemPreferencesManager ItemPreferences;
        public int JobFindTimer;
        public Knowledge Knowledge;
        public BehaviorExecutePlan LastBehavior;
        public Vector3 Leash;
        public Queue<TargetArgs> MoveOrders = new();
        public Actor Owner; //use this?
        public Path Path;
        public PathingSync PathFinder = new();
        public Dictionary<string, object> Properties = [];
        public GameObject Talker;
        public GameObject Target;
        //public Queue<BehaviorExecutePlan> TaskQueue = [];
        //public Stack<BehaviorExecutePlan> TaskStack = [];
        public Queue<Behavior> TaskQueue = [];
        public Stack<Behavior> TaskStack = [];
        public string TaskString = "none";
        public SortedSet<Threat> Threats = [];
        public Plan CurrentPlan => this.Behavior?.Plan;

        public AIState(Actor actor)
        {
            this.Owner = actor;
            this.NearbyEntities = new List<GameObject>();
            this.ItemPreferences = new ItemPreferencesManager(actor);
            this.Log = new(actor);
        }
        private void Enqueue(BehaviorExecutePlan bhav)
        {
            this.TaskQueue.Enqueue(bhav);
        }
        private void Push(BehaviorExecutePlan bhav)
        {
            this.TaskStack.Push(bhav);
        }
        internal void AddMoveOrder(TargetArgs target, bool enqueue)
        {
            this.Owner.EndCurrentTask();
            if (!enqueue)
                this.MoveOrders.Clear();
            this.MoveOrders.Enqueue(target);
        }

        internal void ForceTask(Plan task)
        {
            this.ForcedTask = task;
        }
        internal void Generate(GameObject npc, RandomThreaded random)
        {
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
            this.CurrentPlan?.MapLoaded(parent);
            this.Behavior?.Actor = parent;
        }

        internal bool NextTask()
        {
            this.Behavior?.CleanUp();
            if (this.TaskStack.Count > 0)
            {
                TaskStack.Pop();
                if(TaskStack.Count > 0)
                    PacketReportPlan.SendReportBehavior(this.Owner, TaskStack.Peek());
                return true;
            }
            else if (TaskQueue.Count > 0)
            {
                TaskQueue.Dequeue();
                if (TaskQueue.Count > 0)
                    PacketReportPlan.SendReportBehavior(this.Owner, TaskQueue.Peek());
                return true;
            }
            PacketReportPlan.SendReportBehavior(this.Owner, null);
            return false;
        }

        internal void ObjectLoaded(GameObject parent)
        {
            this.Behavior?.Plan.ObjectLoaded(parent);
            this.Behavior?.ObjectLoaded(parent);
        }

        internal void Reset()
        {
            this.Path = null;
            this.TaskQueue.Clear();
            this.TaskStack.Clear();
            PacketReportPlan.SendReportBehavior(this.Owner, null);

        }

        internal void ResolveReferences()
        {
        }
        internal void OnAttachedToMap()
        {
            this.ItemPreferences.ResolveReferences();

        }
        public void Assign(BehaviorExecutePlan bhav)
        {
            PacketReportPlan.SendReportBehavior(this.Owner, bhav);
            bhav.Plan.Actor = this.Owner;
            if (bhav.Plan.IsImmediate)
                this.Push(bhav);
            else
                this.Enqueue(bhav);
        }
        public bool TryAssign(Plan task)
        {
            var bhav = task.CreateBehavior(this.Owner);
            if (!bhav.ReserveBase())
            {
                this.Owner.Unreserve();
                return false;
            }
            this.Assign(bhav);
            return true;
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
        public static AIState GetState(GameObject entity)
        {
            return entity.GetComponent<AIComponent>().State;
        }

        public bool HasJob(JobDef job)
        {
            return this.Jobs.TryGetValue(job, out var j) && j.Enabled;
        }
        public bool IsJobEnabled(JobDef job)
        {
            return this.Jobs[job].Enabled;
        }
        public void Load(SaveTag tag)
        {
            this.Leash = tag.GetValue<Vector3>("Leash");
            var tagStack = tag["TaskStack"];
            var listStack = tagStack.Value as List<SaveTag>;
            foreach(var t in listStack)
            {
                var tasktag = t["Task"];
                var task = Plan.Load(tasktag);
                var bhavtag = t["Behavior"];
                var bhav = task.CreateBehavior(this.Owner);
                bhav.Plan = task;
                bhav.Load(bhavtag);
                this.TaskStack.Push(bhav);
            }
            var tagQueue = tag["TaskQueue"];
            var listQueue = tagQueue.Value as List<SaveTag>;
            foreach (var t in listQueue)
            {
                var tasktag = t["Task"];
                var task = Plan.Load(tasktag);
                var bhavtag = t["Behavior"];
                var bhav = task.CreateBehavior(this.Owner);
                bhav.Plan = task;
                bhav.Load(bhavtag);
                this.TaskQueue.Enqueue(bhav);
            }

            tag.TryLoad("Path", out this.Path);
            this.Jobs.TrySync(tag, "Jobs", keyTag => Def.TryGetDef<JobDef>((string)keyTag.Value));
            tag.TryLoadDefOut("Planner", out this.CurrentPlanner);
            tag.TryGetTag("ItemPreferences", t => this.ItemPreferences.Load(t));
        }
        public void Read(IDataReader r)
        {
            r.ReadValuesWithInferredKeys(this.Jobs, v => v.Def);
            this.ItemPreferences.Read(r); // sync to clients?
        }
        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(new SaveTag(SaveTag.Types.Vector3, "Leash", this.Leash));
            var tagStack = new SaveTag(SaveTag.Types.List, "TaskStack", SaveTag.Types.Compound);
            foreach (var bhav in this.TaskStack)
            {
                var tupleTag = new SaveTag(SaveTag.Types.Compound);
                tupleTag.Add(bhav.Plan.Save("Task"));
                var bhavtag = bhav.Save("Behavior");
                //bhavtag.Add(bhav.GetType().FullName.Save("TypeName"));
                tupleTag.Add(bhavtag);
                tagStack.Add(tupleTag);
            }
            tag.Add(tagStack);
            var tagQueue = new SaveTag(SaveTag.Types.List, "TaskQueue", SaveTag.Types.Compound);
            foreach (var bhav in this.TaskQueue)
            {
                var tupleTag = new SaveTag(SaveTag.Types.Compound);
                tupleTag.Add(bhav.Plan.Save("Task"));
                var bhavtag = bhav.Save("Behavior");
                //bhavtag.Add(bhav.GetType().FullName.Save("TypeName"));
                tupleTag.Add(bhavtag);
                tagQueue.Add(tupleTag);
            }
            tag.Add(tagQueue);

            this.Path.TrySave(tag, "Path");
            this.Jobs.Save(tag, "Jobs", SaveTag.Types.String, key => key.Name);
            this.ItemPreferences.Save(tag, "ItemPreferences");
            if(this.CurrentPlanner is not null)
                tag.Save("Planner", this.CurrentPlanner);
            return tag;
        }
        public void ToggleJob(JobDef job)
        {
            this.Jobs[job].Toggle();
        }
        public override string ToString()
        {
            return this.CurrentPlan != null ? "Task: " + this.CurrentPlan.ToString() : this.TaskString;
        }
        public static bool TryGetState(GameObject entity, out AIState state)
        {
            if (entity.TryGetComponent(out AIComponent ai))
                state = ai.State;
            else
                state = null;
            return state != null;
        }

        public void Write(IDataWriter w)
        {
            w.WriteValues(this.Jobs);
            this.ItemPreferences.Write(w); // sync to clients?
        }
        public void Tick()
        {
            this.ItemPreferences.Tick();
        }

        //public IEnumerable<BehaviorExecutePlan> AllPlannedTasks => TaskStack.Concat(TaskQueue);
        //public BehaviorExecutePlan Behavior => this.TaskStack.Count > 0 ? this.TaskStack.Peek() : (this.TaskQueue.Count > 0 ? this.TaskQueue.Peek() : null);

        public IEnumerable<Behavior> AllPlannedTasks => TaskStack.Concat(TaskQueue);
        public Behavior Behavior => this.TaskStack.Count > 0 ? this.TaskStack.Peek() : (this.TaskQueue.Count > 0 ? this.TaskQueue.Peek() : null);

        public TargetArgs MoveOrder => this.MoveOrders.Any() ? this.MoveOrders.Peek() : TargetArgs.Null;
        public List<GameObject> NearbyEntities { get; set; }
    }
}
