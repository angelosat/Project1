using System;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class BuildToolDef : Def, INamed
    {
        readonly Type ToolClass;
        readonly Type ToolWorkerClass;
        //string IconTexturePath;
        public Icon Icon;

        public BuildToolDef(string name, string iconTexturePath, Type toolClass, Type toolWorkerClass) : base(name)
        {
            this.ToolClass = toolClass;
            this.ToolWorkerClass = toolWorkerClass;
            //this.IconTexturePath = iconTexturePath;
            this.Icon = new(UIManager.Atlas.Load(iconTexturePath));
        }

        public BuildToolDef(string name, Type toolClass, Type toolWorkerClass ) : base(name)
        {
            this.ToolClass = toolClass;
            this.ToolWorkerClass = toolWorkerClass;
        }

        internal ToolBlockBuild Create(Action<ToolBlockBuild.Args> callback)
        {
            var tool = (ToolBlockBuild)Activator.CreateInstance(this.ToolClass);
            tool.Callback = callback;
            tool.ToolDef = this;
            return tool;
        }
        BuildToolWorker _cachedWorker;
        public BuildToolWorker Worker => _cachedWorker ??= (BuildToolWorker)Activator.CreateInstance(this.ToolWorkerClass);

        string INamed.Name => this.Label;
    }
}
