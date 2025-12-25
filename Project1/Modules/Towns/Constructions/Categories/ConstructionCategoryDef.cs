using System;
using System.Collections.Generic;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

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

        static void CallBack(Func<ProductMaterialPair> itemGetter, ToolBlockBuild.Args a)
        {
            PacketDesignateConstruction.Send(Client.Instance, itemGetter(), a);
        }

        static public Window WindowToolsBox;
        static public UIToolsBox ToolsBox;

        internal ToolBlockBuild GetTool(BuildToolDef toolDef, ProductMaterialPair productMaterialPair)
        {
            var tool = toolDef.Create(a => CallBack(() => productMaterialPair, a)); // TODO improve
            tool.Block = productMaterialPair.Block;
            tool.Material = productMaterialPair.Material;
            tool.State = productMaterialPair.Data;
            return tool;
        }
        internal ToolBlockBuild GetTool(BuildToolDef toolDef, ConstructionDesignationArgs args, byte data = 0)
        {
            var tool = toolDef.Create(a => PacketDesignateConstruction.Send(Client.Instance, a, args)); // TODO improve
            tool.Block = args.Block;
            tool.Material = args.Material;
            tool.State = data;
            return tool;
        }
    }
}
