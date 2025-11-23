using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class DialogLoading : Window
    {
        static Queue<(string Label, Action Task)> LoadingTasks = new Queue<(string, Action)>();
        static public void AddTask(string label, Action action)
        {
            // add failsafe in case a task is added while currently loading
            LoadingTasks.Enqueue((label, action));
        }
        static public void Start(Action callback)
        {
            var loadingDialog = new DialogLoading();
            loadingDialog.ShowDialog();
            var count = LoadingTasks.Count;
            Task.Factory.StartNew(() =>
            {
                var i = 0;
                while(LoadingTasks.Any())
                {
                    var task = LoadingTasks.Dequeue();
                    loadingDialog.Refresh(string.Format(task.Label, i, count), i / (float)count);
                    i++;
                    task.Task();
                }
                loadingDialog.Close();
                callback();
            });
        }
        LoadingWidget Widget;

        public DialogLoading() : this(() => { }) { }
        public DialogLoading(Action callback) : this("Please wait...", callback) { }
        public DialogLoading(string text) : this(text, () => { }) { }
        public DialogLoading(string text, Action callback)
        {
            this.Title = text;
            this.Movable = false;
            this.AutoSize = true;
            this.Closable = false;

            this.Widget = new LoadingWidget(200) { Callback = callback };
            this.Client.Controls.Add(this.Widget);

            //this.AnchorToScreenCenter();
            //this.Anchor = Vector2.One * .5f;
        }
        public DialogLoading(string title, string message, float percentage)
        {
            this.Title = title;
            this.Movable = false;
            this.AutoSize = true;
            this.Closable = false;

            this.Widget = new LoadingWidget(200, message, percentage);
            this.Client.Controls.Add(this.Widget);

            //this.SnapToScreenCenter();
            //this.Anchor = Vector2.One * .5f;
            this.AutoSize = false;
        }

        public void Refresh(string text, float percentage)
        {
            this.Widget.Refresh(text, percentage);
        }
    }
}
