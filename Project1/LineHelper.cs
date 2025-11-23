using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class LineHelper
    {
        static public bool LineOfSight(int x0, int y0, int z0, int x1, int y1, int z1, Predicate<Vector3> check)
        {
            var points = Plot3dThickFrom2d(x0, y0, x1, y1, z1);

            foreach (var p in points)
            {
                // if blocked return false
                if (check(p))
                    return false;
            }
            return true;
        }
        static public bool LineOfSight(int x0, int y0, int z0, int x1, int y1, int z1, Predicate<Vector3> check, out List<Vector3> points)
        {
            points = Plot3dThickFrom2d(x0, y0, x1, y1, z1);

            foreach (var p in points)
            {
                // if blocked return false
                if (check(p))
                    return false;
            }
            return true;
        }

        static public List<Vector2> Plot2D(int x0, int y0, int x1, int y1)
        {
            var points = new List<Vector2>();

            var dx = x1 - x0;
            var dy = y1 - y0;
            int sx, sy; //steps

            if (dy < 0)
            {
                dy = -dy;
                sy = -1;
            }
            else
                sy = 1;

            if (dx < 0)
            {
                dx = -dx;
                sx = -1;
            }
            else
                sx = 1;

            int f = 0;
            int x = x0, y = y0;
            if (dx >= dy)
            {
                while (x != x1) // doesn't add last point?
                {
                    var p = new Vector2(x, y);
                    points.Add(p);

                    f += dy;
                    if (f > dx)
                    {
                        y += sy;
                        f -= dx;
                    }
                    x += sx;
                }
            }
            else
            {
                while (y != y1)
                {
                    var p = new Vector2(x, y);
                    points.Add(p);

                    f += dx;
                    if (f > dy)
                    {
                        x += sx;
                        f -= dy;
                    }
                    y += sy;
                }
            }

            points.Add(new Vector2(x1, y1));

            return points;
        }

        static List<Vector2> Plot2dThick(int x0, int y0, int x1, int y1)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx - dy, e2, x2;                       /* error value e_xy */
            float ed = dx + dy == 0 ? 1 : (float)Math.Sqrt((float)dx * dx + (float)dy * dy);
            var points = new List<Vector2>();

            for (; ; )
            {                                         /* pixel loop */
                points.Add(new Vector2(x0, y0));
                e2 = err; x2 = x0;
                if (2 * e2 >= -dx)
                {                                    /* x step */
                    if (x0 == x1) break;
                    if (e2 + dy < ed) points.Add(new Vector2(x0, y0 + sy));
                    err -= dy; x0 += sx;
                }
                if (2 * e2 <= dy)
                {                                     /* y step */
                    if (y0 == y1) break;
                    if (dx - e2 < ed) points.Add(new Vector2(x2 + sx, y0));
                    err += dx; y0 += sy;
                }
            }
            return points;
        }
        static public List<Vector3> Plot3dThickFrom2d(int x0, int y0, int x1, int y1, int z)
        {
            var points = Plot2dThick(x0, y0, x1, y1);
            return points.Select(v => new Vector3(v, z)).ToList();
        }
    }
}
