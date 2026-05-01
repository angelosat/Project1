using Project1.Core.Entities;
using Project1.Core.Screens;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using System;

namespace Project1.Core.UI.Hud;

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
        new("CameraFollow", Icon.Replace, "Camera Follow", typeof(MiniButton_CameraFollow));
    public static readonly SelectionMiniButtonDef SlizeZ =
        new("SliceZlevel", Icon.ArrowDown, "Slice z-level", typeof(MiniButton_SlizeZ)); 
    public static readonly SelectionMiniButtonDef CameraCenter =
        new("CameraCenter", Icon.ArrowUp, "Camera Center", typeof(MiniButton_CameraCenter)); 
    public static readonly SelectionMiniButtonDef CycleSelection =
        new("CycleSelection", Icon.Replace, "Cycle Selection", typeof(MiniButton_CycleSelection)); 
    public static readonly SelectionMiniButtonDef Inspect =
        new("Inspect", Icon.ArrowRight, "Inspect", typeof(MiniButton_Inspect));
    public static readonly SelectionMiniButtonDef DetachTooltip =
    new("Detach", Icon.ArrowUp, "Detach Tooltip", typeof(MiniButton_DetachTooltip));
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

internal class MiniButton_CameraFollow : SelectionMiniButtonWorker
{
    internal override bool IsVisible(ISelectable selectable)
        => selectable is Entity;

    internal override void OnClick(ISelectable selectable)
        => Ingame.MainView.ToggleFollow(selectable as Entity);
}
internal class MiniButton_SlizeZ : SelectionMiniButtonWorker
{
    internal override bool IsVisible(ISelectable selectable)
        => true;
    internal override void OnClick(ISelectable selectable)
        => Ingame.MainView.SliceOn((int)selectable.Global.Z);
}
internal class MiniButton_CameraCenter : SelectionMiniButtonWorker
{
    internal override bool IsVisible(ISelectable selectable)
        => true;
    internal override void OnClick(ISelectable selectable)
        => Ingame.MainView.CenterOn(selectable.Global);
}
internal class MiniButton_CycleSelection : SelectionMiniButtonWorker
{
    internal override bool IsVisible(ISelectable selectable)
        => true;
    internal override void OnClick(ISelectable selectable)
        => SelectionManager.Instance.CycleTargetsNew();
}
internal class MiniButton_Inspect : SelectionMiniButtonWorker
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
internal class MiniButton_DetachTooltip : SelectionMiniButtonWorker
{
    internal override bool IsVisible(ISelectable selectable)
        => selectable is ITooltippable;
    internal override void OnClick(ISelectable selectable)
    {
        TooltipManager.Detach(selectable as ITooltippable);
    }
}
internal class MiniButtonIssues : SelectionMiniButtonWorker
{
    internal override bool IsVisible(ISelectable selectable)
        => throw new NotImplementedException();
    internal override void OnClick(ISelectable selectable)
        => throw new NotImplementedException();
}
