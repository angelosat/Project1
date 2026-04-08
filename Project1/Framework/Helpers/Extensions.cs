using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Simulation;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace Project1.Framework.Helpers
{
    public static class Extensions
    {
        extension(IntVec3 global)
        {
            public GlobalCellId Id => new(global);
        }

        public static Vector2 ToVector(this Point point) => new(point.X, point.Y); 
        static public Vector3 ToBlock(this Vector3 global)
        {
            global += 0.5f * new Vector3(1, 1, 0); // shouldnt it be -0.5f?
            global -= global.FloorXY();
            return global;
        }
        
        public static TValue GetValueOrDefaultMy<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            // Ignore return value
            dictionary.TryGetValue(key, out TValue ret);
            return ret;
        }
        public static string ToLocalTime(this DateTime dateTime)
        { return dateTime.ToString("MMM dd, HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-GB")); }
       
        public static bool Intersects(this Vector4 bounds, Vector2 position)
        {
            return (bounds.X <= position.X &&
                position.X < bounds.X + bounds.Z &&
                bounds.Y <= position.Y &&
                position.Y < bounds.Y + bounds.W);
        }

        public static T SelectRandom<T>(this ICollection<T> collection, Random random)
        {
            return collection.ElementAt(random.Next(0, collection.Count));
        }

        public static List<T> Randomize<T>(this IEnumerable<T> list, RandomThreaded random)
        {
            var unhandled = list.ToList();
            var randomized = new Queue<T>();
            while (unhandled.Count > 0)
            {
                var current = unhandled[random.Next(unhandled.Count)];
                unhandled.Remove(current);
                randomized.Enqueue(current);
            }
            return randomized.ToList();
        }
        public static T[] Shuffle<T>(this IEnumerable<T> collection, Random random)
        {
            var array = collection.ToArray();
            array.Shuffle(random);
            return array;
        }
        public static void Shuffle<T>(this T[] collection, Random random)
        {
            var watch = Stopwatch.StartNew();
            var count = collection.Length;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = random.Next(i, count);
                var tmp = collection[i];
                collection[i] = collection[r];
                collection[r] = tmp;
            }
            watch.Stop();
            $"{count} items randomized in {watch.ElapsedMilliseconds} ms".ToConsole();
        }
        
        static public BoundingBox GetBoundingBox(this Vector3 vec1, Vector3 vec2)
        {
            int xm = (int)Math.Min(vec1.X, vec2.X);
            int ym = (int)Math.Min(vec1.Y, vec2.Y);
            int zm = (int)Math.Min(vec1.Z, vec2.Z);

            int xM = (int)(vec1.X + vec2.X - xm);
            int yM = (int)(vec1.Y + vec2.Y - ym);
            int zM = (int)(vec1.Z + vec2.Z - zm);

            var m = new Vector3(xm, ym, zm);
            var M = new Vector3(xM, yM, zM);

            return new BoundingBox(m, M);
        }
        static public BoundingBox GetBoundingBox(this IntVec3 vec1, IntVec3 vec2)
        {
            int xm = (int)Math.Min(vec1.X, vec2.X);
            int ym = (int)Math.Min(vec1.Y, vec2.Y);
            int zm = (int)Math.Min(vec1.Z, vec2.Z);

            int xM = (int)(vec1.X + vec2.X - xm);
            int yM = (int)(vec1.Y + vec2.Y - ym);
            int zM = (int)(vec1.Z + vec2.Z - zm);

            var m = new IntVec3(xm, ym, zm);
            var M = new IntVec3(xM, yM, zM);

            return new BoundingBox(m, M);
        }

        static public List<Vector3> GetBox(this Vector3 begin, int dx, int dy, int dz)
        {
            var list = new List<Vector3>();

            for (int i = 0; i < dx; i++)
            {
                for (int j = 0; j < dy; j++)
                {
                    for (int k = 0; k < dz; k++)
                    {
                        list.Add(begin + new Vector3(i, j, k));
                    }
                }
            }
            return list;
        }
        
        static public BoundingBox GetBoundingBox(this Vector3 blockCoords)
        {
            blockCoords = blockCoords.ToCell(); //necessary? do i need this?
            return new BoundingBox(blockCoords - new Vector3(.5f, .5f, 0), blockCoords + new Vector3(.5f, .5f, 1));
        }
        
        static public Vector3 Average(this ICollection<IntVec3> positions)
        {
            Vector3 average = default;
            foreach (var pos in positions)
                average += (Vector3)pos;
            return average / positions.Count;

        }

        public static void ShowDialog(this Exception e)
        {
            MessageBox.Create("Exception", e.ToString(),
                        "Copy to Clipboard", () =>
                        {
                            var t = new Thread(() => System.Windows.Forms.Clipboard.SetText(e.ToString()));
                            t.SetApartmentState(ApartmentState.STA);
                            t.Start();
                        }).ShowDialog();
        }
        
        

        /// <summary>
        /// https://stackoverflow.com/questions/14892594/how-to-get-an-xelement-and-create-it-if-it-doesnt-exist
        /// </summary>
        /// <param name="container"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static XElement GetOrCreateElement(this XContainer container, string name, object value = null)
        {
            var element = container.Element(name);
            if (element == null)
            {
                element = new XElement(name, value);
                container.Add(element);
            }
            return element;
        }
        public static XElement GetOrCreateElements(this XContainer container, params string[] names)
        {
            var currentelement = container;
            for (int i = 0; i < names.Length; i++)
            {
                var name = names[i];
                var nextelement = currentelement.Element(name);
                if (nextelement == null)
                {
                    nextelement = new XElement(name);
                    currentelement.Add(nextelement);
                }
                currentelement = nextelement;
            }
            return currentelement as XElement;
        }
        public static void SetValue(this XDocument document, string path, object value)
        {
            var names = path.Split('/');
            document.Root.GetOrCreateElements(names).SetValue(value.ToString());
        }
        
        public static Texture2D ToGrayscale(this Texture2D tex)
        {
            Color[] array = new Color[tex.Width * tex.Height];
            tex.GetData(array);
            Color[] grayscale = new Color[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                var c = array[i];
                var value = c.R + c.G + c.B;
                value /= 3;
                grayscale[i] = new Color(value, value, value, c.A);
            }
            var copy = new Texture2D(tex.GraphicsDevice, tex.Width, tex.Height);
            copy.SetData(grayscale);
            return copy;
        }
        public static Color[] ToGrayscaleArray(this Texture2D tex)
        {
            Color[] array = new Color[tex.Width * tex.Height];
            tex.GetData(array);
            Color[] grayscale = new Color[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                var c = array[i];
                var value = c.R + c.G + c.B;
                value /= 3;
                grayscale[i] = new Color(value, value, value, c.A);
            }
            return grayscale;
        }

        public static bool Roll(this Random rand, double chance)
        {
            if (chance <= 0)
                return false;
            if (chance >= 1)
                return true;
            var roll = rand.NextDouble();
            return roll <= chance;
        }
        public static bool Roll100(this Random rand, int chance)
        {
            if (chance <= 0)
                return false;
            if (chance >= 100)
                return true;
            var roll = rand.Next(100) + 1;
            return roll <= chance;
        }
        static public Dictionary<T, U> ToDictionary<T,U>(this IList<T> listA, IList<U> listB)
        {
            var count = listA.Count;
            if (count != listB.Count)
                throw new Exception();
            var dic = new Dictionary<T, U>();
            for (int i = 0; i < count; i++)
            {
                dic.Add(listA[i], listB[i]); 
            }
            return dic;
        }
        static public Dictionary<TResult, UResult> ToDictionary<T, U, TResult, UResult>(this IList<T> listA, IList<U> listB, Func<T, TResult> keySelector, Func<U, UResult> valueSelector)
        {
            var count = listA.Count;
            if (count != listB.Count)
                throw new Exception();
            var dic = new Dictionary<TResult, UResult>();
            for (int i = 0; i < count; i++)
            {
                dic.Add(keySelector(listA[i]), valueSelector(listB[i]));
            }
            return dic;
        }
    }
}
