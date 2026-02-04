using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Graphics
{
    public partial class Atlas
    {
        public partial class Node : Inspectable
        {
            public Texture2D Texture { get; set; }
            public Rectangle Rectangle { get; set; }
            public List<Node> Children = new List<Node>(2);
            public Node(Rectangle rectangle)
            {
                this.Rectangle = rectangle;
            }
            public Node(int x, int y, int w, int h)
            {
                this.Rectangle = new Rectangle(x, y, w, h);
            }
            public int Width { get { return this.Rectangle.Width; } }
            public int Height { get { return this.Rectangle.Height; } }
            public int X { get { return this.Rectangle.X; } }
            public int Y { get { return this.Rectangle.Y; } }

            public List<Node> GetChildren()
            {
                List<Node> list = new List<Node>() { this };
                foreach (var child in this.Children)
                    list.AddRange(child.GetChildren());
                return list;
            }

            public Node FindNode(int w, int h)
            {
                if (this.Texture != null)
                    return this.Children[0].FindNode(w, h) ?? this.Children[1].FindNode(w, h);
                if (w <= this.Width && h <= this.Height)
                    return this;
                return null;
            }
            public void Split(int w, int h)
            {
                if (w < h)
                {
                    // perform vertical slice
                    Node right = new Node(new Rectangle(this.X + w, this.Y, this.Width - w, this.Height));
                    Node left = new Node(new Rectangle(this.X, this.Y + h, w, this.Height - h));
                    this.Children.Add(right);
                    this.Children.Add(left);
                }
                else
                {
                    // perform horizontal slice
                    Node bottom = new Node(new Rectangle(this.X, this.Y + h, this.Width, this.Height - h));
                    Node top = new Node(new Rectangle(this.X + w, this.Y, this.Width - w, h));
                    this.Children.Add(top);
                    this.Children.Add(bottom);
                }
            }
        }
    }
}
