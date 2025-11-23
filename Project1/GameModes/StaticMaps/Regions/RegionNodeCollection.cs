using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class RegionNodeCollection : IEnumerable<KeyValuePair<Vector3, RegionNode>>
    {
        Dictionary<Vector3, RegionNode> Nodes = new();
        public IEnumerator<KeyValuePair<Vector3, RegionNode>> GetEnumerator() => this.Nodes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        readonly Region Region;
        public RegionNodeCollection(Region region)
        {
            this.Region = region;
        }
        public void Clear()
        {
            this.Nodes.Clear();
        }
        public bool ContainsKey(Vector3 key)
        {
            return this.Nodes.ContainsKey(key);
        }
        public RegionNode GetValueOrDefault(Vector3 key)
        {
            this.Nodes.TryGetValue(key, out RegionNode val);
            return val;
        }
        public bool TryGetValue(Vector3 key, out RegionNode value)
        {
            return this.Nodes.TryGetValue(key, out value);
        }
        public Dictionary<Vector3, RegionNode>.ValueCollection Values => this.Nodes.Values; 
        public int Count => this.Nodes.Count; 
        public RegionNode this[Vector3 key]
        {
            get => this.Nodes[key]; 
            set => this.Nodes[key] = value;
        }
        public bool Remove(Vector3 key) => this.Nodes.Remove(key);
        public void Add(RegionNode node)
        {
            if (node.Region != this.Region)
            {
                if (node.Region != null)
                    node.Region.Nodes.Remove(node.Global);
                node.Region = this.Region;
            }
            this.Nodes.Add(node.Global, node);
        }
        
        internal void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.Nodes.Count);
            foreach(var node in this.Nodes)
            {
                throw new NotImplementedException();
            }
        }
    }
}
