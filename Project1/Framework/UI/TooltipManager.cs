using Microsoft.Xna.Framework.Graphics;
using Project1.Core.UI.Settings;
using Project1.Core.Base;
using Project1.Core.Input;
using Project1.Core.Net;
using System;
using System.Linq;
using Project1.Core;
using Project1.Core.Simulation;

namespace Project1.Framework.UI
{
    public class TooltipManager
    {
        public static bool MouseTooltips = true;
        public static float DelayInterval = Ticks.PerSecond / 4;
        float DelayValue;
        public static int Width = 300;
        protected static TooltipManager _Instance;
        public static TooltipManager Instance => _Instance ??= new TooltipManager();

        Tooltip _tooltip;
        public Tooltip Tooltip
        {
            get => this._tooltip;
            set
            {
                this._tooltip?.Dispose();
                this._tooltip = value;
            }
        }

        ITooltippable Object;


        public void Update()
        {
            if (this.Object is null)
                return;

            this.DelayValue -= 1;

            if (this.DelayValue <= 0)
            {
                this.DelayValue = DelayInterval;
                if (this.Tooltip is null)
                    this.Build();
                else
                    this.Tooltip.Update();
            }
            this.Tooltip?.Update();
        }

        void Build()
        {
            this.Tooltip = new(this.Object);
            this.Tooltip.AutoSize = true;
            this.Object.GetTooltipInfo(this.Tooltip);
            foreach (var comp in Game1.Instance.GameComponents)
                comp.OnTooltipCreated(this.Object, this.Tooltip);

            if (this.Tooltip.Controls.Count > 0)
            {
                this.Tooltip.Update();
                this.Tooltip.SetMousethrough(true, true);
            }
            else
                this.Tooltip = null;
        }

        void Object_TooltipChanged(object sender, EventArgs e)
        {
            this.Build();
        }

        TooltipManager()
        {
            Controller.MouseoverObjectChanged += new EventHandler<MouseoverEventArgs>(this.Controller_MouseoverObjectChanged);

            if (!bool.TryParse(InterfaceSettings.XMouseTooltip.Value, out MouseTooltips))
                MouseTooltips = true;
        }

        void Controller_MouseoverObjectChanged(object sender, MouseoverEventArgs e)
        {
            this.Reset();
            this.Object = e.ObjectNext as ITooltippable;
        }

        private void Reset()
        {
            this.Tooltip = null;
            this.DelayValue = DelayInterval;
        }

        public void Draw(SpriteBatch sb)
        {
            this.Tooltip?.Draw(sb);
        }

        internal static void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                default:
                    Instance.Tooltip?.OnGameEvent(e);
                    break;
            }
        }

        internal static void Bind(NetEndpoint net)
        {
            net.Events.ListenTo<CellsInvalidatedEvent>(HandleBlocksChanged);
        }
        static void HandleBlocksChanged(CellsInvalidatedEvent e)
        {
            var map = e.Map;
            var cells = e.Positions;
            if(Engine.Map == map)
            if (Instance.Object is TargetArgs target && target.Type == TargetType.Position && cells.Contains((IntVec3)target.Global))
                Instance.Reset();
        }
    }
}
