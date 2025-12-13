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

        public static AIConversationManager ConversationManager = new();

        private BehaviorPerformTask _currentTaskBehavior;

        readonly Dictionary<JobDef, Job> Jobs = JobDefOf.All.ToDictionary(i => i, i => new Job(i));
        public Progress Attention = new();
        public float AttentionDecay = 1;
        public float AttentionDecayDefault = 1;
        public bool Autonomy = true;

        public Dictionary<string, object> Blackboard = new();
        public Dictionary<Actor, ConversationTopic> CommunicationPending = new();
        public Actor ConversationPartner, TradingPartner;
        public AIConversationManager.Conversation CurrentConversation;
        public AITask CurrentTask => this.Behavior?.Task;
        public AITask ForcedTask;
        public AILog History = new();
        public bool InSync;
        public ItemPreferencesManager ItemPreferences;
        public int JobFindTimer;
        public Knowledge Knowledge;

        public BehaviorPerformTask LastBehavior;
        public Vector3 Leash;

        public Queue<TargetArgs> MoveOrders = new();
        public Actor Parent; //use this?
        public PathFinding.Path Path;
        public PathingSync PathFinder = new();
        public Dictionary<string, object> Properties = new();
        public GameObject Talker;
        public GameObject Target;
        //public Queue<(AITask task, BehaviorPerformTask behavior)> TaskQueue = [];
        //public Stack<(AITask task, BehaviorPerformTask behavior)> TaskStack = [];
        public Queue<BehaviorPerformTask> TaskQueue = [];
        public Stack<BehaviorPerformTask> TaskStack = [];

        public string TaskString = "none";
        public SortedSet<Threat> Threats = new();

        public AIState(Actor actor)
        {
            this.Parent = actor;
            this.NearbyEntities = new List<GameObject>();
            this.ItemPreferences = new ItemPreferencesManager(actor);
        }

        private void Enqueue(BehaviorPerformTask bhav)
        {
            this.TaskQueue.Enqueue(bhav);
        }

        //public BehaviorPerformTask CurrentTaskBehavior
        //{
        //    get => this._currentTaskBehavior;
        //    set
        //    {
        //        if (this.Parent.Net is Server)
        //            PacketTaskUpdate.Send(this.Parent.Net as Net.Server, this.Parent.RefId, value?.GetType().Name ?? "Idle");
        //        this._currentTaskBehavior = value;
        //    }
        //}

        private void NotifyTaskUpdate()
        {
            if (Parent.Net is Server)
                PacketTaskUpdate.Send(
                    Parent.Net as Server,
                    Parent.RefId,
                    Behavior?.GetType().Name ?? "Idle"
                );
        }
        private void Push(BehaviorPerformTask bhav)
        {
            this.TaskStack.Push(bhav);
        }

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
            //foreach (var t in targets)
            //    t.Map = parent.Map;
            this.CurrentTask?.MapLoaded(parent);
            this.Behavior?.Actor = parent;
            //if (this.CurrentTaskBehavior is not null)
            //    this.CurrentTaskBehavior.Actor = parent;
        }

        internal bool NextTask()
        {
            this.Behavior?.CleanUp();
            if (this.TaskStack.Count > 0)
            {
                TaskStack.Pop();
                return true;
            }
            else if (TaskQueue.Count > 0)
            {
                TaskQueue.Dequeue();
                return true;
            }
            return false;
        }

        internal void ObjectLoaded(GameObject parent)
        {
            //this.CurrentTask?.ObjectLoaded(parent);
            //this.CurrentTaskBehavior?.ObjectLoaded(parent);
            this.Behavior?.Task.ObjectLoaded(parent);
            this.Behavior?.ObjectLoaded(parent);
        }

        internal void Reset()
        {
            //this.CurrentTask = null;
            this.LastBehavior = null;
            this.Path = null;
            //this.CurrentTaskBehavior = null;
            this.TaskQueue.Clear();
            this.TaskStack.Clear();
        }

        internal void ResolveReferences()
        {
            this.ItemPreferences.ResolveReferences();
        }

        public void Assign(BehaviorPerformTask bhav)
        {
            if (bhav.Task.IsImmediate)
                this.Push(bhav);
            else
                this.Enqueue(bhav);
        }
        public bool TryAssign(AITask task)
        {
            var bhav = task.CreateBehavior(this.Parent);
            if (!bhav.InitBaseReservations())
            {
                this.Parent.Unreserve();
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
            //tag.TryGetTagValueOrDefault<bool>("HasTask", out var hastask);
            //if (hastask)
            //{
            //    var task = AITask.Load(tag["Task"]);
            //    this.CurrentTask = task;
            //}
            //tag.TryGetTag("Behavior", t =>
            //{
            //    var bhavtype = (string)t["TypeName"].Value;
            //    this.CurrentTaskBehavior = Activator.CreateInstance(Type.GetType(bhavtype)) as BehaviorPerformTask;
            //    this.CurrentTaskBehavior.Load(t);
            //});

            //if (hastask)
            //{
            //    var task = AITask.Load(tag["Task"]);
            //    //this.CurrentTask = task;
            //    tag.TryGetTag("Behavior", t =>
            //    {
            //        var bhavtype = (string)t["TypeName"].Value;
            //        var bhav = Activator.CreateInstance(Type.GetType(bhavtype)) as BehaviorPerformTask;
            //        bhav.Load(t);
            //    });
            //}

            var tagStack = tag["TaskStack"];
            var listStack = tagStack.Value as List<SaveTag>;
            foreach(var t in listStack)
            {
                var tasktag = t["Task"];
                var task = AITask.Load(tasktag);
                var bhavtag = t["Behavior"];
                var bhavname = (string)bhavtag["TypeName"].Value;
                var bhav = Activator.CreateInstance(Type.GetType(bhavname)) as BehaviorPerformTask;
                bhav.Task = task;
                bhav.Load(bhavtag);
                this.TaskStack.Push(bhav);
            }
            var tagQueue = tag["TaskQueue"];
            var listQueue = tagQueue.Value as List<SaveTag>;
            foreach (var t in listQueue)
            {
                var tasktag = t["Task"];
                var task = AITask.Load(tasktag);
                var bhavtag = t["Behavior"];
                var bhavname = (string)bhavtag["TypeName"].Value;
                var bhav = Activator.CreateInstance(Type.GetType(bhavname)) as BehaviorPerformTask;
                bhav.Task = task;
                bhav.Load(bhavtag);
                this.TaskQueue.Enqueue(bhav);
            }

            tag.TryLoad("Path", out this.Path);
            this.Jobs.TrySync(tag, "Jobs", keyTag => Def.TryGetDef<JobDef>((string)keyTag.Value));

            tag.TryGetTag("ItemPreferences", t => this.ItemPreferences.Load(t));
        }
        public void Read(IDataReader r)
        {
            //this.Jobs.Sync(r);
            r.ReadValuesWithInferredKeys(this.Jobs, v => v.Def);
            //this.Jobs.Write(r);
            this.ItemPreferences.Read(r); // sync to clients?
        }
        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(new SaveTag(SaveTag.Types.Vector3, "Leash", this.Leash));

            //tag.Add((this.CurrentTask != null).Save("HasTask"));
            //if (this.CurrentTask != null)
            //    tag.Add(this.CurrentTask.Save("Task"));
            //if (this.CurrentTaskBehavior != null)
            //{
            //    var bhavtag = this.CurrentTaskBehavior.Save("Behavior");
            //    bhavtag.Add(this.CurrentTaskBehavior.GetType().FullName.Save("TypeName"));
            //    tag.Add(bhavtag);
            //}
            //tag.Add((this.Current is not null).Save("HasTask"));
            //if (this.Current is not null)
            //{
            //    tag.Add(this.Current.Value.task.Save("Task"));
            //    var bhavtag = this.Current.Value.behavior.Save("Behavior");
            //    bhavtag.Add(this.Current.Value.behavior.GetType().FullName.Save("TypeName"));
            //    tag.Add(bhavtag);
            //}

            var tagStack = new SaveTag(SaveTag.Types.List, "TaskStack", SaveTag.Types.Compound);
            foreach (var bhav in this.TaskStack)
            {
                var tupleTag = new SaveTag(SaveTag.Types.Compound);
                tupleTag.Add(bhav.Task.Save("Task"));
                //tupleTag.Add(task.behavior.Save("Behavior"));
                var bhavtag = bhav.Save("Behavior");
                bhavtag.Add(bhav.GetType().FullName.Save("TypeName"));
                tupleTag.Add(bhavtag);
                tagStack.Add(tupleTag);
            }
            tag.Add(tagStack);
            var tagQueue = new SaveTag(SaveTag.Types.List, "TaskQueue", SaveTag.Types.Compound);
            foreach (var bhav in this.TaskQueue)
            {
                var tupleTag = new SaveTag(SaveTag.Types.Compound);
                tupleTag.Add(bhav.Task.Save("Task"));
                var bhavtag = bhav.Save("Behavior");
                bhavtag.Add(bhav.GetType().FullName.Save("TypeName"));
                tupleTag.Add(bhavtag);
                tagQueue.Add(tupleTag);
            }
            tag.Add(tagQueue);

            this.Path.TrySave(tag, "Path");
            this.Jobs.Save(tag, "Jobs", SaveTag.Types.String, key => key.Name);
            this.ItemPreferences.Save(tag, "ItemPreferences");
            return tag;
        }
        public void ToggleJob(JobDef job)
        {
            this.Jobs[job].Toggle();
        }
        public override string ToString()
        {
            return this.CurrentTask != null ? "Task: " + this.CurrentTask.ToString() : this.TaskString;
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
            //this.Jobs.Sync(w);
            w.WriteValues(this.Jobs);
            this.ItemPreferences.Write(w); // sync to clients?
        }
        public void Tick()
        {
            this.ItemPreferences.Tick();
        }

        public IEnumerable<BehaviorPerformTask> AllPlannedTasks => TaskStack.Concat(TaskQueue);
        //public BehaviorPerformTask Current => this.TaskStack.Count > 0 ? this.TaskStack.Peek() : (this.TaskQueue.Count > 0 ? this.TaskQueue.Peek() : null);
        public BehaviorPerformTask Behavior => this.TaskStack.Count > 0 ? this.TaskStack.Peek() : (this.TaskQueue.Count > 0 ? this.TaskQueue.Peek() : null);

        public TargetArgs MoveOrder => this.MoveOrders.Any() ? this.MoveOrders.Peek() : TargetArgs.Null;
        public List<GameObject> NearbyEntities { get; set; }
    }
}
