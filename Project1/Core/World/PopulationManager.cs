using Project1.Core.AI.MetaRoles;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Needs;
using Project1.Core.Networking;
using Project1.Core.Networking.Entities;
using Project1.Core.Resources;
using Project1.Core.Simulation;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Tools;
using Project1.Core.Towns;
using Project1.Core.Towns.AI.Needs;
using Project1.Core.UI;
using Project1.Core.UI.Hud;
using Project1.Core.World.WorldAreas;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Input;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Project1.Core.World;

public sealed class PopulationManager : Inspectable, ISaveable, ISerializable
{
    bool Populated;
    readonly ObservableCollection<WorldInhabitantView> WorldInhabitants = [];
    public IEnumerable<WorldInhabitantView> AllActors => this.WorldInhabitants;
    public readonly StaticWorld World;
    const int WorldPopulationCap = 3;//1;//0;//6;
    public int WorldPopulationCount { get; private set; }
    const float TickRate = 1 / 3f, InitialChance = .05f, VisitChanceBaseRate = .001f;// 2 seconds per tick //1 tick per second 
    const int InitialApproval = 50;
    readonly HashSet<int> Undiscovered = [];
    readonly Scheduler Schedule = new(Ticks.FromMinutes(1));

    int TickCount = (int)(Ticks.PerSecond / TickRate);
    public PopulationManager(StaticWorld world)
    {
        this.World = world;
        world.Events.ListenTo<EntityDisposedEvent>(OnEntityDisposed);
        world.Events.ListenTo<EntityRegisteredEvent>(OnEntityRegistered);
    }

    private void OnEntityDisposed(EntityDisposedEvent e)
    {
        if (e.Entity is Actor actor)
        {
            if (this.WorldInhabitants.FirstOrDefault(v => v.Actor == actor) is not WorldInhabitantView view)
                return;
            this.WorldInhabitants.Remove(view);
            this.WorldPopulationCount--;
        }

    }
    private void OnEntityRegistered(EntityRegisteredEvent e)
    {
        if (e.Entity is Actor actor)
        {
            if (this.WorldInhabitants.FirstOrDefault(v => v.Actor == actor) is WorldInhabitantView existing)
                throw new Exception();
            this.WorldInhabitants.Add(new(actor));
            this.WorldPopulationCount++;
            this.Undiscovered.Add(actor.RefId);
        }

    }
    public void Tick()
    {
        if(this.Schedule.OnSchedule(this.World.CurrentTick))
            this.PopulateRuntime();
    }

    internal void Initialize()
    {
        this.InitializeInhabitants();
    }

    void InitializeInhabitants()
    {
        for (int i = 0; i < WorldPopulationCap; i++)
        {
            var actor = GenerateInhabitant();
            this.World.Register(actor);
            this.RegisterVisitor(actor);
        }
    }

    private Actor PopulateRuntime()
    {
        var net = this.World.Net;
        if (net.IsServer && this.WorldPopulationCount < WorldPopulationCap)
        {
            Actor actor = GenerateInhabitant();
            var chosenPlace = this.World.PlaceAtRandom(actor);//
            net.Report($"{actor.Name} created and placed at {chosenPlace.LabelReadable}");
            return actor;
        }
        return null;
    }
    private Actor PopulateRuntime(INetEndpoint net)
    {
        if (net is Server && this.WorldPopulationCount < WorldPopulationCap)
        {
            Actor actor = GenerateInhabitant();
            var chosenPlace = this.World.PlaceAtRandom(actor);//
            net.Report($"{actor.Name} created and placed at {chosenPlace.LabelReadable}");
            return actor;
        }
        return null;
    }
   
    private Actor GenerateInhabitant()
    {
        var actor = ActorSystem.Create(ActorDnaDefOf.Npc, RoleMetaDefOf.Adventurer);
        var coins = ItemDefOf.Coins.Create().SetStackSize(500);
        var townscroll = ConsumableSystem.Create(ConsumableDefOf.TownScroll, MaterialDefOf.ShrubStem, 1);
        var inventory = actor.Inventory;
        inventory.Insert(coins);
        inventory.Insert(townscroll);
        var need = actor.GetNeed(AdventurerNeedsDefOf.Adventuring);
        need.Value = this.World.Random.Next(0, 100);
        actor.Needs.OverridePercentage(NeedDefOf.Energy, .1f);
        actor.Skills.Randomize();
        //actor.Resources.SetPercentage(ResourceDefOf.Health, .2f);

        var damagedTool = ToolSystem.CreateRandom(this.World.Random, 1);
        damagedTool.Resources.SetPercentage(ResourceDefOf.Durability, .05f);
        //inventory.Insert(damagedTool);

        this.World.Register(actor);
        return actor;
    }

    private void RegisterVisitor(Actor actor)
    {
        var props = new WorldInhabitantView(this.World, actor, InitialChance, InitialApproval) { OffsiteArea = FrontierDefOf.Forest };
        this.WorldInhabitants.Add(props);
        MakeVisitor(actor);
    }


    private static void MakeVisitor(Actor actor)
    {
    }

