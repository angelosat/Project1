using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.UI;
using Project1.Core.UI.Hud;
using Project1.Core.World;
using Project1.Core.World.WorldAreas;
using Project1.Core.WorldGen;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Project1.Core.Simulation;

public class StaticWorld : WorldBase
{
    static class Packets
    {
        public static readonly int PacketClockAdvanced;
        static Packets()
        {
            PacketClockAdvanced = Registry.PacketHandlers.Register(ReceiveClockAdvanced);
        }

        private static void ReceiveClockAdvanced(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            if (net is Server)
                throw new Exception();
            net.World.CurrentTick++;
        }
    }
    public override float Gravity => -0.015f;//-0.04f;// -0.05f; //35f;
    public const int Zenith = 14;

    public bool Lighting;
    
    //public const int TickTime = (int)(60 * 1.44f);
    public bool Flat;
    public bool Trees;
    TimeSpan ClockOffset = TimeSpan.FromHours(12);
    public override TimeSpan Clock => this.ClockOffset + TimeSpan.FromMilliseconds((double)this.CurrentTick * Ticks.IngameMillisecondsPerTick);
    //public MapCollection Maps;
    //public StaticMap Map => this.Maps.Values.First() as StaticMap;
    public StaticMap Map => this.Maps.First() as StaticMap;
    public override PopulationManager Population => this.PopulationManager;
    public IWorldSpaceManager Space;
    ulong currentTick;
    public override ulong CurrentTick { get => this.currentTick; set => this.currentTick = value; }

    readonly PopulationManager PopulationManager;
    public override void Tick()
    {
        this.PopulationManager.Tick();
        this.Space.Tick();
        this.CurrentTick++;
    }
    public string GetName()
    {
        return this.Name;
    }
    //public override MapBase GetMap(Vector2 mapCoords)
    //{
    //    return this.Maps.GetValueOrDefaultMy(mapCoords);
    //}
    public Random GetRandom()
    {
        return this.Random;
    }

    public int GetSeed()
    {
        return this.Seed;
    }
    public static byte[] GetHash(string inputString)
    {
        using HashAlgorithm algorithm = SHA256.Create();
        return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
    }

    StaticWorld() : base()
    {
        this.MaxHeight = 128;
        this.DefaultBlock = BlockDefOf.Soil.Block;
        this.Terraformers = new List<Terraformer>();
        this.Trees = true;
        //this.Maps = new MapCollection();
        this.PopulationManager = new PopulationManager(this);
        this.Space = new FrontierManager(this);
    }
    public StaticWorld(string name, List<Terraformer> mutators)
       : this()
    {
        if (name.IsNullEmptyOrWhiteSpace())
            throw new ArgumentNullException();
        this.Name = name;
        this.SeedArray = GetHash(name);
        this.Seed = BitConverter.ToInt32(this.SeedArray, 0);
        this.Random = new Random(this.Seed);
        this.Terraformers = mutators;
        this.DefaultBlock = BlockDefOf.Soil.Block;
    }

    public StaticWorld(SaveTag save)
        : this()
    {
        base.Load(save);

        this.Name = (string)save["Name"].Value;
        this.Seed = (int)save["Seed"].Value;
        save.TryGetTagValue<int>("RandomState", v =>
        {
            this.Random = new Random(v);
        });
        if (save.TryGetTag("Flat", out SaveTag flatTag))
            this.Flat = (bool)flatTag.Value;
        save.TryGetTagValue<double>("CurrentTick", v => this.CurrentTick = (ulong)v);

        if (save.TryLoadDefOut<BlockDef>("DefaultBlock", out var bd))
            this.DefaultBlock = bd.Block;
        else
            this.DefaultBlock = BlockDefOf.Soil.Block;
        this.Name = (string)save["Name"].Value;

        //this.Terraformers.LoadAbstract(save, "Mutators");
        this.Terraformers = Terraformer.Defaults.Select(d => d.Create()).ToList(); // HACK

        this.Population.TryLoad(save, "Population");
        var mapsList = save["Maps"].Value as List<SaveTag>;
        foreach (var tag in mapsList)
        {
            var map = StaticMap.Load(this, Vector2.Zero, tag);
            //this.Maps.Add(map.Coordinates, map);
            this.AddMap(map);
        }

    }
    public StaticWorld(IDataReader r)
       : this()
    {
        base.Read(r);
        this.Name = r.ReadString();
        this.Seed = r.ReadInt32();
        this.CurrentTick = r.ReadUInt64();
        this.Trees = r.ReadBoolean();
        this.DefaultBlock = r.ReadDef<BlockDef>().Block;
        this.Terraformers.ReadListAbstract(r);
        this.Population.Read(r);
    }
    public override void WriteData(IDataWriter w)
    {
        base.Write(w);
        w.Write(this.Name);
        w.Write(this.Seed);
        w.Write(this.CurrentTick);
        w.Write(this.Trees);
        w.Write(this.DefaultBlock.BlockDef);
        this.Terraformers.WriteAbstract(w);
        this.Population.Write(w);
    }
    public void GetFileInfo(out string saveDir, out string worldDir, out string worldFile)
    {
        saveDir = GlobalVars.SaveDir + @"/Worlds/";
        worldDir = this.Name + @"/";
        worldFile = this.Name + ".world.sat";
    }

