using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public static class UIHelper
    {
        public static void DrawHighlight(this Rectangle bounds, SpriteBatch sb, Vector2 origin, float rotation, float alpha = 0.5f)
        {
            bounds.DrawHighlight(sb, Color.Lerp(Color.Transparent, Color.White, alpha), origin, rotation);
        }
        public static void DrawHighlight(this Rectangle bounds, SpriteBatch sb, Color color, Vector2 origin, float rotation)
        {
            sb.Draw(UI.UIManager.Highlight, bounds, null, color, rotation, origin, SpriteEffects.None, 0);
        }
        public static void DrawHighlight(this Vector4 bounds, SpriteBatch sb, Color color, Vector2 origin, float rotation)
        {
            sb.Draw(UI.UIManager.Highlight, new Vector2(bounds.X, bounds.Y), null, color, rotation, origin, new Vector2(bounds.Z, bounds.W), SpriteEffects.None, 0);
        }
        public static void DrawHighlight(this Rectangle bounds, SpriteBatch sb, float alpha = 0.5f)
        {
            bounds.DrawHighlight(sb, Vector2.Zero, 0, alpha);
        }
        public static void DrawHighlight(this Rectangle bounds, SpriteBatch sb, Color color)
        {
            sb.Draw(UI.UIManager.Highlight, bounds, null, color, 0, Vector2.Zero, SpriteEffects.None, 0);
        }
        public static void DrawHighlightBorder(this Rectangle bounds, SpriteBatch sb, float alpha = .5f, float thickness = 1, int padding = 0)
        {
            bounds.DrawHighlightBorder(sb, Color.White * alpha, Vector2.Zero, thickness, padding);
        }
        public static void DrawHighlightBorder(this Rectangle bounds, SpriteBatch sb, Color color, float thickness = 1)
        {
            bounds.DrawHighlightBorder(sb, color, Vector2.Zero, thickness);
        }
        public static void DrawHighlightBorder(this Rectangle bounds, SpriteBatch sb)
        {
            bounds.DrawHighlightBorder(sb, Color.White, Vector2.Zero);
        }
        public static void DrawHighlightBorder(this Rectangle bounds, SpriteBatch sb, Color color, Vector2 origin, float thickness = 1, int padding = 0)
        {
            var intthickness = (int)Math.Max(1, thickness);
            var padpad = 2 * padding;
            // Draw top line
            sb.Draw(UIManager.Highlight, new Rectangle(bounds.X - padding + intthickness, bounds.Y - padding, bounds.Width + padpad - intthickness, intthickness), color);

            // Draw left line
            sb.Draw(UIManager.Highlight, new Rectangle(bounds.X - padding, bounds.Y - padding, intthickness, bounds.Height + padpad - intthickness), color);

            // Draw bottom line
            sb.Draw(UIManager.Highlight, new Rectangle(bounds.X - padding,
                                            bounds.Y + padding + bounds.Height - intthickness,
                                            bounds.Width - intthickness + padpad,
                                            intthickness), color);

            // Draw right line
            sb.Draw(UIManager.Highlight, new Rectangle(bounds.X + padding + bounds.Width - intthickness,
                                            bounds.Y - padding + intthickness,
                                            intthickness,
                                            bounds.Height - intthickness + padpad), color);
        }
        public static void DrawFlashingBorder(this Rectangle bounds, SpriteBatch sb)
        {
            //var lerp = (float)Math.Cos(Math.PI * 2 * UIManager.FlashingTimer / 120f);
            //lerp += 1;
            //lerp /= 2;
            float lerp = UIManager.FlashingValue;
            bounds.DrawHighlightBorder(sb, Color.Lerp(Color.Transparent, Color.White, lerp), Vector2.Zero);
        }

        public static Label ToLabel(this string text) { return new UI.Label(Vector2.Zero, text); }
        public static Label ToLabel(this string text, Vector2 location) { return new UI.Label(location, text); }
        public static Label ToLabel(this string text, Vector2 location, int width) { return new UI.Label(location, text) { Width = width }; }

        static public Panel ToPanel(this Control ctrl)
        {
            return ctrl.ToPanel(Panel.DefaultStyle);
        }
        static public Panel ToPanel(this Control ctrl, BackgroundStyle panelBackgroundStyle, float opacity = 1)
        {
            var panel = new Panel() { AutoSize = true, BackgroundStyle = panelBackgroundStyle, Opacity = opacity };
            panel.AddControls(ctrl);
            return panel;
        }

        static public PanelScrollable ToPanelScrollable(this Control ctrl, ScrollModes mode = ScrollModes.Both)
        {
            return ctrl.ToPanelScrollable(mode, Panel.DefaultStyle);
        }
        static public PanelScrollable ToPanelScrollable(this Control ctrl, ScrollModes mode, BackgroundStyle panelBackgroundStyle, float opacity = 1)
        {
            var panel = new PanelScrollable(ctrl, mode) { AutoSize = true, BackgroundStyle = panelBackgroundStyle, Opacity = opacity };
            return panel;
        }
        static public PanelLabeledNew ToPanelLabeled(this Control ctrl)
        {
            var panel = new PanelLabeledNew(ctrl.Name) { AutoSize = true };
            panel.Client.AddControls(ctrl);
            return panel;
        }
        static public PanelLabeledNew ToPanelLabeled(this Control ctrl, string label)
        {
            var panel = new PanelLabeledNew(label) { AutoSize = true };
            panel.Client.AddControls(ctrl);
            return panel;
        }
        static public PanelLabeledNew ToPanelLabeled(this Control ctrl, Func<string> label)
        {
            var panel = new PanelLabeledNew(label) { AutoSize = true };
            panel.Client.AddControls(ctrl);
            return panel;
        }
        
        static public Control HideOnRightClick(this Control control)
        {
            control.MouseRBAction = () => { if (!control.ContainsMouse()) control.Hide(); };
            return control;
        }
        static public Control HideOnLeftClick(this Control control)
        {
            control.MouseLBAction = () => { if (!control.ContainsMouse()) control.Hide(); };
            return control;
        }
        static public Control HideOnAnyClick(this Control control)
        {
            control.HideOnLeftClick();
            control.HideOnRightClick();
            return control;
        }
        static public Control ToContextMenu(this Control control, string title)
        {
            return control.ToPanelLabeled(title).SetLocation(UIManager.Mouse).HideOnAnyClick();
        }
        static public Control ToContextMenuClosable(this Control control, string title)
        {
            return control.ToWindow(title, movable: false).SetLocation(UIManager.Mouse).HideOnAnyClick();
        }
        static public Control ToContextMenuClosable(this Control control, string title, Vector2 screenLoc)
        {
            return control.ToWindow(title, movable: false).SetLocation(screenLoc).HideOnAnyClick();
        }
        static public GroupBox Wrap(params ButtonBase[] controls)
        {
            return Wrap(controls as IEnumerable<ButtonBase>);
        }
        static public GroupBox Wrap(int width, params ButtonBase[] controls)
        {
            return Wrap(controls, width);
        }
        static public GroupBox Wrap(IEnumerable<ButtonBase> labels, int width = int.MaxValue)
        {
            var box = new GroupBox();
            var currentX = 0;
            var currentY = 0;
            foreach (var l in labels)
            {
                if (currentX + l.Width > width)
                {
                    currentX = 0;
                    currentY += l.Height;
                }
                l.Location = new IntVec2(currentX, currentY);
                currentX += l.Width;
                box.AddControls(l);
            }
            if (width != int.MaxValue)
                box.Width = width;
            return box;
        }
        static public GroupBox ToGroupBoxVertically<T>(this IEnumerable<T> controls, int spacing = 0) where T : Control
        {
            return new GroupBox().AddControlsVertically(spacing, controls.ToArray()) as GroupBox;
        }
        static public GroupBox ToGroupBoxHorizontally<T>(this IEnumerable<T> controls) where T : Control
        {
            return new GroupBox().AddControlsHorizontally(controls) as GroupBox;
        }
        static public GroupBox ToGroupBoxHorizontally(params Control[] controls)
        {
            return new GroupBox().AddControlsHorizontally(controls) as GroupBox;
        }
        static public Control ToTabbedContainer(params Control[] namedControls)
        {
            var box = new GroupBox();

            var labeledPanels = namedControls.Select(c =>
            {
                if (c.Name.IsNullEmptyOrWhiteSpace())
                    throw new ArgumentException($"Control's \"Name\" field is null empty or whitespace");
                return c.ToPanelLabeled(c.Name);
            });

            var maxw = labeledPanels.Max(c => c.Width);
            var maxh = labeledPanels.Max(c => c.Height);

            var boxclient = new GroupBox(maxw, maxh);

            var boxtabs = Wrap(labeledPanels.Select(c => new Button(c.Name, () => select(c))), maxw);

            void select(Control c)
            {
                boxclient.ClearControls();
                boxclient.AddControls(c);
            }

            box.AddControlsVertically(boxtabs, boxclient);
            return box;
        }

        public static ListBoxObservable<T> GetListObservableControl<T>(this ObservableCollection<T> boundCollection) where T : class, IListable
        {
            return new ListBoxObservable<T>(boundCollection);
        }
    }
}