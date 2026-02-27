using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Graphics;
using Project1.Core.Input;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.UI;
using Project1.Core.UI.Hud;
using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

namespace Project1.Core.Towns
{
    public abstract class TownComponent : Inspectable
    {
        public Town Town;
        public MapBase Map => this.Town.Map;
        public NetEndpoint Net => this.Map.Net;
        public override string LabelReadable => this.Name;
        const float UpdateFrequency = 1; // per second
        float UpdateTimerMax = Ticks.PerSecond / UpdateFrequency;
        float UpdateTimer;
        protected TownComponent()
        {

        }
        public TownComponent(Town town)
        {
            this.Town = town;
        }

        public abstract string Name { get; }
        public virtual void Update()
        {
            if (this.UpdateTimer > 0)
            {
                this.UpdateTimer--;
                return;
            }
            this.UpdateTimer = UpdateTimerMax;
            this.OnUpdate();
        }

        public virtual void OnUpdate() { }

        internal virtual IEnumerable<Tuple<Func<string>, Action>> OnQuickMenuCreated() { yield break; }
        internal virtual void OnContextMenuCreated(IContextable obj, ContextArgs a) { }
        internal virtual void OnContextActionBarCreated(ContextActionBar.ContextActionBarArgs a) { }
        public SaveTag Save()
        {
            var tag = new SaveTag(SaveTag.Types.Compound, this.Name);
            this.AddSaveData(tag);
            return tag;
        }

        protected virtual void AddSaveData(SaveTag tag) { }

        public virtual void Load(SaveTag tag) { }
        public virtual void Write(IDataWriter w) { }
        public virtual void Read(IDataReader r) { }


        public virtual IContextable QueryPosition(Vector3 global) { return null; }
        public virtual ISelectable QuerySelectable(CellSelection selected) { return null; }

        public virtual void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera cam) { }
        public virtual void DrawUI(SpriteBatch sb, MapBase map, Camera cam) { }

        internal virtual void UpdateOrderButtons() { }

        public virtual void Tick()
        {
        }
        internal virtual IEnumerable<Button> GetTabs(ISelectable selected) { yield break; }
        //internal virtual void OnTargetSelected(IUISelection info, ISelectable target) { }
        //internal virtual void OnTargetSelected(SelectionManager info, ISelectable target) { }
        //internal virtual void OnTargetSelected(IUISelection info, TargetArgs targetArgs)
        //{
        //}
        internal virtual void OnTooltipCreated(Control tooltip, TargetArgs targetArgs)
        {
        }
        internal virtual void OnCitizenAdded(int actorID) { }
        internal virtual void OnCitizenRemoved(int actorID) { }
        protected PlayerData Player { get { return this.Town.Map.Net.GetPlayer(); } }
        internal virtual void ResolveReferences() { }
        internal virtual void OnBlocksChanged(IEnumerable<IntVec3> positions) { }
        internal virtual void OnHudCreated(Hud hud) { }
        internal virtual void OnCameraRotated(Camera camera) { }
        internal virtual void GetQuickButtons(Action<string, Type> register, IntVec3 global) { }
    }
}