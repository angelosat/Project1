using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Serialization;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Project1.Core.Input
{
    public enum SelectionType { Null, List, Box, Entities }

    public readonly record struct Selection : ISerializableNewNew<Selection>
    {
        readonly SelectionType Type;
        readonly IntVec3 Begin, End;
        readonly ImmutableHashSet<EntityRefId> EntityIDs = [];
        readonly ImmutableHashSet<IntVec3> Cells = [];
       
        public IEnumerable<TargetArgs> ResolveTargets(MapBase map)
        {
            if (this.Type == SelectionType.Null)
                return [];
            if (this.Type == SelectionType.List)
                return this.Cells.Select(c => new TargetArgs(map, c));
            else if (this.Type == SelectionType.Box)
                return IntVec3Helper.GetBox(this.Begin, this.End).Select(c => new TargetArgs(map, c));
            else if (this.Type == SelectionType.Entities)
                return map.World.GetEntities(this.EntityIDs).Select(e => new TargetArgs(e));
            throw new UnreachableException();
        }
        public Selection(IEnumerable<EntityRefId> ids)
        {
            this.Type = SelectionType.Entities;
            this.EntityIDs = [.. ids];
        }
        public Selection(IEnumerable<IntVec3> cells)
        {
            this.Type = SelectionType.List;
            this.Cells = [.. cells];
        }
        public Selection(IntVec3 cell)
        {
            this.Type = SelectionType.List;
            this.Cells.Add(cell);
        }
        public Selection(IntVec3 begin, IntVec3 end)
        {
            this.Type = SelectionType.Box;
            this.Begin = begin;
            this.End = end;
        }
        public Selection Add(IEnumerable<IntVec3> cells)
        {
            Debug.Assert(this.Type == SelectionType.List);
            return new Selection(this.Cells.Union(cells));
        }
        public Selection Remove(IEnumerable<IntVec3> cells)
        {
            Debug.Assert(this.Type == SelectionType.List);
            return new Selection(this.Cells.Except(cells));
        }
        public Selection Add(IEnumerable<EntityRefId> ids)
        {
            Debug.Assert(this.Type == SelectionType.Entities);
            return new Selection(this.EntityIDs.Union(ids));
        }
        public Selection Remove(IEnumerable<EntityRefId> ids)
        {
            Debug.Assert(this.Type == SelectionType.Entities);
            return new Selection(this.EntityIDs.Except(ids));
        }
        public readonly void Write(IDataWriter w)
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
        }
        public static Selection Create(IDataReader r)
        {
            var type = (SelectionType)r.ReadInt32();
            return type switch
            {
                SelectionType.Null => default,
                SelectionType.Entities => new Selection(r.ReadListEntityRefId().ToImmutableHashSet()),
                SelectionType.List => new Selection(r.ReadListIntVec3().ToImmutableHashSet()),
                SelectionType.Box => new Selection(r.ReadIntVec3(), r.ReadIntVec3()),
                _ => throw new UnreachableException()
            };
        }
    }
}
