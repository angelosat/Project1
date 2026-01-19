using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.Components;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public sealed class Actor : Entity
    {
        public MobileComponent Mobile => this.GetComponent<MobileComponent>();

        internal NpcSkillsComponent Skills => this.GetComponent<NpcSkillsComponent>();

        internal AttributesComponent Attributes => this.GetComponent<AttributesComponent>();

        internal NpcComponent Npc => this.GetComponent<NpcComponent>();

        public PossessionsComponent Possessions => this.GetComponent<PossessionsComponent>();

        public WorkComponent Work => this.GetComponent<WorkComponent>();

        public AIComponent AI => this.GetComponent<AIComponent>();

        internal PersonalityComponent Personality => this.GetComponent<PersonalityComponent>();

        [InspectorHidden]
        public Skill this[SkillDef skill] => this.Skills.GetSkill(skill);

        [InspectorHidden]
        public Trait this[TraitDef trait] => this.GetTrait(trait);

        [InspectorHidden]
        public Entity this[GearType slot] => this.GetEquipmentSlot(slot);


        public MoodComp Mood => this.GetComponent<MoodComp>();

        public float MoodValue => this.Mood.Mood;

        public Actor()
        {

        }
        public Actor(ItemDef def, int amount) : base(def, amount)
        {
            
        }
        public override float Height => this.Physics.Height - (this.Mobile.Crouching ? 1 : 0);
        public override bool IsHaulable => false;

        public float Acceleration
        {
            get => this.Mobile.Acceleration;
            set => this.Mobile.Acceleration = value;
        }

        public Interaction CurrentInteraction => this.Work.Task;
        //AIState _state;

        //public AIState State => this._state ??= this.GetComponent<AIComponent>().State;
        internal Plan CurrentTask
        {
            get => this.AI.State.CurrentTask;
            set => throw new Exception();// this.State.CurrentTask = value;
        }
        //internal BehaviorPerformTask CurrentTaskBehavior
        //{
        //    get => this.State.CurrentTaskBehavior;
        //    set => this.State.CurrentTaskBehavior = value;
        //}
        public AILog Log => AIState.GetState(this).Log;
        public ItemPreferencesManager ItemPreferences => this.GetState().ItemPreferences;

        public Room AssignedRoom => this.Town?.RoomManager.FindRoom(this.RefId); // replaced this.town with this.net.map.town because when the actor leaves the map, this.town returns null
        internal Workplace Workplace => this.Town?.ShopManager.GetShop(this); // replaced this.town with this.net.map.town because when the actor leaves the map, this.town returns null
        public bool IsTownMember => this.Town?.Members.Contains(this.RefId) ?? false; // replaced this.town with this.net.map.town because when the actor leaves the map, this.town returns null


        public override string Name => this.Npc.FullName;
        internal override GameObject SetName(string name)
        {
            // HACK
            var splitname = name.Split(' ');
            this.Npc.FirstName = splitname[0];
            this.Npc.LastName = splitname.Length > 1 ? splitname[1] : "";
            return this;
        }

        internal void Loot(Entity loot, FrontierDef area)
        {
            var net = this.Net;
            if (net is Server server)
            {
                loot.SyncInstantiate(server);
                PacketInventoryInsertItem.Send(server, this, loot, area);
            }
            //this.Log.Write($"Looted [{loot.Name},{loot.PrimaryMaterial.Color}] while exploring [{area.Name}]"); // call this before inserting because the item might be absorbed/disposed
            //this.Log.Write($"Looted [{loot.Name},{loot.Body.Material.Color}] while exploring [{area.Name}]"); // call this before inserting because the item might be absorbed/disposed
            this.Inventory.Insert(loot);
        }
        internal bool InitiateTrade(Actor actor, Entity item, int itemcost)
        {
            // TODO do stuff with item and itemcost
            var state = this.GetState();
            if (state.TradingPartner != null)
                return false;
            state.TradingPartner = actor;
            return true;
        }

        internal bool HasMoney(int amount)
        {
            var coins = this.Inventory.First(i => i.Def == ItemDefOf.Coins); // TODO find all ammount instead of find first
            return coins?.StackSize >= amount;
        }
        internal Entity GetMoney()
        {
            return this.Inventory.First(i => i.Def == ItemDefOf.Coins) as Entity;
        }
        internal int GetMoneyTotal()
        {
            return this.Inventory.Count(o => o.Def == ItemDefOf.Coins);
        }

        //internal void ModifyNeed(NeedDef def, Func<int, int> modOldValue)
        //{
        //    var need = this.GetNeed(def);
        //    var old = need.Value;
        //    //need.Value = modOldValue(need.Value);
        //    need.SetValue(modOldValue(need.Value), this);
        //    //this.Net?.EventOccured((int)Message.Types.NeedUpdated, this, need, need.Value - old);
        //    this.World.Events.Post(new ActorNeedUpdatedEvent(this, need.NeedDef, need.Value - old));

        //}

        internal void Carry(Entity item)
        {
            this.Inventory.Haul(item);
        }

        /// <summary>
        /// if force is true, target actor drops current carried item and replaces it with the given one
        /// </summary>
        /// <param name="seller"></param>
        /// <param name="force"></param>
        internal void GiveCarriedTo(Actor target, bool force = false)
        {
            throw new NotImplementedException();
        }

        internal void ForceTask(PlanDef taskdef, TargetArgs target)
        {
            throw new NotImplementedException();
        }

        internal void FaceTowards(TargetArgs targetA)
        {
            if(targetA is not null)
                this.FaceTowards(targetA.Global);
        }
        internal void FaceTowards(Vector3 global)
        {
            this.Direction = global - this.Global;
            this.Direction.Normalize();
            this.Net.LogStateChange(this.RefId);
        }
        internal void ForceTask(Planner taskGiver, TargetArgs target)
        {
            var task = taskGiver.TryTaskOn(this, target, true);
            if (task is not null)
                this.GetState().ForceTask(task);
        }
        internal bool CanStandInNew(Vector3 global)
        {
            var map = this.Map;
            var occupyingCells = this.Def.OccupyingCellsStanding(global);
            if (occupyingCells.Any(c => this.Map.IsSolid(c)))
                return false;
            return map.IsSolid(global.Below());
        }

        internal bool CanStandIn(Vector3 global)
        {
            var map = this.Map;
            return
                map.GetBlock(global).IsStandableIn &&
                map.GetBlock(global.Above()).IsStandableIn &&
                map.GetBlock(global.Below()).IsStandableOn; //TODO: take into account actor's height instead of hardcoding checks 2 blocks above
        }
        internal bool CanStandOn(Vector3 global)
        {
            var map = this.Map;
            var above = global.Above();
            return
                map.GetBlock(global).IsStandableOn &&
                map.GetBlock(above).IsStandableIn &&
                map.GetBlock(above.Above()).IsStandableIn; //TODO: take into account actor's height instead of hardcoding checks 2 blocks above
        }

        internal void FinishConversation()
        {
            if (this.Net is Client)
                return;
            var state = this.GetState();
            state.ConversationPartner.GetState().ConversationPartner = null;
            state.ConversationPartner = null;
        }

        internal void TalkTo(Actor target, ConversationTopic topic)
        {
            topic.ApplyNew(this, target);
        }

        internal void EnqueueCommunication(Actor target, ConversationTopic topic)
        {
            this.GetState().CommunicationPending.Add(target, topic);
        }

        internal ConversationTopic GetNextConversationTopicFor(Actor target)
        {
            var state = this.GetState();
            var topic = state.CommunicationPending[target];
            state.CommunicationPending.Remove(target);
            return topic;
        }
        internal void Interact(Interaction interaction)
        {
            AIManager.Interact(this, interaction, TargetArgs.Null);
        }
        internal void Interact(Interaction interaction, TargetArgs targetArgs)
        {
            AIManager.Interact(this, interaction, targetArgs);
        }
        internal void Interact(Interaction interaction, Vector3 target)
        {
            AIManager.Interact(this, interaction, new TargetArgs(this.Map, target));
        }
        internal void Interact(Interaction interaction, GameObject target)
        {
            AIManager.Interact(this, interaction, new TargetArgs(target));
        }
        internal void EndInteraction()
        {
            AIManager.EndInteraction(this);
        }
        
        internal void Equip(GameObject item)
        {
            this.Interact(new InteractionEquip(), item);
        }
        internal bool IsEquipping(Entity item)
        {
            return this.GetGear().Any(i => i == item);
        }
        internal int GetReservedAmount(GameObject item)
        {
            return this.Town.ReservationManager.GetReservedAmount(this, item);
        }

        internal void StopPathing()
        {
            this.GetState().Path = null;
        }

        //public override GameObject Create()
        //{
        //    return new Actor();
        //}

        

        internal void AddNeed(params NeedDef[] defs)
        {
            this.GetComponent<NeedsComponent>().Add(defs);
        }
        //[Obsolete]
        //public static Actor Create(ItemDef def)
        //{
        //    //var obj = def.CreateBase() as Actor;
        //    var obj = ActorDefOf.Npc.Create() as Actor;
        //    //obj.Physics.Height = def.Height;

        //    foreach (var b in obj.Body.GetAllBones())
        //        b.Material = def.DefaultMaterial;

        //    obj.Sprite.Customization = new CharacterColors(obj.Body).Randomize();
        //    return obj;
        //}
        public EffectsComponent Effects => this.GetComponent<EffectsComponent>();
        public override Color GetNameplateColor()
        {
            if (this.IsPlayerControlled)
                return Color.Yellow;
            if (this.IsTownMember)
                return Color.White;
            return Color.Cyan;
        }

        internal void EndCurrentTask()
        {
            this.Work.Interrupt();
            this.GetComponent<AIComponent>().FindBehavior<BehaviorHandlePlans>().EndCurrentPlan(this);
        }
        internal void MoveToggle(bool toggle)
        {
            if (this.Net is Server)
                PacketEntityMoveToggle.Send(this.Net as NetEndpoint, this.RefId, toggle);

            this.Mobile.Toggle(this, toggle);
        }
        public void AddMoodlet(Moodlet m)
        {
            this.GetComponent<MoodComp>().Add(m);
        }
        public void RemoveMoodlet(MoodletDef mdef)
        {
            this.GetComponent<MoodComp>().Remove(mdef);
        }
        public bool HasMoodlet(MoodletDef mdef)
        {
            return this.GetComponent<MoodComp>().Contains(mdef);
        }

        readonly Button btnVisitor = new("Visitor");

        internal override IEnumerable<(string label, Type guiType)> GetQuickButtons()
        {
            yield return ("Log", typeof(NpcLogUINewNew));
            yield return ("Skills", typeof(SkillsUINew));
            yield return ("Needs", typeof(NeedsMoodsUINew));
            yield return ("Gear", typeof(InventoryUINew));
            yield return ("Stats", typeof(StatsGuiNew));
        }
        //public override IEnumerable<Control> GetSelectionDetails()
        //{
        //    yield return GuiBuilder.BuildFloating<SkillsUI>(this);
        //    yield return GuiBuilder.BuildFloating<PersonalityUI>(this);
        //    yield return GuiBuilder.BuildFloating<NpcLogUINew>(this);
        //    yield return GuiBuilder.BuildFloating<InventoryUI>(this);
        //    yield return GuiBuilder.BuildFloating<NeedsMoodsUI>(this);
        //    yield return GuiBuilder.BuildFloating<StatsGui>(this);
        //}
        //protected override IEnumerable<Button> GetInfoTabsExtraNew()
        //{
        //    yield return new Button("Skills").SetLeftClickAction(b => GuiBuilder.RefreshSingleton<SkillsUI>(this).SetTitle(this.Name).Toggle()) as Button;
        //    yield return new Button("Personality").SetLeftClickAction(b => GuiBuilder.RefreshSingleton<PersonalityUI>(this).SetTitle(this.Name).Toggle()) as Button;
        //    yield return new Button("Log").SetLeftClickAction(b => GuiBuilder.RefreshSingleton<NpcLogUINew>(this).SetTitle(this.Name).Toggle()) as Button;
        //    yield return new Button("Gear").SetLeftClickAction(b => GuiBuilder.RefreshSingleton<InventoryUI>(this).SetTitle(this.Name).Toggle()) as Button;
        //    yield return new Button("Needs").SetLeftClickAction(b => GuiBuilder.RefreshSingleton<NeedsMoodsUI>(this).SetTitle(this.Name).Toggle()) as Button;
        //    yield return new Button("Stats").SetLeftClickAction(b => GuiBuilder.RefreshSingleton<StatsGui>(this).SetTitle(this.Name).Toggle()) as Button;

        //    if (!this.IsTownMember)
        //        yield return this.btnVisitor.SetLeftClickAction(b => this.GetVisitorProperties().ShowGui()) as Button;
        //}
        //static readonly (string, Type)[] AvailableInfoTypesTest =
        //[
        //    ("Skills", typeof(SkillsUI)),
        //    ("Personality", typeof(PersonalityUI)),
        //    ("Log", typeof(NpcLogUINew)),
        //    ("Gear", typeof(InventoryUI)),
        //    ("Needs", typeof(NeedsMoodsUI)),
        //    ("Stats", typeof(StatsGui))];
        public bool CanOperate(TargetArgs target)
        {
            if (target.Type != TargetType.Position)
                throw new Exception();
            var global = target.Global;
            return this.CanOperate(global);
        }
        public bool CanOperate(Vector3 global)
        {
            return this.FindOperatablePosition(global).HasValue;
        }
        public bool CanOperate(Vector3 global, out IntVec3 operatingPos)
        {
            var poos = this.FindOperatablePosition(global);
            operatingPos = poos.Value;
            return poos.HasValue;
        }
        public IntVec3? FindOperatablePosition(IntVec3 facilityGlobal)
        {
            var operatingPositions = this.Map.GetCell(facilityGlobal).GetInteractionSpotLocal(this.Map, facilityGlobal);
            if (!operatingPositions.Any())
                return null;
            foreach (var pos in operatingPositions)
            {
                var globalpos = facilityGlobal + pos;
                if (this.CanReach(globalpos) && this.Map.GetBlock(globalpos).IsStandableIn)
                    return globalpos;
            }
            return null;
        }

        public bool CanReach(GameObject obj)
        {
            return this.Map.Regions.CanReach(this.GetCellStandingOn(), obj.Global.ToCell(), this as Actor);
            //old
            //return this.Map.GetRegionDistance(this.GetCellStandingOn(), obj.Global.ToCell(), this as Actor) != -1;
        }
        internal bool CanReach(Vector3 global)
        {
            return this.Map.Regions.CanReach(this.GetCellStandingOn(), global.ToCell(), this as Actor);
            //old
            //return this.Map.GetRegionDistance(this.GetCellStandingOn(), global.ToCell(), this as Actor) != -1;
        }
        public bool CanReachAndReserve(Entity e)
        {
            return this.CanReach(e) && this.CanReserve(e);
        }
        public bool CanReachAndReserve(IntVec3 pos)
        {
            return this.CanReach(pos) && this.CanReserve(pos);
        }
        public bool CanReachAndReserve(BlockEntity entity)
        {
            return entity.CellsOccupied.All(pos => this.CanReach(pos) && this.CanReserve(pos));
        }
        internal int GetHaulStackLimitFromEndurance(ItemDef def)
        {
            var maxHaulWeight = StatDefOf.MaxHaulWeight.GetValue(this);
            var activityLevel = this.GetTrait(TraitDefOf.Activity)?.Normalized ?? 0;
            var maxDesiredEncumberance = maxHaulWeight + maxHaulWeight * activityLevel * .5f;
            var unitWeight = def.Weight;
            int stackEnduranceLimit = (int)Math.Floor(maxDesiredEncumberance / unitWeight);
            var max = Math.Min(def.StackCapacity, stackEnduranceLimit); // this was missing and i was calculating it when i was calling this func
            return max;
        }
        internal int GetHaulStackLimitFromEndurance(GameObject haulable)
        {
            return this.GetHaulStackLimitFromEndurance(haulable.Def);
        }

        internal float GetOpportunisticHaulSearchRange(int baseSearchRange)
        {
            var organizationValue = this.GetTrait(TraitDefOf.Planning)?.Normalized ?? 0;
            var num = baseSearchRange * organizationValue * .5f;
            return baseSearchRange + num;
        }

        public bool IsTooTiredToWork
        {
            get
            {
                var stamina = this.GetResource(ResourceDefOf.Stamina);
                var staminaPercentage = stamina.Percentage;
                var threshold = StatDefOf.StaminaThresholdForWork.GetValue(this);
                var tired = staminaPercentage < threshold;
                return tired;
            }
        }

        public GameObject AttackTarget => null;//.GetComponent<AttackComponent>().Target;

        

        internal Trait GetTrait(TraitDef trait)
        {
            return this.GetComponent<PersonalityComponent>().Traits[trait];//.First(t => t.TraitDef == trait);
        }

        internal void WalkToggle(bool toggle)
        {
            if (this.Net is Server)
                PacketEntityWalkToggle.Send(this.Net as NetEndpoint, this.RefId, toggle);

            this.Mobile.ToggleWalk(toggle);
        }
        internal void SprintToggle(bool toggle)
        {
            if (this.Net is Server)
                PacketEntitySprintToggle.Send(this.Net as NetEndpoint, this.RefId, toggle);

            this.Mobile.ToggleSprint(toggle);
        }
        internal void CrouchToggle(bool toggle)
        {
            if (this.Net is Server)
                PacketEntityCrouchToggle.Send(this.Net, this.RefId, toggle);

            this.Mobile.ToggleCrouch(toggle);
        }
        internal void Jump()
        {
            if (this.Net is Server)
                PacketEntityJump.Send(this.Net, this.RefId);

            this.Mobile.Jump(this);
        }

        internal IEnumerable<(PlanDef task, Planner giver)> CanForceTaskOn(TargetArgs target)
        {
            if (target == null || target.Type == TargetType.Null)
                yield break;
            var givers = Planner.CitizenTaskGivers.Concat(Planner.EssentialPlanners);
            foreach (var giver in givers)
                if (giver.CanGiveTask(this, target) is PlanDef taskDef)
                    yield return (taskDef, giver);
        }

        internal bool OwnsOrCanClaim(Entity item)
        {
            return this.GetPossesions().Contains(item) || (!this.Map?.Town.GetMembers().Any(a => a.Owns(item)) ?? false);
        }
        internal bool Owns(Entity item)
        {
            return this.GetPossesions().Contains(item);
        }

        internal GearType[] GetGearTypes()
        {
            return this.GetComponent<GearComponent>().Equipment.Slots.Select(s => GearType.Dictionary[(GearType.Types)s.ID]).ToArray();
        }
        internal Entity[] GetGear()
        {
            return this.GetComponent<GearComponent>().Equipment.Slots.Where(sl => sl.Object != null).Select(sl => sl.Object as Entity).ToArray();
        }
        internal Entity GetEquipmentSlot(GearType type)
        {
            return this.Gear.GetSlot(type).Object as Entity;
        }
        public ButtonNew GetButton(int width, Func<string> bottomText, Action onLeftClick)
        {
            return ButtonNew.CreateBig(onLeftClick, width, this.RenderIcon(), () => this.Name, bottomText);
        }
        internal float GetToolWorkAmount(ToolUseDef toolUse)
        {
            if (this.Gear.GetSlot(GearType.Mainhand).Object is not Tool tool)
                return 1;

            var ability = tool.ToolComponent.ToolUse;
            return ability == toolUse ? tool.GetStat(StatDefOf.ToolEffectiveness) : 1;
        }
        public int EvaluateItem(Entity item)
        {
            var score = ItemUsefulnessEvaluator.Evaluate(this, item);
            return score;
        }

        internal bool CanAcceptQuest(QuestDef quest)
        {
            return !this.GetVisitorProperties().HasQuest(quest);
        }
        internal void AcceptQuest(int questID)
        {
            this.GetVisitorProperties().AcceptQuest(this.Town.QuestManager.GetQuest(questID));
        }
        internal bool AcceptQuest(QuestDef quest)
        {
            return this.GetVisitorProperties().AcceptQuest(quest);
        }
        internal override void OnSpawn(MapBase map)
        {
            base.OnSpawn(map);
            if (this.GetVisitorProperties() is WorldInhabitantView props)
                props.OffsiteArea = null;
        }

        
    }
}
