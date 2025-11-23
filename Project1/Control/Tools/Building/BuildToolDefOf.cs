namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class BuildToolDefOf
    {
        public static readonly BuildToolDef Single = new("Single", "Graphics/Gui/toolsingle", typeof(ToolBuildSingle), typeof(BuildToolWorkerSingle));
        public static readonly BuildToolDef SinglePreview = new("SinglePreview", "Graphics/Gui/toolsingle", typeof(ToolBuildSinglePreview), typeof(BuildToolWorkerSingle));
        public static readonly BuildToolDef Line = new("Line", "Graphics/Gui/toolline", typeof(ToolBuildLine), typeof(BuildToolWorkerLine));
        public static readonly BuildToolDef Floor = new("Floor", "Graphics/Gui/toolfloor", typeof(ToolBuildFloor), typeof(BuildToolWorkerFloor));
        public static readonly BuildToolDef Wall = new("Wall", "Graphics/Gui/toolwall", typeof(ToolBuildWall), typeof(BuildToolWorkerWall));
        public static readonly BuildToolDef Enclosure = new("Enclosure", "Graphics/Gui/toolenclosure", typeof(ToolBuildEnclosure), typeof(BuildToolWorkerEnclosure));
        public static readonly BuildToolDef Box = new("Box", "Graphics/Gui/toolboxhollow", typeof(ToolBuildBox), typeof(BuildToolWorkerBox));
        public static readonly BuildToolDef BoxFilled = new("BoxFilled", "Graphics/Gui/toolboxfilled", typeof(ToolBuildBoxFilled), typeof(BuildToolWorkerBoxFilled));
        public static readonly BuildToolDef Roof = new("Roof", "Graphics/Gui/toolroof", typeof(ToolBuildRoof), typeof(BuildToolWorkerRoof));
        public static readonly BuildToolDef Pyramid = new("Pyramid", "Graphics/Gui/toolpyramid", typeof(ToolBuildPyramid), typeof(BuildToolWorkerPyramid));
        static BuildToolDefOf()
        {
            Def.Register(typeof(BuildToolDefOf));
        }
    }
}
