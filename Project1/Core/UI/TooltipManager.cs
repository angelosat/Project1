using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Input;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.UI.Settings;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Input;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.UI;

public class TooltipManager
{
    public static bool MouseTooltips = true;
    public static float DelayInterval = Ticks.PerSecond / 5;
    float DelayValue;
    public static int Width = 300;
    protected static TooltipManager _Instance;
    public static TooltipManager Instance => _Instance ??= new TooltipManager();

    Tooltip _tooltip;
    public Tooltip Tooltip => this._tooltip;

    void SetTooltip(Tooltip tooltip)
    {
        if (ReferenceEquals(this._tooltip, tooltip))
            return;

        this._tooltip?.Dispose();

        this._tooltip = tooltip;
        this._tooltip?.OnAttached();
    }

    ITooltippable Object;


    public void Update()
    {
        if (this.Object is null)
            return;

        this.DelayValue -= 1;

        if (this.DelayValue <= 0)
        {
            this.DelayValue = DelayInterval;
            if (this.Tooltip is null)
                this.Build();
        }
        this.Tooltip?.Update();
    }

    void Build()
    {
        var tooltip = Build(this.Object);

        if (tooltip.Controls.Count > 0)
        {
            tooltip.Update();
            tooltip.SetMousethrough(true, true);
            this.SetTooltip(tooltip);
        }
        else
        {
            this.SetTooltip(null);
            tooltip.Dispose();
        }
    }
    static Tooltip Build(ITooltippable source)
    {
        var item = source.GetTooltipTarget();
        var tooltip = new Tooltip(item)
        {
            AutoSize = true
        };
        if (!string.IsNullOrEmpty(item.LabelReadable))
            tooltip.Controls.Add(new Label(item.LabelReadable));
        foreach (var ctrl in item.GetTooltipControls())
            tooltip.AddControlsBottomLeft(ctrl);
        foreach (var provider in TooltipProviders)
            foreach (var ctrl in provider(item))
                tooltip.AddControlsBottomLeft(ctrl);
        foreach (var comp in Game1.Instance.GameComponents)
            comp.OnTooltipCreated(item, tooltip);
        if(source != item)
            foreach (var ctrl in source.GetTooltipControls())
                tooltip.AddControlsBottomLeft(ctrl);
        if (item.TooltipColor.HasValue)
            tooltip.Color = item.TooltipColor.Value;
        return tooltip;
    }

    void Object_TooltipChanged(object sender, EventArgs e)
    {
        this.Build();
    }

    TooltipManager()
    {
        Controller.MouseoverObjectChanged += new EventHandler<MouseoverEventArgs>(this.Controller_MouseoverObjectChanged);

        if (!bool.TryParse(InterfaceSettings.XMouseTooltip.Value, out MouseTooltips))
            MouseTooltips = true;
    }

    void Controller_MouseoverObjectChanged(object sender, MouseoverEventArgs e)
    {
        this.Reset();
        this.Object = e.ObjectNext as ITooltippable;
    }

    private void Reset()
    {
        this.SetTooltip(null);
        this.DelayValue = DelayInterval;
    }

    public void Draw(SpriteBatch sb)
    {
        this.Tooltip?.Draw(sb);
    }

    internal static void OnGameEvent(GameEvent e)
    {
        switch (e.Type)
        {
            default:
                Instance.Tooltip?.OnGameEvent(e);
                break;
        }
    }

    internal static void Bind(NetEndpoint net)
    {
        net.Events.ListenTo<CellsInvalidatedEvent>(HandleBlocksChanged);
    }
    static void HandleBlocksChanged(CellsInvalidatedEvent e)
    {
        var map = e.Map;
        var cells = e.Positions;
        if (Ingame.Net.MainView.Map == map)
            if (Instance.Object is InteractionTarget target && target.Type == TargetType.Cell && cells.Contains((IntVec3)target.Global))
                Instance.Reset();
    }

    internal static void Detach(ITooltippable tooltippable)
    {
        UIManager.ToggleUnique<TooltipNew>(tooltippable as ISelectable);
    }

    internal static void RegisterProvider(Func<ITooltippable, IEnumerable<Control>> provider)
    {
        TooltipProviders.Add(provider);
    }

    static readonly List<Func<ITooltippable, IEnumerable<Control>>> TooltipProviders = [];
}
