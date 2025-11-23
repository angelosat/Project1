using System;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class WindowSetStackSize : Window
    {
        TextBox TextBox;
        Action<int> Callback;
        public WindowSetStackSize(int initial, Action<int> callback)
        {
            this.AutoSize = true;
            this.Title = "Set stack size";
            this.TextBox = new TextBox(100) { Text = initial.ToString() };
            var btnok = new Button("Done") { Location = this.TextBox.BottomLeft, LeftClickAction = Done };
            this.Callback = callback;
            this.Client.AddControls(this.TextBox, btnok);
        }
        public WindowSetStackSize(Action<int> callback)
            : this(1, callback)
        {
        }

        private void Done()
        {
            var txt = this.TextBox.Text;
            if (!int.TryParse(txt, out var amount))
                return;
            this.Callback(amount);
            this.Hide();
        }
    }
}
