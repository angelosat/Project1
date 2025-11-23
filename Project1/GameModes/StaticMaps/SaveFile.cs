using System;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class SaveFile : IListable
    {
        public static readonly string SaveFolderFullPath = Directory.GetCurrentDirectory() + $"/Saves/Worlds/";

        public HeaderInfo Header;

        public FileInfo File;

        StaticWorld _world;
        public StaticWorld World => _world ??= this.ReadTag();

        public string Label => this.File.Name;

        long tagReaderPosition;

        StaticWorld ReadTag()
        {
            using var stream = new FileStream(this.File.FullName, FileMode.Open);
            using var decompressedStream = Chunk.Decompress(stream);
            using var reader = new BinaryReader(decompressedStream);
            reader.BaseStream.Position = tagReaderPosition;
            var worldTag = SaveTag.ReadWithRefs(reader);
            return new StaticWorld(worldTag["World"]);
        }

        internal static void Load(FileInfo file, out StaticMap map)
        {
            using var stream = new FileStream(file.FullName, FileMode.Open);
            using var decompressedStream = Chunk.Decompress(stream);
            using var reader = new BinaryReader(decompressedStream);
            var worldTag = SaveTag.ReadWithRefs(reader);
            var world = new StaticWorld(worldTag["World"]);
            map = world.Map;
        }

        internal static SaveFile Load(FileInfo file)
        {
            using var stream = new FileStream(file.FullName, FileMode.Open);
            using var decompressedStream = Chunk.Decompress(stream);
            using var reader = new BinaryReader(decompressedStream);
            var save = new SaveFile();
            save.File = file;
            save.Header = HeaderInfo.Read(reader);
            save.tagReaderPosition = reader.BaseStream.Position;
            return save;
        }

        internal static void Save(SaveTag tag, string name)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            HeaderInfo.Write(writer);
            tag.WriteWithRefs(writer);
            //var fullPath = Directory.GetCurrentDirectory() + $"/Saves/Worlds/{name}.sat";
            var fullPath = SaveFolderFullPath + $"{name}.sat";

            Chunk.Compress(stream, fullPath);
            stream.Close();
        }

        static public void Delete(FileInfo save, Action callback = null)
        {
            var msgbox = new MessageBox("", $"Delete {save.Name}?", () => delete(save, callback));
            msgbox.ShowDialog();

            static void delete(FileInfo save, Action callback)
            {
                save.Delete(); 
                callback?.Invoke();
            }
        }

        static SaveFile()
        {
            Gui.Width = 320;
        }
        public static GuiInfo Gui;
        public struct GuiInfo
        {
            public int Width;
        }
        public Control GetListControlGui()
        {
            var btn = ButtonNew.CreateBig(null, Gui.Width, () => Path.GetFileNameWithoutExtension(this.File.Name), () => this.Header.Version + " - " + this.Header.CreationTime.ToString("R"))
                .SetLeftClickAction(load);
            btn.AddControls(IconButton.CreateCloseButton().SetLeftClickAction(b => showDeleteDialogue(this)).SetLocation(btn.TopRight + new Vector2(- btn.Padding,btn.Padding)).SetAnchor(1, 0).ShowOnParentFocus(true));
            return btn;
            void showDeleteDialogue(SaveFile save)
            {
                var msgbox = new MessageBox("", $"Delete {save.File.Name}?", () => SaveFileManager.Delete(save));
                msgbox.ShowDialog();
            }
            void load(ButtonBase b)
            {
                SaveFileManager.Load(this);
                b.TopLevelControl.Hide();
            }
        }

        public struct HeaderInfo
        {
            public string Version;
            public DateTime CreationTime;

            HeaderInfo(string version, DateTime creationTime)
            {
                this.Version = version;
                this.CreationTime = creationTime;
            }

            public static void Write(BinaryWriter w)
            {
                var tag = new SaveTag(SaveTag.Types.Compound, "Header");
                GlobalVars.Version.Save(tag, "Version");
                DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo).Save(tag, "CreationTime");
                tag.WriteTo(w);
            }
            public static HeaderInfo Read(BinaryReader r)
            {
                var header = SaveTag.Read(r);
                var version = (string)header["Version"].Value;
                var datetime = DateTime.Parse((string)header["CreationTime"].Value, DateTimeFormatInfo.InvariantInfo);
                return new(version, datetime);
            }
        }
    }
}
