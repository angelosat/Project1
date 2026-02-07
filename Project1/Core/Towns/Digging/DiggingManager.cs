using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Base;
using Project1.Core.Input;
using System.Collections.Generic;
using System.Linq;
using Project1.Core.Rendering;
using Project1.Core.WorldGen;
using Project1.Core.Materials;
using Project1.Core.Towns;
using Project1.Core.Towns.Designations;
using Project1.Core.Helpers;
using Project1.Core.Net;
using Project1.Core.Simulation;

namespace Project1.Core.Towns.Digging
{
    public class DiggingManager : TownComponent
    {
        HashSet<IntVec3> AllPositions = new();

        public DiggingManager(Town town)
        {
            this.Town = town;
            this.Town.Map.Events.ListenTo<CellsInvalidatedEvent>(HandleBlocksChanged);
        }
        public override string Name => "Digging";

        internal HashSet<IntVec3> GetPositions()
        {
            return this.AllPositions;
        }
        void HandleBlocksChanged(CellsInvalidatedEvent e)
        {
            foreach (var global in e.Positions)
                if (this.AllPositions.Contains(global))
                    if (this.Map.IsAir(global))
                        this.AllPositions.Remove(global);
        }

        private void HandlePosition(IntVec3 p)
        {
            if (this.IsMinable(p))
            {
                this.AllPositions.Add(p);
            }
        }
        private void RemovePosition(IntVec3 p)
        {
            this.AllPositions.Remove(p);
        }
        public HashSet<IntVec3> GetAllPendingTasks()
        {
            return this.AllPositions;
        }

        bool IsMinable(IntVec3 global)
        {
            var material = Block.GetBlockMaterial(this.Town.Map, global);
            var mattype = material.Type;
            return
                mattype == MaterialTypeDefOf.Soil ||
                mattype == MaterialTypeDefOf.Stone || 
                mattype == MaterialTypeDefOf.Metal;

            //var skill = material.Type.SkillToExtract;
            //if (skill == null)
            //    return false;
            //var interaction = skill.GetInteraction();
            //if (interaction == null)
            //    return false;
            //return true;
        }
       
        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.AllPositions.ToList().Save("Positions"));
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValue<List<SaveTag>>("Positions", v => this.AllPositions = new HashSet<IntVec3>(new List<IntVec3>().Load(v)));
        }
        public override void Write(IDataWriter w)
        {
            w.Write(this.AllPositions.ToList());
        }
        public override void Read(IDataReader r)
        {
            this.AllPositions = [.. r.ReadListIntVec3()];
        }
        public override void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera cam)
        {
            cam.DrawCellHighlights(sb, Block.BlockBlueprint, this.AllPositions, Color.White);
        }
        bool IsDiggingTask(IntVec3 global)
        {
            return this.AllPositions.Contains(global);
        }

        public void Edit()
        {
            ToolManager.SetTool(new ToolDigging((a, b, r) => PacketDesignation.Send(Client.Instance, r, a, b, DesignationDefOf.Mine)));
        }
        public void EditDeconstruct()
        {
            ToolManager.SetTool(new ToolDigging((a, b, r) => PacketDesignation.Send(Client.Instance, r, a, b, DesignationDefOf.Deconstruct)));
        }
    }
}
