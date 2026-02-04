using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using Project1.Framework.Input;
using Project1.Framework.Input.Tools;
using Project1.Framework.Net;
using Project1.Framework.Entities;
using Project1.Framework.Base;

namespace Start_a_Town_
{
    class ToolCommandNpc : ToolManagement
    {
        private readonly List<GameObject> Actors;
        public ToolCommandNpc()
        {

        }
        public ToolCommandNpc(GameObject npc)
            : this(new List<GameObject>() { npc })
        {
        }

        public ToolCommandNpc(List<GameObject> actors)
        {
            this.Actors = actors.ToList();
        }
        public override Icon GetIcon()
        {
            return Icon.Replace;
        }
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Target != null)
                if (this.Target.Type == TargetType.Position)
                    PacketCommandNpc.Send(Client.Instance, this.Actors.Select(i => i.RefId).ToList(), this.Target, IsEnqueing);
            return base.MouseLeftPressed(e);
        }
        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return Messages.Remove;
        }

        public bool IsEnqueing
        {
            get
            {
                return InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey);
            }
        }
    }
}
