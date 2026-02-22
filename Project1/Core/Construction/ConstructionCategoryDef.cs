using Project1.Core.Blocks;
using Project1.Core.Construction.Packets;
using Project1.Core.Construction.Tools;
using Project1.Core.Input.Building;
using Project1.Core.Networking;
using Project1.Framework.UI;

namespace Project1.Core.Construction
{
    public sealed class ConstructionCategoryDef : Def//, INamed
    {
        public readonly BuildToolDef[] Tools;

        public ConstructionCategoryDef(string name, BuildToolDef[] tools) : base(name)
        {
            this.Tools = tools;
        }

        static public Window WindowToolsBox;
        static public UIToolsBox ToolsBox;

        internal ToolBlockBuild GetTool(BuildToolDef toolDef, ConstructionDesignationArgs args, byte data = 0)
        {
            var tool = toolDef.Create(a => PacketDesignateConstruction.Send(Client.Instance, a, args)); // TODO improve
            tool.Block = args.Block.Block;
            tool.Material = args.Material;
            tool.State = data;
            return tool;
        }
    }
}
