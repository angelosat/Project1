using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Project1.Framework;
using Project1.Framework.UI;
using Project1.Core.Blocks;
using Project1.Core.Resources;
using Project1.Core.Stats;
using Project1.Core.Simulation;
using Project1.Core.Gear;
using Project1.Core.Entities.Stats;
using Project1.Core.Attributes;
using Project1.Core.AI.Behaviors.Reserve;
using Project1.Core.Networking.Entities;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.Conversation;
using Project1.Core.AI.Planners;
using Project1.Core.Quests;
using Project1.Core.Towns;
using Project1.Core.Towns.AI.Behaviors.ItemEvaluators;
using Project1.Core.Towns.Labors;
using Project1.Core.UI;
using Project1.Core.World.WorldAreas;
using Project1.Core.Interactions;
using Project1.Core.Networking;
using Project1.Core.Rooms;
using Project1.Core.Skills;
using Project1.Core.Mood;
using Project1.Core.Networking.Inventory;
using Project1.Core.Networking;

namespace Project1.Core.Entities.Actors
{
    public sealed class Actor : Entity
    {
        public MobileComponent Mobile => this.GetComponent<MobileComponent>();

        internal SkillsComponent Skills => this.GetComponent<SkillsComponent>();

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
        public Entity this[GearTypeDef slot] => this.GetEquipmentSlot(slot);


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
        internal Plan CurrentPlan
        {
            get => this.AI.State.CurrentPlan;
            set => throw new Exception();
        }
        
        public AILog Log => AIState.GetState(this).Log;
        public ItemPreferencesManager ItemPreferences => this.AI.State.ItemPreferences;

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
            this.Inventory.Insert(loot);
        }
        internal bool InitiateTrade(Actor actor, Entity item, int itemcost)
        {
            // TODO do stuff with item and itemcost
            var state = this.AI.State;
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
        internal void ForceTask(PlannerDef planner, TargetArgs target)
        {
            var task = planner.Worker.TryTaskOn(this, target, true);
            if (task is not null)
                this.AI.State.ForceTask(task);
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
            var state = this.AI.State;
            state.ConversationPartner.AI.State.ConversationPartner = null;
            state.ConversationPartner = null;
        }

        internal void TalkTo(Actor target, ConversationTopic topic)
        {
            topic.ApplyNew(this, target);
        }

        internal void EnqueueCommunication(Actor target, ConversationTopic topic)
        {
            this.AI.State.CommunicationPending.Add(target, topic);
        }

        internal ConversationTopic GetNextConversationTopicFor(Actor target)
        {
            var state = this.AI.State;
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
        [Obsolete]
        internal void Equip(GameObject item)
        {
            throw new Exception();
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
            this.AI.State.Path = null;
        }
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
            yield return ("Gear", typeof(InventoryUI));
            yield return ("Stats", typeof(StatsGuiNew));
        }
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
        }
        internal bool CanReach(Vector3 global)
        {
            return this.Map.Regions.CanReach(this.GetCellStandingOn(), global.ToCell(), this as Actor);
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
            var maxHaulWeight = StatDefOf.MaxHaulWeight.CalculateFor(this);
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
                var threshold = StatDefOf.StaminaThresholdForWork.CalculateFor(this);
                var tired = staminaPercentage < threshold;
                return tired;
            }
        }

        public GameObject AttackTarget => null;

        internal Trait GetTrait(TraitDef trait)
        {
            return this.GetComponent<PersonalityComponent>().Traits[trait];
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

        internal IEnumerable<(PlanDef task, PlannerDef giver)> CanForceTaskOn(TargetArgs target)
        {
            if (target == null || target.Type == TargetType.Null)
                yield break;
            var planners = Planner.CitizenTaskGivers.Concat(Planner.EssentialPlanners);
            foreach (var planner in planners)
                if (planner.Worker.CanGiveTask(this, target) is PlanDef taskDef)
                    yield return (taskDef, planner);
        }

        internal bool OwnsOrCanClaim(Entity item)
        {
            return this.GetPossesions().Contains(item) || (!this.Map?.Town.GetMembers().Any(a => a.Owns(item)) ?? false);
        }
        internal bool Owns(Entity item)
        {
            return this.GetPossesions().Contains(item);
        }

        internal GearTypeDef[] GetGearTypes()
        {
            var profile = this.Profile as ActorDnaDef;
            return profile.Gear;
        }
        internal Entity[] GetGear()
        {
            return this.GetComponent<GearComponent>().Equipment.Slots.Where(sl => sl.Object != null).Select(sl => sl.Object as Entity).ToArray();
        }
        internal Entity GetEquipmentSlot(GearTypeDef type)
        {
            return this.Gear.GetSlot(type).Object as Entity;
        }
        public ButtonNew GetButton(int width, Func<string> bottomText, Action onLeftClick)
        {
            return ButtonNew.CreateBig(onLeftClick, width, this.RenderIcon(), () => this.Name, bottomText);
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