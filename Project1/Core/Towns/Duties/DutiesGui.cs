using Project1.Core.Entities.Actors;
using Project1.Core.Screens;
using Project1.Framework.UI;

namespace Project1.Core.Towns.Duties
{
    class DutiesGui : GroupBox
    {
        internal static DutiesGui Instance;
        internal DutiesGui(DutyRoster roster)
        {
            PanelLabeledNew panelMembers = new("Town Members");
            var tableBox = new GroupBox();
            var tableAuto = new TableScrollableCompact<Actor>(true)
                            .AddColumn(null, "Name", 100, o => new Label(o.Name));
            var tableManual = new TableScrollableCompact<Actor>(true)
                           .AddColumn(null, "Name", 100, o => new Label(o.Name));
            foreach (var duty in roster.Provider.AvailableDuties)
            {
                var ic = duty.Icon;

                var icon = new PictureBox(ic.SpriteSheet, ic.SourceRect) { HoverText = duty.Name };
                var iconManual = new PictureBox(ic.SpriteSheet, ic.SourceRect) { HoverText = duty.Name };

                tableAuto.AddColumn(duty, icon, CheckBoxFinalNew.DefaultBounds.Width, (actor) =>
                {
                    var job = roster.Roster[actor].Duties[duty];
                    var ch = new CheckBoxFinalNew(() => Ingame.Instance.Events.Post(new PlayerDutyToggleEvent(actor, duty)), () => job.Enabled);
                    ch.InvalidateOn(job);
                    return ch;
                }, 0);
                tableManual.AddColumn(duty, iconManual, CheckBoxFinalNew.DefaultBounds.Width, (actor) =>
                {
                    var job = roster.Roster[actor].Duties[duty];
                    var btn = new ButtonFinal(CheckBoxFinalNew.CheckedRegion.Width)
                    {
                        TextFunc = () => job.Enabled ? job.Priority.ToString() : "",
                        LeftClickAction = () => Ingame.Instance.Events.Post(new PlayerDutyAdjustPriorityEvent(actor, duty, +1)),
                        RightClickAction = () => Ingame.Instance.Events.Post(new PlayerDutyAdjustPriorityEvent(actor, duty, -1)),
                        HoverText = job.Def.LabelReadable
                    };
                    btn.InvalidateOn(job);
                    return btn;
                }, 0);
            }
            var actors = roster.Roster.Keys;
            tableAuto.AddItems(actors);
            tableManual.AddItems(actors);

            var currentTable = tableAuto;

            tableBox.AddControls(currentTable);
            panelMembers.Client.AddControls(tableBox);
            var btnTogglePriorities = new CheckBoxFinalNew("Manual priorities", switchTables, () => currentTable == tableManual);
            this.AddControlsVertically(
                btnTogglePriorities,
                panelMembers);

            void switchTables()
            {
                tableBox.ClearControls();
                currentTable = currentTable == tableManual ? tableAuto : tableManual;
                tableBox.AddControls(currentTable);
            }
        }
    }
}
