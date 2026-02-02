using Microsoft.Xna.Framework;
using System;

namespace Start_a_Town_.UI
{
    class SearchBarNew<TObject> : TextBox
    {
        readonly IconButton IconClear = new(Icon.Cross) { BackgroundTexture = UIManager.Icon16Background };
        IListSearchable<TObject> List;
        readonly Func<TObject, string> Translator;
        public SearchBarNew(int width, Func<TObject, string> translator)
            : base(width)
        {
            this.Translator = translator;
            this.IconClear.LocationFunc = () => new Vector2(this.Width, this.Height / 2);
            this.IconClear.Anchor = new Vector2(1, .5f);
            this.IconClear.LeftClickAction = () =>
            {
                this.Text = "";
            };
        }
        public SearchBarNew<TObject> BindTo(IListSearchable<TObject> list)
        {
            this.List = list;
            return this;
        }
        protected override void OnTextChanged()
        {
            if (this.Text?.Length > 0 && !this.Controls.Contains(this.IconClear))
                this.AddControls(this.IconClear);
            else if (this.Text?.Length == 0 && this.Controls.Contains(this.IconClear))
                this.RemoveControls(this.IconClear);
            this.List?.Filter(o => this.Translator(o).ToLower().Contains(this.Text.ToLower()));
        }
    }
    class SearchBar<TObject> : TextBox where TObject : class, ILabeled
    {
        readonly IconButton IconClear = new(Icon.Cross) { BackgroundTexture = UIManager.Icon16Background };
        IListSearchable<TObject> List;
        public SearchBar(int width) 
            : base(width)
        {
            this.IconClear.LocationFunc = () => new Vector2(this.Width, this.Height / 2);
            this.IconClear.Anchor = new Vector2(1, .5f);
            this.IconClear.LeftClickAction = () =>
            {
                this.Text = "";
            };
        }
        public SearchBar<TObject> BindTo(IListSearchable<TObject> list)
        {
            this.List = list;
            return this;
        }
        protected override void OnTextChanged()
        {
            if (this.Text?.Length > 0 && !this.Controls.Contains(this.IconClear))
                this.AddControls(this.IconClear);
            else if (this.Text?.Length == 0 && this.Controls.Contains(this.IconClear))
                this.RemoveControls(this.IconClear);
            this.List?.Filter(o => o.Label.ToLower().Contains(this.Text.ToLower()));
        }
    }
}
