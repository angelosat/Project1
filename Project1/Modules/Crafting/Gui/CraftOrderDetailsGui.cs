using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System;
using System.Linq;

namespace Start_a_Town_
{
    class CraftOrderDetailsGui : GroupBox
    {
        public Action<Reaction.Product.ProductMaterialPair> Callback = a => { };
        readonly Panel PanelCollapsible;
        readonly GroupBox PanelInfo;
        readonly CheckBoxNew ChkHaulOnFinish;
        readonly GroupBox ReagentsContainer;
        CraftOrder Order => this.Tag as CraftOrder;
        public CraftOrderDetailsGui(CraftOrder order)
            :this()
        {
            this.Tag = order;
        }
        CraftOrderDetailsGui()
        {
            this.AutoSize = true;


            this.PanelCollapsible = new Panel() { AutoSize = false }.SetClientDimensions(200, 200);

            //this.ReagentsContainer = new ScrollableBoxNewNew(200, 200, ScrollModes.Vertical);
            this.ReagentsContainer = new GroupBox();
            //this.ReagentsContainer.AddControls(scrollableBox);
            var scrollableBox = new ScrollableBoxTest(this.ReagentsContainer, 200, 200, ScrollModes.Vertical);

            this.PanelCollapsible.AddControls(scrollableBox);// this.ReagentsContainer);

            var boxinfo = new GroupBox();
            this.PanelInfo = new(128, 128);// this.PanelCollapsible.Size);

            this.AddControlsHorizontally(this.PanelInfo, this.PanelCollapsible);

            this.ChkHaulOnFinish = new CheckBoxNew("Haul on finish", () => PacketCraftOrderToggleHaul.Send(this.Order, !this.Order.HaulOnFinish), () => this.Order.HaulOnFinish)
            {
                Location = this.PanelCollapsible.BottomLeft
            };
            //this.AddControls(this.ChkHaulOnFinish);

            var input = new ComboBoxNewNew<Stockpile>(200, "Input", s => s?.Name ?? "Anywhere", s => PacketCraftOrderSync.Send(this.Order, s, this.Order.Output), () => this.Order?.Input, () => this.Order.Map.Town.ZoneManager.GetZones<Stockpile>().Prepend(null));
            var output = new ComboBoxNewNew<Stockpile>(200, "Output", s => s?.Name ?? "Anywhere", s => PacketCraftOrderSync.Send(this.Order, this.Order.Input, s), () => this.Order?.Output, () => this.Order.Map.Town.ZoneManager.GetZones<Stockpile>().Prepend(null));
            this.AddControlsBottomLeft(input, output);
        }
        protected override void OnTagChanged()
        {
            var list = this.CreateList(this.Order);
            list.Build();
            this.ReagentsContainer.ClearControls();
            this.ReagentsContainer.AddControls(list);

            this.PanelInfo.ClearControls();
            this.PanelInfo.AddControls(this.Order.Reaction.GetInfoGui());
        }
        ListCollapsibleNew CreateList(CraftOrder order)
        {
            var list = new ListCollapsibleNew();// 200, 200);
            foreach (var r in order.Reaction.Reagents)
            {
                var items = r.Ingredient.GetAllValidItemDefs();

                var itemTypesNode = new ListBoxCollapsibleNode(r.Name);

                list.AddNode(itemTypesNode);
                foreach (var i in items)
                {
                    var mats = r.Ingredient.GetAllValidMaterials().Intersect(i.GetValidMaterials());

                    if (mats.Count() <= 1) // UNDONE
                    {
                        itemTypesNode.AddLeaf(new CheckBoxNew(i.Label)
                        {
                            TickedFunc = () => !order.IsRestricted(r.Name, i),
                            LeftClickAction = () => CraftingManager.SetOrderRestrictions(order, r.Name, new[] { i }, null, null)
                        });
                    }
                    else
                    {
                        var itemNode = new ListBoxCollapsibleNode(i.Label, () => new CheckBoxNew()
                        {
                            TickedFunc = () => !mats.Any(v => order.IsRestricted(r.Name, v)),
                            LeftClickAction = () => CraftingManager.SetOrderRestrictions(
                                order,
                                r.Name,
                                null,
                                mats.GroupBy(m => order.Restrictions[r.Name].Material.Contains(m)).OrderBy(c => c.Count()).First().ToArray(),
                                null)
                        });
                        itemTypesNode.AddNode(itemNode);
                        foreach (var mat in mats)
                        {
                            itemNode.AddLeaf(new CheckBoxNew(mat.Label)
                            {
                                TickedFunc = () => !order.IsRestricted(r.Name, mat),
                                LeftClickAction = () => CraftingManager.SetOrderRestrictions(order, r.Name, null, new MaterialDef[] { mat }, null)
                            });
                        }
                    }
                }
            }
            return list;
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch ((Components.Message.Types)e.Type)
            {
                case Message.Types.OrderParametersChanged:
                    var order = e.Parameters[0] as CraftOrder;
                    if (order == this.Order)
                        this.RefreshReagents();
                    break;

                default:
                    break;
            }
        }

        private void RefreshReagents()
        {

        }
    }
}
