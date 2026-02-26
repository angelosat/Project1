using Project1.Core.Entities;
using Project1.Core.Screens;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using System;

namespace Project1.Core.UI.Hud
{
    public class SelectionMiniButtonDef(string name, Icon icon, string hoverText, Type workerType) : Def(name)
    {
        public readonly Icon Icon = icon;
        public readonly string HoverText = hoverText;
        public SelectionMiniButtonWorker Worker 
            => field ??= ActivatorSafe<SelectionMiniButtonWorker>.CreateInstance(workerType);
    }
    [EnsureStaticCtorCall]
    public static class SelectionMiniButtonDefOf
    {
        public static readonly SelectionMiniButtonDef CameraFollow = 
            new("CameraFollow", Icon.Replace, "Camera Follow", typeof(MiniButtonCameraFollow));
        public static readonly SelectionMiniButtonDef SlizeZ =
            new("SliceZlevel", Icon.ArrowDown, "Slice z-level", typeof(MiniButtonSlizeZ)); 
        public static readonly SelectionMiniButtonDef CameraCenter =
            new("CameraCenter", Icon.ArrowUp, "Camera Center", typeof(MiniButtonCameraCenter)); 
        public static readonly SelectionMiniButtonDef CycleSelection =
            new("CycleSelection", Icon.Replace, "Cycle Selection", typeof(MiniButtonCycleSelection)); 
        public static readonly SelectionMiniButtonDef Inspect =
            new("Inspect", Icon.ArrowRight, "Inspect", typeof(MiniButtonInspect));
        static SelectionMiniButtonDefOf()
        {
            Def.Register(typeof(SelectionMiniButtonDefOf));
        }
    }
    public abstract class SelectionMiniButtonWorker
    {
        internal abstract bool IsVisible(ISelectable selectable);
        internal abstract void OnClick(ISelectable selectable);
    }

    internal class MiniButtonCameraFollow : SelectionMiniButtonWorker
    {
        internal override bool IsVisible(ISelectable selectable)
            => selectable is Entity;

        internal override void OnClick(ISelectable selectable)
            => ScreenManager.CurrentScreen.Camera.ToggleFollowing(selectable as Entity);
    }
    internal class MiniButtonSlizeZ : SelectionMiniButtonWorker
    {
        internal override bool IsVisible(ISelectable selectable)
            => true;
        internal override void OnClick(ISelectable selectable)
            => ScreenManager.CurrentScreen.Camera.SliceOn((int)selectable.Global.Z);
    }
    internal class MiniButtonCameraCenter : SelectionMiniButtonWorker
    {
        internal override bool IsVisible(ISelectable selectable)
            => true;
        internal override void OnClick(ISelectable selectable)
            => ScreenManager.CurrentScreen.Camera.CenterOn(selectable.Global);
    }
    internal class MiniButtonCycleSelection : SelectionMiniButtonWorker
    {
        internal override bool IsVisible(ISelectable selectable)
            => true;
        internal override void OnClick(ISelectable selectable)
            => SelectionManager.Instance.CycleTargetsNew();
    }
    internal class MiniButtonInspect : SelectionMiniButtonWorker
    {
        internal override bool IsVisible(ISelectable selectable)
            => selectable is Inspectable;
        internal override void OnClick(ISelectable selectable)
        {
            var inspected = selectable as Inspectable;
            Inspector.Refresh(inspected);
            //Inspector.Show();
        }
    }
    internal class MiniButtonIssues : SelectionMiniButtonWorker
    {
        internal override bool IsVisible(ISelectable selectable)
            => throw new NotImplementedException();
        internal override void OnClick(ISelectable selectable)
            => throw new NotImplementedException();
    }
}
