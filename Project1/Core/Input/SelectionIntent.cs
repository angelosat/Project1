using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Core.UI;
using Project1.Framework;
using Project1.Framework.Serialization;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Project1.Core.Input
{
    public readonly record struct SelectionIntent : ISerializableNewNew<SelectionIntent>
    {
        readonly SelectionType Type;
        internal readonly IntVec3 Begin, End;
        readonly ImmutableHashSet<EntityRefId> EntityIDs = [];
        readonly ImmutableHashSet<IntVec3> Cells = [];
        public IEnumerable<T> Resolve<T>(MapBase map) where T : ISelectable => this.Resolve(map).Cast<T>();
        public IEnumerable<ISelectable> Resolve(MapBase map)
        {
            if (this.Type == SelectionType.Null)
                return [];
            if (this.Type == SelectionType.List)
                return this.Cells.Select(c => new CellSelection(map, c)).Cast<ISelectable>();
            else if (this.Type == SelectionType.Box)
                return IntVec3Helper.GetBox(this.Begin, this.End).Select(c => new CellSelection(map, c)).Cast<ISelectable>();
            else if (this.Type == SelectionType.Entities)
                return map.World.Get(this.EntityIDs);
            throw new UnreachableException();
        }
        public IEnumerable<InteractionTarget> ResolveTargets(MapBase map)
        {
            if (this.Type == SelectionType.Null)
                return [];
            if (this.Type == SelectionType.List)
                return this.Cells.Select(c => new InteractionTarget(map, c));
            else if (this.Type == SelectionType.Box)
                return IntVec3Helper.GetBox(this.Begin, this.End).Select(c => new InteractionTarget(map, c));
            else if (this.Type == SelectionType.Entities)
                return map.World.Get(this.EntityIDs).Select(e => new InteractionTarget(e));
            throw new UnreachableException();
        }
        public SelectionResolved ResolveTargetsNew(MapBase map)
        {
            if (this.Type == SelectionType.Null)
                return new SelectionResolved([], this);
            if (this.Type == SelectionType.List)
                return new SelectionResolved(this.Cells.Select(c => new InteractionTarget(map, c)), this);
            else if (this.Type == SelectionType.Box)
                return new SelectionResolved(map.Select(this.Begin, this.End), this);
            else if (this.Type == SelectionType.Entities)
                return new SelectionResolved(map.World.Get(this.EntityIDs).Select(e => new InteractionTarget(e)), this);
            throw new UnreachableException();
        }
        public SelectionIntent(IEnumerable<EntityRefId> ids)
        {
            this.Type = SelectionType.Entities;
            this.EntityIDs = [.. ids];
        }
        public SelectionIntent(IEnumerable<IntVec3> cells)
        {
            this.Type = SelectionType.List;
            this.Cells = [.. cells];
        }
        public SelectionIntent(IntVec3 cell)
        {
            this.Type = SelectionType.List;
            this.Cells.Add(cell);
        }
        public SelectionIntent(IntVec3 begin, IntVec3 end)
        {
            this.Type = SelectionType.Box;
            this.Begin = begin;
            this.End = end;
        }
        public SelectionIntent Add(IEnumerable<IntVec3> cells)
        {
            Debug.Assert(this.Type == SelectionType.List);
            return new SelectionIntent(this.Cells.Union(cells));
        }
        public SelectionIntent Remove(IEnumerable<IntVec3> cells)
        {
            Debug.Assert(this.Type == SelectionType.List);
            return new SelectionIntent(this.Cells.Except(cells));
        }
        public SelectionIntent Add(IEnumerable<EntityRefId> ids)
        {
            Debug.Assert(this.Type == SelectionType.Entities);
            return new SelectionIntent(this.EntityIDs.Union(ids));
        }
        public SelectionIntent Remove(IEnumerable<EntityRefId> ids)
        {
            Debug.Assert(this.Type == SelectionType.Entities);
            return new SelectionIntent(this.EntityIDs.Except(ids));
        }
        public readonly IDataWriter Write(IDataWriter w)
        {
            w.Write((int)this.Type);
            switch(this.Type)
            {
                case SelectionType.Null:
                    break;

                case SelectionType.Entities:
                    w.Write(this.EntityIDs);
                    break;

                case SelectionType.List:
                    w.Write(this.Cells);
                    break;

                case SelectionType.Box:
                    w.Write(this.Begin);
                    w.Write(this.End);
                    break;

                default:
                    throw new UnreachableException();
            }
            return w;
        }
        public static SelectionIntent Create(IDataReader r)
        {
            var type = (SelectionType)r.ReadInt32();
            return type switch
            {
                SelectionType.Null => default,
                SelectionType.Entities => new SelectionIntent(r.ReadListEntityRefId().ToImmutableHashSet()),
                SelectionType.List => new SelectionIntent(r.ReadListIntVec3().ToImmutableHashSet()),
                SelectionType.Box => new SelectionIntent(r.ReadIntVec3(), r.ReadIntVec3()),
                _ => throw new UnreachableException()
            };
        }
    }
}
