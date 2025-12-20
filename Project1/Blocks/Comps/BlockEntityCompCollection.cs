using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class BlockEntityCompCollection : Inspectable
    {
        readonly BlockEntity Owner;
        readonly Dictionary<Type, BlockEntityComp> _inner = [];
        public IEnumerable<BlockEntityComp> Values => this._inner.Values;

        internal T GetComp<T>() where T : BlockEntityComp
        {
            return (T)this._inner[typeof(T)];
        }
        internal bool TryGetComp<T>(out T comp) where T : BlockEntityComp
        {
            comp = null;
            if (!this._inner.TryGetValue(typeof(T), out var found))
                return false;
            comp = (T)found;
            return true;
        }
        internal void AddComp(BlockEntityComp comp)
        {
            this._inner.Add(comp.GetType(), comp);
        }
        public override IEnumerable<(string item, object value)> Inspect()
        {
            foreach (var c in this._inner.Values)
                foreach (var i in c.Inspect())
                    yield return i;
        }

        public BlockEntityCompCollection(BlockEntity owner)
        {
            this.Owner = owner;
        }

        public int Count => ((ICollection<BlockEntityComp>)this._inner).Count;

        public bool IsReadOnly => ((ICollection<BlockEntityComp>)this._inner).IsReadOnly;

        public virtual SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            if (!this._inner.Any())
                return tag;
            foreach (var c in this._inner.Values)
                tag.Add(c.Save(c.GetType().FullName));
            return tag;
        }
        public virtual void Load(SaveTag tag)
        {
            foreach (var c in this._inner.Values)
                tag.TryGetTag(c.GetType().FullName, ct => c.Load(ct));
        }

    }
}
