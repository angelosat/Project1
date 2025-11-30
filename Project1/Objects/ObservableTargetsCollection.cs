using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Start_a_Town_
{
    internal class ObservableTargetsCollection : INotifyCollectionChanged
    {
        public readonly ObservableHashSet<IntVec3> Positions;
        public readonly ObservableHashSet<int> Entities;

        public event NotifyCollectionChangedEventHandler CollectionChanged;


    }
}
