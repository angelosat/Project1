using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    public partial class CraftOrder : Inspectable, ILoadReferencable<CraftOrder>, ILoadReferencable, ISerializable, IListable
    {
        enum CraftMode { XTimes, UntilX, Forever }

        static readonly Dictionary<int, CraftOrder> References = new();
        static ListCollapsibleNew DetailsUIReagents;
        static Control DetailsUIContainer;
        static Window DetailsWindow;

        public CraftOrderFinishMode FinishMode = CraftOrderFinishMode.AllModes.First();
        public int Quantity = 1;
        public int ID;
        public Reaction Reaction;
        public IntVec3 Workstation;
        public MapBase Map;
        public Entity UnfinishedItem;
        public bool HaulOnFinish;
        public bool Enabled;
        public Dictionary<string, IngredientRestrictions> Restrictions = new();
        public Stockpile Input, Output;
        int _inputID = -1, _outputID = -1, _unfinishedItemRefID = -1;

        readonly Dictionary<string, HashSet<int>> ReagentRestrictions = new();
        CraftOrderDetailsGui DetailsGui;
        CraftMode Mode = CraftMode.XTimes;

        public string Name => this.Reaction.Name;
        public override string Label => this.Reaction.Label;
        public static CraftOrder GetOrder(int id)
        {
            return References[id];
        }

        public bool IsActive
        {
            get
            {
                if (!this.GetWorkstation().Orders.Contains(this))
                    return false;

                return this.FinishMode.IsActive(this);
            }
        }

        public CraftOrder()
        {

        }
        public CraftOrder(int id, Reaction reaction, MapBase map, IntVec3 workstation)
           : this(reaction)
        {
            this.Map = map;
            this.Workstation = workstation;
            this.ID = id;
        }
        public CraftOrder(Reaction reaction)
        {
            this.Reaction = reaction;
            foreach (var r in this.Reaction.Reagents)
                this.Restrictions.Add(r.Name, new IngredientRestrictions(r.Ingredient.DefaultRestrictions));
        }

        internal bool IsRestricted(string name, ItemDef i)
        {
            return this.Restrictions[name].IsRestricted(i);
        }
        internal bool IsRestricted(string name, MaterialDef i)
        {
            return this.Restrictions[name].IsRestricted(i);
        }
        internal bool IsRestricted(MaterialDef i)
        {
            return this.Restrictions.Values.Any(r => r.IsRestricted(i));
        }
        public bool IsAllowed(MaterialDef mat)
        {
            if (this.IsRestricted(mat))
            {
                return false;
            }

            foreach (var r in this.Reaction.Reagents)
            {
                var items = r.Ingredient.GetAllValidItemDefs();
                foreach (var i in items)
                {
                    if (i.GetValidMaterials().Contains(mat))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void ToggleReagentRestriction(string reagentName, int itemID)
        {
            if (!this.ReagentRestrictions.TryGetValue(reagentName, out HashSet<int> list))
            {
                list = new HashSet<int>();
                this.ReagentRestrictions.Add(reagentName, list);
            }
            if (!list.Remove(itemID))
            {
                list.Add(itemID);
            }
        }
        public void ToggleReagentRestriction(string reagentName, int itemID, bool add)
        {
            if (!this.ReagentRestrictions.TryGetValue(reagentName, out HashSet<int> list))
            {
                if (!add)
                {
                    return;
                }

                list = new HashSet<int>();
                this.ReagentRestrictions.Add(reagentName, list);
            }
            if (!add)
            {
                list.Remove(itemID);
            }
            else
            {
                list.Add(itemID);
            }
        }

        internal void ResolveReferences(MapBase map)
        {
            this.Map = map;
            this.Input = this._inputID > -1 ? map.Town.ZoneManager.GetZone<Stockpile>(this._inputID) : null;
            this.Output = this._outputID > -1 ? map.Town.ZoneManager.GetZone<Stockpile>(this._outputID) : null;
            this.UnfinishedItem = this._unfinishedItemRefID > -1 ? map.Net.GetNetworkObject<Entity>(this._unfinishedItemRefID) : null;
        }

        public bool IsItemAllowed(string reagentName, Entity item)
        {
            return
                !this.Restrictions[reagentName].IsRestricted(item) &&
                this.Reaction.Reagents.Find(r => r.Name == reagentName).Filter(item);
        }

        public void Complete(GameObject agent)
        {
            this.FinishMode.OnComplete(this);
        }

        public BlockEntityCompWorkstation GetWorkstation()
        {
            return this.Map.GetBlockEntity(this.Workstation).GetComp<BlockEntityCompWorkstation>();
        }

        internal void ToggleReagentRestrictions(string reagent, ItemDef[] defs, MaterialDef[] mats, MaterialTypeDef[] matTypes)
        {
            this.Restrictions[reagent].ToggleRestrictions(defs, mats, matTypes);
        }

        CraftOrder(SaveTag tag) : this(null, tag)
        {

        }
        CraftOrder(MapBase map, SaveTag tag)
        {
            //tag.TryGetTagValue<int>("ReactionID", p => this.Reaction = Reaction.Dictionary[p]);
            this.Reaction = tag.LoadDef<Reaction>("Reaction");
            tag.TryGetTagValue<int>("Mode", p => this.Mode = (CraftMode)p);
            tag.TryGetTagValue<int>("FinishMode", p => this.FinishMode = CraftOrderFinishMode.GetMode(p));
            tag.TryGetTagValue<int>("Quantity", p => this.Quantity = p);
            tag.TryGetTagValue<IntVec3>("Bench", p => this.Workstation = p);
            this.ReagentRestrictions.Clear();
            tag.TryGetTagValue<List<SaveTag>>("Restrictions", restrictionsTag =>
            {

                foreach (var rTag in restrictionsTag)
                {
                    var name = rTag.GetValue<string>("Reagent");
                    rTag.TryGetTagValue<List<SaveTag>>("Items", list =>
                        {
                            foreach (var itemTag in list)
                            {
                                var item = (int)itemTag.Value;
                                this.ToggleReagentRestriction(name, item);
                            }
                        });
                }

            });
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.ID);
            //w.Write(this.Reaction.ID);
            this.Reaction.Write(w);
            w.Write((int)this.Mode);
            w.Write((int)this.FinishMode.Mode);
            w.Write(this.Input?.ID ?? -1);
            w.Write(this.Output?.ID ?? -1);
            w.Write(this.UnfinishedItem?.RefID ?? -1);
            w.Write(this.Quantity);
            w.Write(this.Workstation);
            w.Write(this.ReagentRestrictions.Count);
            w.Write(this.HaulOnFinish);
            foreach (var r in this.ReagentRestrictions)
            {
                w.Write(r.Key);
                w.Write(r.Value.Count);
                foreach (var i in r.Value)
                {
                    w.Write(i);
                }
            }

            w.Write(this.Restrictions.Keys.ToArray());
            this.Restrictions.Values.Write(w);
        }
        public void Read(MapBase map, BinaryReader r)
        {
            this.Read(r);
            this.Map = map;
        }
        public ISerializable Read(BinaryReader r)
        {
            this.ID = r.ReadInt32();
            //this.Reaction = Reaction.Dictionary[r.ReadInt32()];
            this.Reaction = r.ReadDef<Reaction>();
            this.Mode = (CraftMode)r.ReadInt32();
            this.FinishMode = CraftOrderFinishMode.GetMode(r.ReadInt32());
            //this.Input = r.ReadInt32() is int input && input == -1 ? null : this.Map.Town.ZoneManager.GetZone<Stockpile>(input);
            //this.Output = r.ReadInt32() is int output && output == -1 ? null : this.Map.Town.ZoneManager.GetZone<Stockpile>(output);
            this._inputID = r.ReadInt32();
            this._outputID = r.ReadInt32();
            this._unfinishedItemRefID = r.ReadInt32();
            this.Quantity = r.ReadInt32();
            this.Workstation = r.ReadIntVec3();
            var rCount = r.ReadInt32();
            this.ReagentRestrictions.Clear();
            this.HaulOnFinish = r.ReadBoolean();
            for (int i = 0; i < rCount; i++)
            {
                var rName = r.ReadString();
                var iCount = r.ReadInt32();
                for (int j = 0; j < iCount; j++)
                {
                    var id = r.ReadInt32();
                    this.ToggleReagentRestriction(rName, id);
                }
            }

            var restrictionsKeys = r.ReadStringArray();
            var restrictionValues = new List<IngredientRestrictions>();
            restrictionValues.ReadMutable(r);
            this.Restrictions = restrictionsKeys.ToDictionary(restrictionValues);

            return this;
        }
        public CraftOrder(MapBase map, BinaryReader r)
        {
            this.Read(map, r);
        }

        internal int GetIndex()
        {
            return this.Map.Town.CraftingManager.GetOrdersNew(this.Workstation).FindIndex(c => c == this);
        }

        internal bool IsCompletable(BlockEntity buildSite)
        {
            if (this.Reaction.Fuel == 0)
            {
                return true;
            }

            return buildSite.GetComp<BlockEntityCompRefuelable>()?.Fuel.Value > this.Reaction.Fuel;
        }
        internal bool IsCompletable()
        {
            return this.IsCompletable(this.Map.GetBlockEntity(this.Workstation));
        }

        internal static CraftOrder Load(SaveTag t)
        {
            var order = new CraftOrder(t);
            var id = order.ID;
            if (References.TryGetValue(order.ID, out var existing))
            {
                return existing;
            }

            References[id] = order;
            return order;
        }

        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            //tag.Add(new SaveTag(SaveTag.Types.Int, "ReactionID", this.Reaction.ID));
            this.Reaction.Save(tag, "Reaction");
            tag.Add(new SaveTag(SaveTag.Types.Int, "Mode", (int)this.Mode));
            tag.Add(new SaveTag(SaveTag.Types.Int, "FinishMode", (int)this.FinishMode.Mode));
            tag.Add(this.ID.Save("ID"));
            this.Enabled.Save(tag, "Enabled");
            tag.Add(new SaveTag(SaveTag.Types.Int, "Quantity", this.Quantity));
            tag.Add(this.Workstation.Save("Bench"));

            tag.Add("Input", this.Input?.ID ?? -1);
            tag.Add("Output", this.Output?.ID ?? -1);

            tag.Add("UnfinishedItem", this.UnfinishedItem?.RefID ?? -1);

            var tagRestr = new SaveTag(SaveTag.Types.Compound, "RestrictionsNew");
            tagRestr.Add(this.Restrictions.Keys.Save("Keys"));
            tagRestr.Add(this.Restrictions.Values.SaveNewBEST("Values"));
            tag.Add(tagRestr);

            var tagRestrictions = new SaveTag(SaveTag.Types.List, "Restrictions", SaveTag.Types.Compound);
            foreach (var rest in this.ReagentRestrictions)
            {
                var tagReagent = new SaveTag(SaveTag.Types.Compound, "");
                tagReagent.Add(new SaveTag(SaveTag.Types.String, "Reagent", rest.Key));
                var tagItems = new SaveTag(SaveTag.Types.List, "Items", SaveTag.Types.Int);
                foreach (var item in rest.Value)
                {
                    tagItems.Add(new SaveTag(SaveTag.Types.Int, "", item));
                }

                tagReagent.Add(tagItems);
                tagRestrictions.Add(tagReagent);
            }
            tag.Add(tagRestrictions);
            return tag;
        }

        public string GetUniqueLoadID()
        {
            return $"CraftOrder{this.ID}";
        }

        ISaveable ISaveable.Load(SaveTag tag)
        {
            //tag.TryGetTagValue<int>("ReactionID", p => this.Reaction = Reaction.Dictionary[p]);
            this.Reaction = tag.LoadDef<Reaction>("Reaction");
            tag.TryGetTagValue<int>("Mode", p => this.Mode = (CraftMode)p);
            tag.TryGetTagValue<int>("FinishMode", p => this.FinishMode = CraftOrderFinishMode.GetMode(p));
            tag.TryGetTagValue<int>("ID", out this.ID);
            tag.TryGetTagValue<int>("Quantity", p => this.Quantity = p);
            this.Workstation = tag.LoadIntVec3("Bench");
            tag.TryGetTagValue("Enabled", out this.Enabled);
            tag.TryGetTag("RestrictionsNew", t =>
                {
                    var keys = t.LoadStringList("Keys");
                    var values = new List<IngredientRestrictions>();
                    values.TryLoadMutable(t, "Values");
                    this.Restrictions = keys.ToDictionary(values);
                });

            //tag.TryGetTagValue<int>("Input", i => this.Input = i == -1 ? null : this.Map.Town.ZoneManager.GetZone<Stockpile>(i));
            //tag.TryGetTagValue<int>("Output", i => this.Output = i == -1 ? null : this.Map.Town.ZoneManager.GetZone<Stockpile>(i));
            tag.TryGetTagValue("Input", out this._inputID);
            tag.TryGetTagValue("Output", out this._outputID);
            tag.TryGetTagValue("UnfinishedItem", out this._unfinishedItemRefID);

            this.ReagentRestrictions.Clear();
            tag.TryGetTagValue<List<SaveTag>>("Restrictions", restrictionsTag =>
            {
                foreach (var rTag in restrictionsTag)
                {
                    var name = rTag.GetValue<string>("Reagent");
                    rTag.TryGetTagValue<List<SaveTag>>("Items", list =>
                    {
                        foreach (var itemTag in list)
                        {
                            var item = (int)itemTag.Value;
                            this.ToggleReagentRestriction(name, item);
                        }
                    });
                }
            });
            return this;
        }

        public override string ToString()
        {
            return this.GetUniqueLoadID();
        }
        internal void Removed()
        {
            if (this.DetailsGui is not null && this.DetailsGui.Tag == this)
                this.DetailsGui.GetWindow()?.Hide();
        }

        public Control GetListControlGui()
        {
            var box = new GroupBox
            {
                BackgroundColor = UIManager.DefaultListItemBackgroundColor
            };

            var btnUp = new ButtonIcon(Icon.ArrowUp, MoveUp);
            var btnDown = new ButtonIcon(Icon.ArrowDown, MoveDown) { Location = btnUp.BottomLeft };
            box.AddControls(btnUp, btnDown);

            var orderName = new Label(this.Reaction.Label) { Location = btnUp.TopRight };
            var comboFinishMode = new ComboBoxNewNew<CraftOrderFinishMode>(CraftOrderFinishMode.AllModes, 100, c => c.GetString(this), ChangeFinishMode, () => this.FinishMode) { Location = orderName.BottomLeft };

            box.AddControls(orderName,
                comboFinishMode);

            var btnClose = new IconButton(Icon.X) { LocationFunc = () => new Vector2(PanelTitled.GetClientLength(290), 0), BackgroundTexture = UIManager.Icon16Background };
            btnClose.Anchor = Vector2.UnitX;
            btnClose.LeftClickAction = RemoveOrder;
            btnClose.ShowOnParentFocus(true);
            box.AddControls(btnClose);

            var btnMinus = new Button("-", Minus, Button.DefaultHeight) { Location = comboFinishMode.TopRight };
            var btnPlus = new Button("+", Plus, Button.DefaultHeight) { Location = btnMinus.TopRight };
            box.AddControls(btnMinus, btnPlus);

            this.DetailsGui = this.DetailsGui ??= new CraftOrderDetailsGui(this);

            var btnDetails = new Button("Details", ToggleDetails);
            box.AddControls(btnDetails.AnchorToBottomRight());

            return box;

            void ToggleDetails()
            {
                if (DetailsWindow is null)
                    DetailsWindow = new Window() { Movable = true, Closable = true };
                DetailsWindow.Client.ClearControls();
                DetailsWindow.Client.AddControls(this.DetailsGui);
                DetailsWindow.SetTitle(this.Name);
                if (DetailsWindow.Show())
                    DetailsWindow.Location = UIManager.Mouse;
            }
            void MoveDown()
            {
                ChangeOrderPriority(false);
            }
            void MoveUp()
            {
                ChangeOrderPriority(true);
            }
            void ChangeOrderPriority(bool p)
            {
                CraftingManager.WriteOrderModifyPriority(Client.Instance.OutgoingStream, this, p);
            }
            void RemoveOrder()
            {
                PacketOrderRemove.Send(this.Map.Net, this);
            }
            void Minus()
            {
                CraftingManager.WriteOrderModifyQuantityParams(Client.Instance.OutgoingStream, this, -1);
            }
            void Plus()
            {
                CraftingManager.WriteOrderModifyQuantityParams(Client.Instance.OutgoingStream, this, 1);
            }
            void ChangeFinishMode(CraftOrderFinishMode obj)
            {
                PacketCraftOrderChangeMode.Send(this, (int)obj.Mode);
            }
        }
        [Obsolete]
        public void ShowDetailsUI(Action<CraftOrder, string, ItemDef[], MaterialDef[], MaterialTypeDef[]> callback)
        {
            var box = new ScrollableBoxNewNew(200, 200, ScrollModes.Vertical);
            var list = DetailsUIReagents ??= new ListCollapsibleNew();
            list.Clear();
            foreach (var r in this.Reaction.Reagents)
            {
                var items = r.Ingredient.GetAllValidItemDefs();
                var itemTypesNode = new ListBoxCollapsibleNode(r.Name);
                list.AddNode(itemTypesNode);
                foreach (var i in items)
                {
                    var mats = i.GetValidMaterials().ToList();

                    var itemNode = new ListBoxCollapsibleNode(i.Name, () => new CheckBoxNew()
                    {
                        TickedFunc = () => !mats.Any(v => this.IsRestricted(r.Name, v)),
                        LeftClickAction = () => callback(
                            this,
                            r.Name,
                            null,
                            mats.GroupBy(m => this.Restrictions[r.Name].Material.Contains(m)).OrderBy(c => c.Count()).First().ToArray(),
                            null)
                    });
                    itemTypesNode.AddNode(itemNode);
                    foreach (var mat in mats)
                    {
                        itemNode.AddLeaf(new CheckBoxNew(mat.Name)
                        {
                            TickedFunc = () => !this.IsRestricted(r.Name, mat),
                            LeftClickAction = () => callback(this, r.Name, null, new MaterialDef[] { mat }, null)
                        });
                    }
                }
            }
            list.Build();
            box.AddControls(list);
            var container = DetailsUIContainer ??= new GroupBox()
                .AddControls(box)
                .ToWindow("Details")
                .SnapToMouse()
                .SetGameEventAction(e =>
                {
                    if (e.Type == Components.Message.Types.OrderDeleted && e.Parameters[0] == DetailsUIContainer.Tag)
                    {
                        DetailsUIContainer.Hide();
                    }
                });

            if (container.Tag == this)
            {
                container.Toggle();
            }
            else
            {
                container.Show();
            }

            container.Tag = this;
            container.GetWindow().SetTitle($"\"{this.Label}\" details");
        }
    }
}
