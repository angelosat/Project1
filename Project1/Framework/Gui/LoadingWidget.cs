using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class LoadingWidgetNoBar : GroupBox
    {
        public Action Callback = () => { };
       
        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.Draw(sb);
        }
        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }
    }
    class LoadingWidget : GroupBox
    {
        readonly Label LblLoading, LblInfo;
        readonly Panel PanelBar;
        readonly Bar ProgressBar;
        readonly Progress MapLoadProgress;
        public Action Callback = () => { };

        public LoadingWidget(int barWidth)
        {
            this.MapLoadProgress = new Progress();
            this.PanelBar = new Panel() { AutoSize = true };
            this.ProgressBar = new Bar(this.MapLoadProgress) { Width = barWidth };
            this.PanelBar.Controls.Add(this.ProgressBar);
            this.PanelBar.LocationFunc = () => new Vector2((float)Math.Floor( this.LblInfo.BottomCenter.X), (float)Math.Floor(this.LblInfo.BottomCenter.Y));
            this.PanelBar.Anchor = new Vector2(.5f, 0);
            this.LblLoading = new Label("Please wait...")
            {
                LocationFunc = () => new Vector2((int)(this.PanelBar.Width * 0.5f), 0),
                Anchor = Vector2.UnitX * 0.5f
            };
            this.LblInfo = new Label() { AutoSize = true };
            this.LblInfo.LocationFunc = () => this.LblLoading.BottomCenter;
            this.LblInfo.Anchor = new Vector2(.5f, 0);
            this.Controls.Add(this.LblLoading, this.LblInfo, this.PanelBar);
            this.AutoSize = true;
        }
        public LoadingWidget(int barWidth, string initialMessage, float initialPercentage)
        {
            this.MapLoadProgress = new Progress() { Percentage = initialPercentage };
            this.LblLoading = new Label("Please wait...") { Location = new Vector2((int)(barWidth * 0.5f), 0) };
            this.LblLoading.Anchor = Vector2.UnitX * 0.5f;
            this.LblInfo = new Label(initialMessage);
            this.PanelBar = new Panel() { AutoSize = true };
            this.ProgressBar = new Bar(this.MapLoadProgress) { Width = barWidth };
            this.PanelBar.Controls.Add(this.ProgressBar);
            this.PanelBar.Location = new Vector2(0, this.LblInfo.Bottom);
            this.LblInfo.Location = new Vector2(this.PanelBar.Location.X + this.PanelBar.Width / 2, this.PanelBar.Location.Y);
            this.LblInfo.Anchor = new Vector2(0.5f, 1);
            this.Controls.Add(this.LblInfo, this.PanelBar);
        }
        public void Refresh(string text, float percentage)
        {
            this.LblInfo.Text = text;
            this.MapLoadProgress.Percentage = percentage;
            if (percentage < 1)
                return;
            this.Callback();
        }
    }
}
