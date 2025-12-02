using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    static class SaveFileManager
    {
        public static readonly ObservableCollection<SaveFile> SaveFiles = [];

        internal static void Refresh()
        {
            SaveFiles.Clear();
            var directory = new DirectoryInfo(SaveFile.SaveFolderFullPath);
            if (!Directory.Exists(directory.FullName))
                Directory.CreateDirectory(directory.FullName);
            var files = directory.GetFiles();
            var savefiles = files.Select(SaveFile.Load);
            var ordered = savefiles.OrderByDescending(s => s.Header.CreationTime);
            foreach (var s in ordered)
                SaveFiles.Add(s);
        }

        public static void Delete(SaveFile save)
        {
            SaveFiles.Remove(save);
            save.File.Delete();
        }
        static Control _guiCached;
        public static Control Gui => _guiCached ??= CreateGui();
        private static Control CreateGui()
        {
            var box = ScrollableBoxTest.FromContentsSize(SaveFile.Gui.Width, SaveFile.Gui.Width, ScrollModes.Vertical)
                .AddControls(new ListBoxObservable<SaveFile>(SaveFiles));
            return box.ToPanel();
        }

        public static void Load(SaveFile item)
        {
            Net.Server.Start();

            StaticMap map = null;
            DialogLoading.AddTask(
                "Loading map",
                () => map = item.World.Map);
            DialogLoading.AddTask(
                "Initializing undiscovered areas",
                () => map.InitUndiscoveredAreas()); // TEMP UNTIL I START SAVING IT
            DialogLoading.AddTask(
               "Initializing",
               () => map.Init());
            DialogLoading.Start(() =>
            {
                map.CameraRecenter();
                string localHost = "127.0.0.1";
                UIConnecting.Create(localHost);
                Net.Server.Instance.SetMap(map); // is this needed??? YES!!! it enumerates all existing entities in the network
                Net.Client.Instance.Connect(localHost, "host", a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); });
            });
        }

        internal static Control InitGui()
        {
            Refresh();
            return Gui;
        }
    }
}
