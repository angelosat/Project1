using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class OwnershipComponent : EntityComponent
    {
        public override string Name { get; } = "Ownership";
        public int OwnerRef { get; private set; } = -1;
        public Actor Owner;

        public new OwnershipComponent Initialize(GameObject owner = null)
        {
            this.OwnerRef = owner == null ? -1 : owner.RefId;
            return this;
        }
        public OwnershipComponent()
        {

        }
        
        OwnershipComponent(int owner)
        {
            this.OwnerRef = owner;
        }

        public override object Clone()
        {
            return new OwnershipComponent(this.OwnerRef);
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.OwnerRef);
        }
        public override void Read(IDataReader r)
        {
            this.OwnerRef = r.ReadInt32();
        }
        internal override void SaveExtra(SaveTag tag)
        {
            tag.Add(this.OwnerRef.Save("Owner"));
        }
        internal override void LoadExtra(SaveTag tag)
        {
            tag.TryGetTagValue<int>("Owner", v => this.OwnerRef = v);
        }

        public override void OnTooltipCreated(GameObject parent, UI.Control tooltip)
        {
            if (parent.Net == null)
                return;
            var owner = parent.World.GetEntity(this.OwnerRef);
            //tooltip.AddControlsBottomLeft(new UI.Label("Owner: " + (owner != null ? owner.Name : "<None>"), fill: Color.Lime));
            tooltip.AddControlsBottomLeft(UI.Label.ParseWrap("Owner: ", this.Owner));
        }

        static public bool Owns(GameObject owner, GameObject obj)
        {
            if (!obj.TryGetComponent("Ownership", out OwnershipComponent ownership))
                throw new Exception();
            return ownership.OwnerRef == owner.RefId;
        }

        internal override void GetManagementInterface(GameObject gameObject, Control box)
        {
            var setOwnerBtn = new Button("Set Owner")
            {
                LeftClickAction = () =>
                {
                    //150, 400
                    var listNpc = new ListBoxNoScroll<GameObject, Label>(o => new Label(o?.Name ?? "None", () => PacketPlayerSetItemOwner.Send(Net.Client.Instance, gameObject.RefId, -1)));
                    listNpc.AddItems(gameObject.Map.Town.GetAgents().Prepend(null));
                    listNpc.Toggle();
                }
            };
            var alllist = new List<GameObject>() { null };
            alllist.AddRange(gameObject.Map.Town.GetAgents());
            
            var comp = gameObject.GetComponent<OwnershipComponent>();
            var setownercombo = new ComboBoxNewNew<GameObject>(150, "Owner",
                A => A?.Name ?? "None",
                o => PacketPlayerSetItemOwner.Send(Net.Client.Instance, gameObject.RefId, o != null ? o.RefId : -1),
                () => comp.OwnerRef == -1 ? null : gameObject.World.GetEntity(comp.OwnerRef),
                () => alllist.Prepend(null));

            setownercombo.OnGameEventAction = a =>
            {
                switch (a.Type)
                {
                    case Message.Types.NpcsUpdated:
                        alllist.Clear();
                        alllist.Add(null);
                        alllist.AddRange(gameObject.Map.Town.GetAgents());
                        break;
                    default:
                        break;
                }
            };
            box.AddControls(setownercombo);
        }

        public void SetOwner(GameObject parent, int actorID)
        {
            this.OwnerRef = actorID;
            parent.Net.EventOccured((int)Message.Types.ItemOwnerChanged, parent.RefId);
        }
        static Control ActorList;
        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            info.AddInfo(new Label() { TextFunc = () => string.Format("Assigned to {0}", parent.Town.GetAgents().FirstOrDefault(a => a.GetPossesions().Contains(parent))?.Name ?? "none") });
        }
        readonly Button BtnOwner = new("Owner");
        internal override IEnumerable<Button> GetTabs()
        {
            var parent = this.Parent;
            //dimensions 200, 200, 
            if (ActorList is null)
                ActorList = new ListBoxNoScroll<Actor, Button>(a => new Button(a?.Name ?? "none", () => PacketPlayerSetItemOwner.Send(Net.Client.Instance, parent.RefId, a?.RefId ?? -1)))
                                                                   .AddItems(parent.Town.GetAgents().Prepend(null))
                                                                   .ToPanelLabeled("Select owner")
                                                                   .HideOnRightClick()
                                                                   .HideOnLeftClick()
                                                                   ;
            yield return BtnOwner.SetLeftClickAction(() => ActorList.SetLocation(UIManager.Mouse).Toggle()) as Button;
            //yield return ("Owner", () => ActorList.SetLocation(UIManager.Mouse).Toggle());
        }
    }
}
