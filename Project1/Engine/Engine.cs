using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_
{
    public class EngineArgs
    {
        public bool HideWalls, HideTerrain;
        EngineArgs()
        {
            this.HideWalls = Engine.HideWalls;
            this.HideTerrain = Engine.HideTerrain;
        }

        static public EngineArgs Default
        { get { return new EngineArgs(); } }
    }
    public class Engine
    {
        static Engine()
        {
            try { Config = XDocument.Load("config.xml"); }
            catch
            {
                GenerateDefaultConfigFile(Game1.Instance.graphics.GraphicsDevice);
                Config = XDocument.Load("config.xml");
            }
            _drawRooms = Config.GetOrCreateElement("Settings").GetOrCreateElement("Engine").GetOrCreateElement("DrawRooms", false);
        }
        public const int ChunkRadius = 2;
        public static XDocument Config;
        static public void SaveConfig()
        {
            Config.Save("config.xml");
        }
        static XmlDocument _settings;
        public static XmlDocument Settings => _settings ??= InitSettings();
       
        static MapBase _Map;
        static public MapBase Map
        {
            get => _Map;
            set => _Map = value;
        }
        static public bool HideWalls;
        static public bool HideTerrain;
        static public bool HideOccludedBlocks;
        static public bool MouseTooltip;
        public static bool DrawRegions;

        static XElement _drawRooms;
        public static bool DrawRooms
        {
            get => bool.Parse(_drawRooms.Value);
            set
            {
                _drawRooms.SetValue(value);
                SaveConfig();
            }
        }

        static XmlDocument InitSettings()
        {
            var settings = new XmlDocument();
            try 
            {
                settings.Load("config.xml"); 
            }
            catch (System.IO.FileNotFoundException)
            {
                GenerateDefaultConfigFile(Game1.Instance.graphics.GraphicsDevice);
                settings.Load("config.xml");
            }

            MouseTooltip = bool.Parse(settings["Settings"]["Interface"]["MouseTooltip"].InnerText);
            HideWalls = bool.Parse(settings["Settings"]["Engine"]["HideWalls"].InnerText);
            HideTerrain = bool.Parse(settings["Settings"]["Engine"]["HideTerrain"].InnerText);
            HideOccludedBlocks = false;
            DrawRooms = bool.Parse(settings["Settings"]["Engine"]["DrawRooms"]?.InnerText ?? bool.TrueString);
            return settings;
        }

        static void GenerateDefaultConfigFile(GraphicsDevice gfx)
        {
            var displayMode = gfx.Adapter.SupportedDisplayModes[SurfaceFormat.Color].First();

            new XDocument(
                new XElement("Settings",
                    new XElement("Video",
                        new XElement("Resolution",
                            new XElement("Width", displayMode.Width.ToString()),
                            new XElement("Height", displayMode.Height.ToString())
                        ),
                        new XElement("Fullscreen", false)
                    ),
                    new XElement("Engine",
                        new XElement("AsyncLoading", true),
                        new XElement("HideWalls", true),
                        new XElement("HideTerrain", true),
                        new XElement("DrawRooms", false)
                    ),
                    new XElement("Interface",
                        new XElement("MouseTooltip", true),
                        new XElement("Scale", 1)
                    ),
                    new XElement("Graphics",
                        new XElement("Particles", Particles.ParticleDensityLevel.Current.Name)
                    )
                )
            ).Save("config.xml");
        }

        public static XElement XmlNodeSettings => Config.GetOrCreateElement("Settings");

        static public void Init(Game1 game)
        {
            game.ApplyVideoSettings();
            game.graphics.ApplyChanges();
        }

        static public void PlayGame()
        {
            Nameplate.Reset();
            GC.Collect();
        }
    }
}
