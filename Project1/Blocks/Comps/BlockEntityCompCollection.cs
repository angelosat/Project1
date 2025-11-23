using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Start_a_Town_
{
    public class BlockEntityCompCollection : Inspectable, ICollection<BlockEntityComp>
    {
        //readonly BlockEntity Parent;
        readonly Collection<BlockEntityComp> Comps = new();

        public override IEnumerable<(string item, object value)> Inspect()
        {
            foreach (var c in this.Comps)
                foreach (var i in c.Inspect())
                    yield return i;
        }

        //public BlockEntityCompCollection(BlockEntity parent)
        //{
        //    this.Parent = parent;
        //}

        public int Count => ((ICollection<BlockEntityComp>)this.Comps).Count;

        public bool IsReadOnly => ((ICollection<BlockEntityComp>)this.Comps).IsReadOnly;

        public virtual SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            if (!this.Comps.Any())
                return tag;
            foreach (var c in this.Comps)
                tag.Add(c.Save(c.GetType().FullName));
            return tag;
        }
        public virtual void Load(SaveTag tag)
        {
            foreach (var c in this.Comps)
                tag.TryGetTag(c.GetType().FullName, ct => c.Load(ct));
        }

        public void Add(BlockEntityComp item)
        {
            //item.Parent = this.Parent;
            ((ICollection<BlockEntityComp>)this.Comps).Add(item);
        }

        public void Clear()
        {
            ((ICollection<BlockEntityComp>)this.Comps).Clear();
        }

        public bool Contains(BlockEntityComp item)
        {
            return ((ICollection<BlockEntityComp>)this.Comps).Contains(item);
        }

        public void CopyTo(BlockEntityComp[] array, int arrayIndex)
        {
            ((ICollection<BlockEntityComp>)this.Comps).CopyTo(array, arrayIndex);
        }

        public bool Remove(BlockEntityComp item)
        {
            return ((ICollection<BlockEntityComp>)this.Comps).Remove(item);
        }

        public IEnumerator<BlockEntityComp> GetEnumerator()
        {
            return ((IEnumerable<BlockEntityComp>)this.Comps).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.Comps).GetEnumerator();
        }
    }
    public class BlockEntityCompCollection<T> : List<T> where T : IBlockEntityComp
    {
        public virtual SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            if (!this.Any())
                return tag;
            foreach (var c in this)
                tag.Add(c.Save(c.GetType().FullName));
            return tag;
        }
        public virtual void Load(SaveTag tag)
        {
            foreach (var c in this)
                tag.TryGetTag(c.GetType().FullName, ct => c.Load(ct));
        }
    }
    public class BlockEntityCompCollectionNew : List<BlockEntityComp> 
    {
        public virtual SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            if (!this.Any())
                return tag;
            foreach (var c in this)
                tag.Add(c.Save(c.GetType().FullName));
            return tag;
        }
        public virtual void Load(SaveTag tag)
        {
            foreach (var c in this)
                tag.TryGetTag(c.GetType().FullName, ct => c.Load(ct));
        }
    }
}
