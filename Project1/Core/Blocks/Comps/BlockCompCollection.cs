using Project1.Framework;
using System;
using System.Collections.Generic;

namespace Project1.Core.Blocks
{
    public class BlockCompCollection(BlockEntity owner) : Inspectable
    {
        readonly BlockEntity Owner = owner;
        [InspectorHidden]
        readonly Dictionary<Type, BlockComp> _inner = [];
        [InspectorHidden]
        readonly List<BlockComp> compList = [];

        public IEnumerable<BlockComp> Values => this._inner.Values;
        internal T GetComp<T>() where T : BlockComp => (T)this._inner[typeof(T)];
        internal BlockComp GetComp(Type compType) => this._inner[compType];
        internal BlockComp GetComp(int compIndex) => this.compList[compIndex];
        public IReadOnlyCollection<BlockComp> Inner => this._inner.Values;

        internal bool TryGetComp<T>(out T comp) where T : BlockComp
        {
            comp = null;
            if (!this._inner.TryGetValue(typeof(T), out var found))
                return false;
            comp = (T)found;
            return true;
        }

        internal void AddComp(BlockComp comp)
        {
            this._inner.Add(comp.GetType(), comp);
            comp.RuntimeIndex = this.compList.Count;
            this.compList.Add(comp);
        }

        public virtual SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            if (this._inner.Count == 0)
                return tag;
            foreach (var c in this._inner.Values)
                tag.Add(c.Save(c.CompDef.Name));
            return tag;
        }

        public virtual void Load(SaveTag tag)
        {
            foreach (var c in this._inner.Values)
                tag.TryGetTag(c.CompDef.Name, c.Load);
        }
    }
}
