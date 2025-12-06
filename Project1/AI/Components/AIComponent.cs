using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public class AIComponent : EntityComponent
    {
        public override string Name { get; } = "AI";
        public Guid Guid = Guid.NewGuid();
        public static Dictionary<Guid, GameObject> Registry = new();
        public static Guid GetGuid(GameObject agent)
        {
            return agent.GetComponent<AIComponent>().Guid;
        }
        public static GameObject GetAgent(Guid guid)
        {
            return Registry[guid];
        }
        public void Sync(int seed)
        {
            this.State.InSync = true;
        }

        public static void Invalidate(GameObject obj)
        {
            // TODO: signal each npc that remembers obj, that obj's state has changed, so that they evaluate it again next time they see it
            throw new NotImplementedException();
        }
        Behavior Root;
        readonly Knowledge Knowledge;
        public AIState State;
        bool Enabled = true;
        public AIComponent()
        {
            this.Knowledge = new Knowledge();
            this.Root = null;
        }
        public override void Randomize(GameObject parent, RandomThreaded random)
        {
            this.State.Generate(parent, random);
        }

        internal T FindBehavior<T>() where T : Behavior
        {
            return (this.Root as BehaviorComposite).Find(typeof(T)) as T;
        }

        public void Enable()
        {
            this.Enabled = true;
        }
        public void Disable()
        {
            this.Enabled = false;
        }

        public override void MakeChildOf(GameObject parent)
        {
            this.State = new AIState(parent as Actor) { Knowledge = this.Knowledge };
        }

        public AIComponent Initialize(Behavior root)
        {
            this.Root = root;
            return this;
        }

        public Behavior GetCurrentBehavior()
        {
            return (this.Root as BehaviorQueue).Current;
        }

        public override void Tick()
        {
            var parent = this.Parent;
            var net = parent.Net;
            if (net is Client) // do i want to run some deterministic behaviors locally too? UPDATE: NO
                return;

            if (this.Enabled)
                this.Root.Execute(parent as Actor, this.State);
        }

        public override void OnSpawn()
        {
            this.State.Leash = this.Parent.Global;
            //this._unListen = this.Parent.Map.World.Events.ListenTo<BlocksChangedEvent>(this.HandleBlocksChange);
            this.Parent.Map.World.Events.ListenTo<BlocksChangedEvent>(this.HandleBlocksChange);
        }
        public override void OnObjectSynced(GameObject parent)
        {
            this.Parent.Map?.World.Events.ListenTo<BlocksChangedEvent>(this.HandleBlocksChange);
        }
        //Action _unListen;
        //public override void OnDespawn()
        //{
        //    this.Parent.Map.World.Events.ListenTo<BlocksChangedEvent>(this.HandleBlocksChange);
        //}
        //internal override void OnGameEvent(GameObject gameObject, GameEvent e)
        //{
        //    if (e.Type == Message.Types.BlocksChanged)
        //    {
        //        if (!this.State.Path?.IsValid(gameObject as Actor) ?? false)
        //        {
        //            this.State.Path = null;
        //        }
        //    }
        //}

        void HandleBlocksChange(BlocksChangedEvent e)
        {
            if (!this.State.Path?.IsValid(this.Parent as Actor) ?? false)
            {
                this.State.Path = null;
            }
        }

        public override object Clone()
        {
            AIComponent ai = new AIComponent().Initialize(
                this.Root.Clone() as Behavior);
            return ai;
        }

        internal override List<SaveTag> Save()
        {
            var save = new List<SaveTag>
            {
                new SaveTag(SaveTag.Types.ByteArray, "Guid", this.Guid.ToByteArray()),
                this.State.Save("State"),
                this.Root.Save("Root")
            };
            return save;
        }
        internal override void LoadExtra(SaveTag save)
        {
            save.TryGetTagValue<byte[]>("Guid", v => this.Guid = new Guid(v));
            this.State.Load(save["State"]);
            this.Root.Load(save["Root"]);
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.Guid.ToByteArray());
            this.State.Write(w); // i dont want to sync the state for the time being
            this.Root.Write(w);
        }
        public override void Read(IDataReader r)
        {
            this.Guid = new Guid(r.ReadBytes(16));
            this.State.Read(r);// i dont want to sync the state for the time being
            this.Root.Read(r);
        }

        internal override void GetInterface(GameObject gameObject, Control box)
        {
            base.GetInterface(gameObject, box);
            box.AddControls(new Interface());
        }
        internal override void MapLoaded(GameObject parent)
        {
            this.State.MapLoaded(parent as Actor);
            this.Root.MapLoaded(parent as Actor);
        }
        public override void OnObjectLoaded(GameObject parent)
        {
            this.State.ObjectLoaded(parent);
            this.Root.ObjectLoaded(parent);
        }
        class Interface : GroupBox
        {
            readonly Label Label;
            public Interface()
            {
                this.Label = new Label("Behavior: ") { Width = 300 };
                this.AddControls(this.Label);
            }
        }

        internal void MoveOrder(TargetArgs target, bool enqueue)
        {
            this.State.AddMoveOrder(target, enqueue);
        }
        public override void DrawAfter(MySpriteBatch sb, Camera cam, GameObject parent)
        {
            var serverentity = parent;
            var state = AIState.GetState(serverentity);
            if (state is null)
            {
                return; // we are in client
            }

            var path = state.Path;
            if (!SelectionManager.IsSelected(parent))
            {
                return;
            }

            var first = true;
            if (path is not null)
            {
                cam.DrawBlockMouseover(sb, parent.Map, path.Current, Color.Lime);

                if (path.Stack != null)
                {
                    foreach (var global in path.Stack)
                    {
                        cam.DrawBlockMouseover(sb, parent.Map, global, first ? Color.Red : Color.Blue);
                        first = false;
                    }
                }
            }
            foreach (var target in state.MoveOrders)
            {
                cam.DrawBlockMouseover(sb, parent.Map, target.Global.Above(), Color.Yellow);
            }
        }
        readonly Label CachedGuiLabelCurrentTask = new();
        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            info.AddInfo(this.CachedGuiLabelCurrentTask.SetTextFunc(()=> $"Current Task: {this.State.TaskString}"));
        }
        internal override void ResolveReferences()
        {
            this.State.ResolveReferences();
        }
    }
}
