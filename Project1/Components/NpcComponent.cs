using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using Start_a_Town_.Net;
using System.IO;

namespace Start_a_Town_
{
    class NpcComponent : EntityComponent
    {
        static int p;
        internal static void Init()
        {
            p = Network.RegisterPacketHandler(ReceiveCitizenshipToggle);
        }

        public string FullName => this.FirstName + (this.LastName.IsNullEmptyOrWhiteSpace() ? "" : string.Format(" {0}", this.LastName));

        static public List<GameObject> NpcDirectory = new List<GameObject>();
        public override void OnObjectCreated(GameObject parent)
        {
            this.GenerateFullName();
        }

        private void GenerateFullName()
        {
            this.FirstName = GetRandomName();
            this.LastName = GetRandomName();
        }
        const int NameCharLimit = 16;
        string _FirstName = "", _LastName = "";
        public string FirstName
        {
            get { return this._FirstName; }
            set {
                var length = Math.Min(NameCharLimit, value.Length);
                var name = value.Substring(0, length);
                if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                    this._FirstName = "";
                else
                {
                    this._FirstName = char.ToUpper(name[0]) + name.Substring(1, length - 1).ToLower();
                }
            }
        }
        public string LastName
        {
            get { return this._LastName; }
            set
            {
                var length = Math.Min(NameCharLimit, value.Length);
                var name = value.Substring(0, length);
                if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                    this._LastName = "";
                else
                {
                    this._LastName = char.ToUpper(name[0]) + name.Substring(1, length-1).ToLower();
                }
            }
        }

        public NpcComponent()
        {

        }
        #region possesions
        HashSet<int> Possesions = new();

        static public HashSet<int> GetPossesions(GameObject actor)
        {
            return actor.GetComponent<NpcComponent>().Possesions;
        }
        static public void AddPossesion(GameObject actor, GameObject item)
        {
            var poss = GetPossesions(actor);
            if (poss.Contains(item.RefID))
                throw new Exception();
            poss.Add(item.RefID);
            item.SetOwner(actor);
        }
        static public void RemovePossesion(GameObject actor, GameObject item)
        {
            var poss = GetPossesions(actor);
            poss.Remove(item.RefID);
            item.SetOwner(null);
        }
        static public bool HasPossesion(GameObject actor, GameObject item)
        {
            var poss = GetPossesions(actor);
            return poss.Contains(item.RefID);
        }
        #endregion

        static List<string> NameParts = new List<string>() { "an", "ro", "sta", "da", "be", "an", "stath", "jo", "cam", "gro", "ma", "ob", "the", "pa", "er", "ble", "arn", "old", "ohn", "ni", "ick", "ber", "tie", "dim", "ste", "ve" };

        static Random Random = new Random();
        static public string GetRandomFullName()
        {
            var r = Random;   

            string first = "";
            for (int i = 0; i < r.Next(1) + 2; i++)
                first += NameParts[r.Next(NameParts.Count)];

            string last = "";
            for (int i = 0; i < r.Next(2) + 2; i++)
                last += NameParts[r.Next(NameParts.Count)];

            return char.ToUpper(first[0]) + first.Substring(1) + " " + char.ToUpper(last[0]) + last.Substring(1);
        }
        static public string GetRandomName()
        {
            var r = Random;

            string name = "";
            for (int i = 0; i < r.Next(2) + 2; i++)
                name += NameParts[r.Next(NameParts.Count)];

            return char.ToUpper(name[0]) + name.Substring(1);
        }
        public override object Clone()
        {
            NpcComponent phys = new NpcComponent();
            
            return phys;
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            base.Write(w);
            w.Write(this.Possesions.ToList());
            w.Write(this.FirstName);
            w.Write(this.LastName);
        }

