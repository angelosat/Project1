using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class ButtonGridGenericNew<TagType> : SelectableItemList<TagType>
        where TagType : INamed
    {
        public ButtonGridGenericNew()
        {

        }
        public ButtonGridGenericNew(IEnumerable<TagType> items, Action<TagType, Button> btnInit)
        {
            this.AddItems(items, btnInit);
        }

        internal ButtonGridGenericNew<TagType> AddItems(IEnumerable<TagType> items, Action<TagType, Button> btnInit)
        {
            var w = Button.GetMaxWidth(items.Select(i => i.Name));
            var row = 0;
            var x = 0;
            var y = 0;
            var spacing = 1;
            var maxrows = Math.Ceiling(items.Count() / 2f);
            foreach (var item in items)
            {
                var button = new Button(item.Name, w) { Location = new Vector2(x, y)};
                btnInit(item, button);
                var prevAction = button.LeftClickAction;
                button.LeftClickAction = () =>
                { 
                    prevAction();
                    this.SelectAction(item);
                };
                this.AddControls(button);

                row++;
                if (row == maxrows)
                {
                    x += button.Width + spacing;
                    y = 0;
                    row = 0;
                }
                else
                    y += button.Height + spacing;
            }
            return this;
        }
    }
}
