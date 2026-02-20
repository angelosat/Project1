using Project1.Core.AI.Packets;
using Project1.Core.Components;
using Project1.Core.Input;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.UI;
using Project1.Core.UI.Hud;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Entities.Actors
{
    class NpcComponent : EntityComp
    {
        public new class Spec : Spec<NpcComponent> { }
        public override EntityCompDef CompDef => EntityCompDefOf.Npc;
        HashSet<int> Possesions = [];
        public string FullName => this.FirstName + (this.LastName.IsNullEmptyOrWhiteSpace() ? "" : string.Format(" {0}", this.LastName));

        static public List<GameObject> NpcDirectory = new List<GameObject>();
        
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
        internal override void Resolve()
        {
            this.GenerateFullName();
        }
        private void GenerateFullName()
        {
            this.FirstName = GetRandomName();
            this.LastName = GetRandomName();
        }

        static public HashSet<int> GetPossessions(GameObject actor)
        {
            return actor.GetComponent<NpcComponent>().Possesions;
        }
        static public void AddPossession(GameObject actor, GameObject item)
        {
            var poss = GetPossessions(actor);
            if (poss.Contains(item.RefId))
                throw new Exception();
            poss.Add(item.RefId);
            item.SetOwner(actor);
        }
        static public void RemovePossession(GameObject actor, GameObject item)
        {
            var poss = GetPossessions(actor);
            poss.Remove(item.RefId);
            item.SetOwner(null);
        }
        static public bool HasPossession(GameObject actor, GameObject item)
        {
            var poss = GetPossessions(actor);
            return poss.Contains(item.RefId);
        }


        static readonly List<string> NameParts = ["an", "ro", "sta", "da", "be", "an", "stath", "jo", "cam", "gro", "ma", "ob", "the", "pa", "er", "ble", "arn", "old", "ohn", "ni", "ick", "ber", "tie", "dim", "ste", "ve"];

        static Random Random = new();
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
        public override void Write(IDataWriter w)
        {
            base.Write(w);
            w.Write(this.Possesions.ToList());
            w.Write(this.FirstName);
            w.Write(this.LastName);
        }

        public override void Read(IDataReader r)
        {
            base.Read(r);
            this.Possesions = new HashSet<int>(r.ReadListInt32());
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
        public override void OnDespawnExtra(MapBase oldMap)
        {
            NpcDirectory.Remove(this.Owner);
        }
        internal override void GetQuickButtons(SelectionManager info, GameObject parent)
        {
            if (parent.IsPlayerControlled)
                return;
            info.AddButton(IconOrder, Command, parent);
            var actor = parent as Actor;
            info.AddButton(IconControl, Control, parent, true);
        }
        static IconButton IconOrder = new('☞') { HoverText = "Order Move" };
        static IconButton IconControl = new(Icon.ArrowUp) { HoverText = "Take Control" };
        public override string Name { get; } = "Npc";
        static void Command(List<ISelectable> actors)
        {
            ToolManager.SetTool(new ToolCommandNpc([.. actors.Cast<Actor>()]));
        }
        static void Control(List<ISelectable> actors)
        {
            var actor = actors.OfType<Actor>().First();
            if (actor.IsTownMember)
                PacketControlActor.Send(Client.Instance, Client.Instance.GetPlayer().ID, actor.RefId);
        }
        internal override void OnGameEvent(GameObject parent, GameEvent e)
        {
            switch((Message.Types)e.Type)
            {
                case Message.Types.PlayerControlNpc:
                    if ((parent.Net is Client))
                    {
                        if (SelectionManager.GetSelectedEntities().Contains(parent))
                        {
                            if (e.Parameters[1] as GameObject == parent)
                                SelectionManager.RemoveOrderButton(IconControl);
                            else if (e.Parameters[2] as GameObject == parent)
                                SelectionManager.AddButton(IconControl);
                        }
                    }
                    break;

                case Message.Types.ObjectDisposed:
                    var item = e.Parameters[0] as GameObject;
                    RemovePossession(parent, item);
                    break;

                case Message.Types.ItemOwnerChanged:
                    item = parent.World.GetEntity((int)e.Parameters[0]) as GameObject;
                    var currentOwner = item.GetOwner();
                    if (currentOwner == parent.RefId)
                        Possesions.Add(item.RefId);
                    else
                        Possesions.Remove(item.RefId);
                    break;

                default:
                    break;
            }
        }
    }
}