        public override void Read(System.IO.BinaryReader r)
        {
            base.Read(r);
            this.Possesions = new HashSet<int>(r.ReadListInt());
            this.FirstName = r.ReadString();
            this.LastName = r.ReadString();
        }
        internal override void SaveExtra(SaveTag tag)
        {
            base.SaveExtra(tag);
            tag.Add(this.Possesions.Save("Possesions"));
            tag.Add(this.FirstName.Save("FirstName"));
            tag.Add(this.LastName.Save("LastName"));
        }
        internal override void LoadExtra(SaveTag tag)
        {
            base.LoadExtra(tag);
            tag.TryGetTag("Possesions", t => this.Possesions = new HashSet<int>(new List<int>().Load(t)));
            tag.TryGetTagValue<string>("FirstName", v => this.FirstName = v);
            tag.TryGetTagValue<string>("LastName", v => this.LastName = v);
        }
       
        public override void OnDespawn()
        {
            NpcDirectory.Remove(this.Parent);
        }

        
        internal override void GetQuickButtons(SelectionManager info, GameObject parent)
        {
            if (parent.IsPlayerControlled)
                return;
            info.AddButton(IconOrder, Command, parent);
            var actor = parent as Actor;
            info.AddButton(IconControl, Control, parent, true);
            info.AddButton(IconToggleCitizen, ToggleCitizenship, parent, true);
        }
        static IconButton IconOrder = new IconButton('☞') { HoverText = "Order Move" };
        static IconButton IconControl = new IconButton(Icon.ArrowUp) { HoverText = "Take Control" };
        static IconButton IconToggleCitizen = new IconButton() { HoverText = "Toggle citizenship" };

        public override string Name { get; } = "Npc";

        static void Command(List<TargetArgs> actors)
        {
            ToolManager.SetTool(new ToolCommandNpc(actors.Select(t=>t.Object).ToList()));
        }
        static void Control(List<TargetArgs> actors)
        {
            var actor = actors.First().Object as Actor;
            if (actor.IsCitizen)
                PacketControlNpc.Send(Net.Client.Instance, Net.Client.Instance.GetPlayer().ID, actor.RefID);
        }
        private void ToggleCitizenship(List<TargetArgs> actors)
        {
            var actor = actors.First();
            var w = Client.Instance.GetOutgoingStream();
            w.Write(p);
            w.Write(Net.Client.Instance.GetPlayer().ID);
            w.Write(actor.Object.RefID);
        }
        private static void ReceiveCitizenshipToggle(INetPeer net, BinaryReader r)
        {
            var plID = r.ReadInt32();
            var actorID = r.ReadInt32();
            var actor = net.GetNetworkObject(actorID);
            actor.Town.ToggleAgent(actor);
            if(net is Server)
            {
                var w = net.GetOutgoingStream();
                w.Write(p);
                w.Write(Net.Client.Instance.GetPlayer().ID);
                w.Write(actor.RefID);
            }
        }
        internal override void OnGameEvent(GameObject parent, GameEvent e)
        {
            switch(e.Type)
            {
                case Message.Types.PlayerControlNpc:
                    if (parent.Net is Net.Client)
                    {
                        if (SelectionManager.GetSelectedEntities().Contains(parent))
                        {
                            if (e.Parameters[1] as GameObject == parent)
                                SelectionManager.RemoveButton(IconControl);
                            else if (e.Parameters[2] as GameObject == parent)
                                SelectionManager.AddButton(IconControl);
                        }
                    }
                    break;

                case Message.Types.ObjectDisposed:
                    var item = e.Parameters[0] as GameObject;
                    RemovePossesion(parent, item);
                    break;

                case Message.Types.ItemOwnerChanged:
                    item = parent.Net.GetNetworkObject((int)e.Parameters[0]) as GameObject;
                    var currentOwner = item.GetOwner();
                    if (currentOwner == parent.RefID)
                        Possesions.Add(item.RefID);
                    else
                        Possesions.Remove(item.RefID);
                    break;

                default:
                    break;
            }
        }
    }
}
