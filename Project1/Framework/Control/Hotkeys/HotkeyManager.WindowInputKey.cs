using System;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    partial class HotkeyManager
    {
        class WindowInputKey : Panel
        {
            string ActionLabel;
            Action<System.Windows.Forms.Keys> CallBack;

            public WindowInputKey()
            {
                this.Width = 256;
                this.Height = 64;
                this.Color = UIManager.TintPrimary;
                var label = new Label(() => $"Rebind key for: [{this.ActionLabel}]") { AutoSize = true };
                var boxLabels = new GroupBox(this.Width - 2 * this.Padding, Label.DefaultHeight * 2);
                boxLabels.AddControlsVertically(0, Alignment.Horizontal.Center,
                    label, 
                    new Label("ESC to unbind")
                    );
                this.AddControls(boxLabels, IconButton.CreateCloseButtonNew());
                boxLabels.AnchorToParentCenter();
            }

            public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
            {
                var key = e.KeyCode;
                this.CallBack(key == System.Windows.Forms.Keys.Escape ? 0 : key);
                this.Hide();
            }
            public void EditHotkey(string actionLabel, Action<System.Windows.Forms.Keys> callBack)
            {
                this.ActionLabel = actionLabel;
                this.CallBack = callBack;
                this.ShowDialog();
            }
        }
    }
}