    public IEnumerable<WorldInhabitantView> Find(Func<WorldInhabitantView, bool> pred)
    {
        foreach (var v in this.WorldInhabitants.Where(pred))
            yield return v;
    }

    internal IEnumerable<WorldInhabitantView> GetVisitorProperties()
    {
        foreach (var v in this.WorldInhabitants)
            yield return v;
    }
    internal WorldInhabitantView GetVisitorProperties(Actor actor)
    {
        return this.WorldInhabitants.FirstOrDefault(v => v.Actor == actor);
    }
    internal void OnTargetSelected(IUISelection info, ISelectable selected)
    {
    }
    internal void OnTargetSelected(SelectionManager info, ISelectable selected)
    {
    }
    Control _gui;
    public Control Gui => this._gui ??= this.CreateGui();
    Control CreateGui()
    {
        var box = new ScrollableBoxNewNewNew(200, UIManager.LargeButton.Height * 8);
        var list = new ListBoxObservable<WorldInhabitantView, ButtonNew>(props =>
        {
            var npc = props.Actor;
            var btn = ButtonNew.CreateBigNew(
                //() => SelectionManager.Select(npc),
                //() => Ingame.Instance.Events.Post(new PlayerSelectionRectangleEvent([npc])),
                () => UIManager.ToggleUnique<SelectionDetailsGui>(npc),
                box.Viewport.Width,
                npc.RenderIcon(),
                new Label(() => npc.Npc.FullName) { TextColorFunc = ()=> npc.GetNameplateColor()},
                //new Label(() => $"{props.CurrentWorldLocation?.LabelReadable ?? "In town"}"));
                new LabelNew(() => $"{props.CurrentWorldLocation?.LabelReadable ?? "In town"}").InvalidateOn(((FrontierManager)((StaticWorld)npc.World).Space).Notifier));

            // debugging stuff
            btn.RightClickActionNew = b =>
            {
                if (!InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey))
                    return;
                ContextMenuManager.PopUp(
                    ("Force visit", () => this.ForceVisitDepart(npc)),
                    ("Dispose", () => PacketEntityDispose.Send(Client.Instance, npc.RefId, Client.Instance.PlayerData))
                );
            };
            return btn;
        });

        Func<WorldInhabitantView, bool>
            filterUndiscovered = i => !i.Discovered,
            filterVisiting = i => i.Actor.Exists,
            filterAway = i => !i.Actor.Exists && i.Discovered;

        var filters = list.CreateFilters(("All", null), ("Visiting", filterVisiting), ("Away", filterAway), ("Unknown", filterUndiscovered));

        list.Bind(this.WorldInhabitants);
        box.AddControlsVertically(filters, list);
        return box;
    }
    void ForceVisitDepart(Actor actor)
    {
        var serverActor = Server.Instance.World.Get<Actor>(actor.RefId);
        var newPercentage = serverActor.Map == null ? 1f : 0;// 0 : 1f;
        serverActor.Needs.OverridePercentage(AdventurerNeedsDefOf.Adventuring, newPercentage);
        serverActor.AI.Meta.LocationDecision.Reset();
        var debugmsg = $"{serverActor.Name}'s visit chance modifier set to 1";
        Server.Instance.ConsoleBox.Write(debugmsg);
        DebugConsole.Write(DebugConsole.Debug, debugmsg);
    }
    public void ResolveReferences()
    {
        this.WorldPopulationCount = 0;
        var allActors = this.World.GetEntities<Actor>();
        foreach (var actor in allActors)
        {
            this.WorldInhabitants.Add(new WorldInhabitantView(actor));
            this.WorldPopulationCount++;
        }
        //this.WorldPopulationCount = AllActors.Count();
    }
    public void ResolveReferencesOld()
    {
        foreach (var props in this.WorldInhabitants) // i added this to add visitor needs to existing visitors because I wasn't saving them in the needscomponent class
        {
            props.World = this.World;
            var actor = props.Actor;
            // TODO move this somewhere else
            if (this.World.Map.Net is Server)
                if (!actor.GetNeeds(AdventurerNeedsDefOf.NeedCategoryVisitor).Any())
                    MakeVisitor(actor);
            if (actor.IsSpawned)
                props.Discovered = true; // HACK
            props.OffsiteArea = FrontierDefOf.Forest; // HACK
        }
    }
    public SaveTag Save(string name = "")
    {
        var tag = new SaveTag(SaveTag.Types.Compound, name);
        this.Populated.Save(tag, "Populated");
        this.TickCount.Save(tag, "Tick");
        this.Undiscovered.Save(tag, "Undiscovered");
        return tag;
    }

    public ISaveable Load(SaveTag tag)
    {
        this.Populated.TryLoad(tag, "Populated");
        this.TickCount.TryLoad(tag, "Tick");
        this.Undiscovered.TryLoad(tag, "Undiscovered");
        return this;
    }
    public void Write(IDataWriter w)
    {
        w.Write(this.Undiscovered);
    }

    public ISerializable Read(IDataReader r)
    {
        this.Undiscovered.Read(r);
        return this;
    }
}