    internal SaveTag SaveToTag()
    {
        var tag = base.Save();// new SaveTag(SaveTag.Types.Compound, "World");
        tag.Add(new SaveTag(SaveTag.Types.Int, "Seed", this.Seed));
        var currentRandomState = this.Random.Next();
        this.Random = new Random(currentRandomState);
        tag.Add(new SaveTag(SaveTag.Types.Int, "RandomState", currentRandomState));
        tag.Add(new SaveTag(SaveTag.Types.Double, "Time", this.Clock.TotalSeconds));
        this.CurrentTick.Save(tag, "CurrentTick");
        //this.DefaultBlock.BaseID.Save(tag, "DefaultBlock");
        tag.SaveDef("DefaultBlock", this.DefaultBlock.BlockDef);
        this.Name.Save(tag, "Name");
        this.Population.Save(tag, "Population");
        this.Terraformers.SaveAbstract(tag, "Mutators");
        var mapsTag = new SaveTag(SaveTag.Types.List, "Maps", SaveTag.Types.Compound);
        foreach (var map in this.Maps)
            mapsTag.Add(map.Save());
        tag.Add(mapsTag);
        return tag;
    }

    public string GetPath()
    {
        return WorldsPath + this.Name + "/";
    }
    static readonly string WorldsPath = GlobalVars.SaveDir + "/Worlds/Static/";
    public static DirectoryInfo[] GetWorlds()
    {
        DirectoryInfo directory = new DirectoryInfo(WorldsPath);
        if (!Directory.Exists(directory.FullName))
            Directory.CreateDirectory(directory.FullName);
        return directory.GetDirectories();
    }

    public DirectoryInfo GetDirectory()
    {
        return new DirectoryInfo(GlobalVars.SaveDir + @"\Worlds\Static\" + this.Name + @"\");
    }

    public static string GetLastWorldName()
    {
        return (string)Engine.Config.Descendants("LastWorld").FirstOrDefault();
    }

    public override void ResolveReferences()
    {
        this.Population.ResolveReferences();
    }

    public static void CreateExpansionMaps(StaticWorld world)
    {
        throw new NotImplementedException();
        //(new List<Vector2>() { new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, 1) }).ForEach(n =>
        //{
        //    foreach (var mapdir in world.GetDirectory().GetDirectories("*.*", SearchOption.TopDirectoryOnly))
        //    {
        //        var c = mapdir.Name.Split('.');
        //        var coords = new Vector2(Convert.ToInt32(c[0]), Convert.ToInt32(c[1]));
        //        var newPos = coords + n;
        //        if (!world.Maps.ContainsKey(newPos))
        //            StaticMap.Create(world, newPos);
        //    }
        //});
    }

    public void Initialize()
    {
        this.PopulationManager.Initialize();
    }
    public IEnumerable<(string, Action)> GetGenerationTasks()
    {
        yield return ("Generating population", this.PopulationManager.Initialize);
    }

    public override void Draw(SpriteBatch sb, Camera cam)
    {
        foreach (var map in this.Maps)
        {
            map.GetThumb().Draw(sb, cam);
        }
    }
    public override void OnHudCreated(Hud hud)
    {
        var win = new Window(this.CreateUI()) { Movable = true, Closable = true };
        win.AutoSize = true;
        hud.AddButton(new IconButton()
        {
            HoverFunc = () => "World",
            LeftClickAction = () =>
            {
                win.Toggle();
            },
        });
        this.Map.OnHudCreated(hud);
    }

    GroupBox CreateUI()
    {
        var box = new GroupBox();
        var winPop = new Lazy<Window>(() => new Window(this.PopulationManager.Gui) { Title = "Population", Movable = true, Closable = true });
        var btnPop = new Button("Population").SetLeftClickAction(b => winPop.Value.Toggle());
        box.AddControls(btnPop);
        return box;
    }

    public override void OnTargetSelected(IUISelection info, ISelectable selected)
    {
        this.PopulationManager.OnTargetSelected(info, selected);
    }
    public override void OnTargetSelected(SelectionManager info, ISelectable selected)
    {
        this.PopulationManager.OnTargetSelected(info, selected);
    }
    static readonly string[] NameFirst = {"glory", "thunder", "realm", "world", "city", "town", "far", "outer", "rim", "border",
        "land", "ville", "honor", "elder", "rock", "stone", "wood", "gold", "silver", "iron", "vale", "spring", "lake",
        "high", "view", "mount", "valor", "thorn"};
    public static string GetRandomName()
    {
        var rand = new Random();
        var name = NameFirst.SelectRandom(rand) + NameFirst.SelectRandom(rand);
        return name.First().ToString().ToUpper() + name[1..];
    }

    //public override MapBase GetMap(int mapId) => this.Map;

    internal FrontierDef /*void*/ PlaceAtRandom(Actor actor)
    {
        return this.Space.PlaceAtRandom(actor);
    }
    public override /*FrontierDef*/ void PlaceAt(Entity entity, WorldSpacePosition pos)
    {
        /*return*/  this.Space.PlaceAt(entity, pos);
    }
    public override FrontierDef GetFrontierOf(Entity entity)
    {
        return this.Space.GetFrontier(entity)?.Def;
    }
}
