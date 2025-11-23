using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    static public class RectangleHelper
    {
        public static Rectangle FindBestUncoveredRectangle(this Rectangle container, List<Rectangle> existingRectangles, int w, int h)
        {
            var freeRects = GetFreeRectangles(container, existingRectangles);
            var bestRect = new Rectangle(0, 0, w, h);
            var ordered = freeRects.OrderBy(r => new Vector2(r.X, r.Y).LengthSquared()).ToList();
            foreach (var rect in ordered)
            {
                if (w <= rect.Width && h <= rect.Height)
                    return rect;
            }
            return bestRect;
        }
        static List<Rectangle> GetFreeRectangles(Rectangle container, List<Rectangle> children)
        {
            var freeRects = new List<Rectangle>() { container };
            foreach (var child in children) // TODO: fix HUD being a control itself
            {
                foreach (var rect in freeRects.ToList())
                {
                    if (rect.Intersects(child))
                    {
                        freeRects.Remove(rect);
                        var divisions = DivideScreenQuad(rect, child);
                        foreach (var div in divisions)
                            if (!freeRects.Contains(div))
                                freeRects.Add(div);
                    }
                }
            }
            return freeRects;
        }
        static public Rectangle FindBestUncoveredRectangle(this IBoundedCollection container, int w, int h)
        {
            var freeRects = GetFreeRectangles(container);
            Rectangle bestRect = new(0, 0, w, h);
            var size = w * h;
            var ordered = freeRects.OrderBy(r => new Vector2(r.X, r.Y).LengthSquared()).ToList();
            foreach (var rect in ordered)
            {
                if (w <= rect.Width && h <= rect.Height)
                    return rect;
            }
            return bestRect;
        }
        static List<Rectangle> GetFreeRectangles(IBoundedCollection container)
        {
            List<Rectangle> freeRects = new() { container.ContainerSize };
            var divided = true;
            var windows = container.Children;
            windows.Reverse();
            while (divided)
            {
                divided = false;

                foreach (var control in windows) // TODO: fix HUD being a control itself
                {
                    foreach (var rect in freeRects.ToList())
                    {
                        var bounds = control.Bounds;
                        if (rect.Intersects(bounds))
                        {
                            freeRects.Remove(rect);
                            var divisions = DivideScreenQuad(rect, bounds);
                            foreach (var div in divisions)
                                if (!freeRects.Contains(div))
                                    freeRects.Add(div);
                        }
                    }
                }
            }
            return freeRects;
        }

        static List<Rectangle> DivideScreenQuad(Rectangle screen, Rectangle rect)
        {
            var right = rect.X + rect.Width;
            var bottom = rect.Y + rect.Height;
            var dx = screen.X + screen.Width - right;
            var dy = screen.Y + screen.Height - bottom;

            var list = new List<Rectangle>();
            if (rect.Y > screen.Y)
                list.Add(new Rectangle(screen.X, screen.Y, screen.Width, rect.Y - screen.Y));
            if (bottom < screen.Y + screen.Height)
                list.Add(new Rectangle(screen.X, bottom, screen.Width, dy));
            if (rect.X > screen.X)
                list.Add(new Rectangle(screen.X, screen.Y, rect.X - screen.X, screen.Height));
            if (right < screen.X + screen.Width)
                list.Add(new Rectangle(right, screen.Y, dx, screen.Height));

            return list;
        }

        public static Rectangle ToRectangle(this Vector4 bounds)
        {
            return new Rectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Z, (int)bounds.W);
        }
        static public Rectangle GetRectangle(this Vector2 vec1, Vector2 vec2)
        {
            int xm = (int)Math.Min(vec1.X, vec2.X);
            int ym = (int)Math.Min(vec1.Y, vec2.Y);

            int xM = (int)(vec1.X + vec2.X - xm);
            int yM = (int)(vec1.Y + vec2.Y - ym);

            return new Rectangle(xm, ym, xM - xm, yM - ym);
        }
        public static List<Rectangle> Divide(this Rectangle rect, int count)
        {
            var list = new List<Rectangle>();
            var sqrt = (int)Math.Sqrt(count);
            var w = rect.Width / sqrt;
            var h = rect.Height / sqrt;
            for (int i = 0; i < sqrt; i++)
                for (int j = 0; j < sqrt; j++)
                    list.Add(new Rectangle(rect.X + i * w, rect.Y + j * h, w, h));
            return list;
        }
        public static void Clip(this Rectangle bounds, Rectangle source, Rectangle viewport, out Rectangle finalBounds, out Rectangle finalSource)
        {
            finalBounds = Rectangle.Intersect(bounds, viewport);
            finalSource =
                new Rectangle(
                    source.X + finalBounds.X - bounds.X,
                    source.Y + finalBounds.Y - bounds.Y,
                    source.Width - (bounds.Width - finalBounds.Width),
                    source.Height - (bounds.Height - finalBounds.Height)
                    );
        }
        public static Vector4 ToVector4(this Rectangle rect)
        {
            return new Vector4(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

    }
}
