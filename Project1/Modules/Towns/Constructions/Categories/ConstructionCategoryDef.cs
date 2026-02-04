using System.Collections.Generic;
using Start_a_Town_.UI;
using Project1.Framework.Input.Tools.Building;
using Project1.Framework.Net;
using Project1.Framework.Base;
using Project1.Core.Construction.Packets;
using Project1.Core.Construction.Tools;
using Project1.Framework.Interfaces;

namespace Start_a_Town_
{
    public sealed class ConstructionCategoryDef : Def, INamed
    {
        BuildToolDef[] _toolDefs;

        public ConstructionCategoryDef(string name, params BuildToolDef[] tools) : base(name)
        {
            this._toolDefs = tools;
        }

        public IEnumerable<BuildToolDef> Tools => this._toolDefs;
     
        string INamed.Name => this.Label;


        static public Window WindowToolsBox;
        static public UIToolsBox ToolsBox;

        internal ToolBlockBuild GetTool(BuildToolDef toolDef, ConstructionDesignationArgs args, byte data = 0)
        {
            var tool = toolDef.Create(a => PacketDesignateConstruction.Send(Client.Instance, a, args)); // TODO improve
            tool.Block = args.Block.Worker;
            tool.Material = args.Material;
            tool.State = data;
            return tool;
        }
    }
}
