using Project1.Core.Blocks;
using Project1.Core.Helpers;
using Project1.Core.Materials;
using Project1.Framework;
using Project1.Framework.Serialization;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Project1.Core.Simulation
{
    
    class ChunkSerializer
    {
        struct AirRun
        {
            public int StartIndex;
            public int Count;
            public bool Discovered;
        }

        struct SolidRun
        {
            public int StartIndex;
            public BitVector32[] Cells;
        }

        struct Run
        {
            public int StartIndex;
            public int Count;
        }

        struct RunIndex<T>
        {
            public T Key;       // BlockDef or MaterialDef
            public Run[] Runs;  // contiguous runs
        }

        class ChunkSnapshot
        {
            public AirRun[] AirRuns;
            public SolidRun[] SolidRuns;
            public RunIndex<BlockDef>[] BlockDefRuns;
            public RunIndex<MaterialDef>[] MaterialDefRuns;
        }

        // Stateless singleton or on-demand; all methods just take Chunk
        ChunkSnapshot BuildSnapshot(Chunk chunk)
        {
            var cells = chunk.Cells;
            int length = cells.Length;

            // --- Run tracking ---
            int airRunStart = -1;
            int airRunCount = 0;
            bool airRunDiscovered = false;
            var airRuns = new List<AirRun>();

            int solidRunStart = -1;
            var solidRunData = new List<BitVector32>();
            var solidRuns = new List<SolidRun>();

            var blockRuns = new Dictionary<BlockDef, List<Run>>();
            var materialRuns = new Dictionary<MaterialDef, List<Run>>();

            BlockDef? currentBlock = null;
            MaterialDef? currentMaterial = null;
            int currentBlockRunStart = 0;
            int currentMaterialRunStart = 0;

            for (int i = 0; i < length; i++)
            {
                var cell = cells[i];

                // --- Air run ---
                if (cell.Block == BlockDefOf.Air.Block)
                {
                    if (airRunStart == -1)
                    {
                        airRunStart = i;
                        airRunCount = 1;
                        airRunDiscovered = cell.Discovered;
                    }
                    else airRunCount++;

                    // flush solid run
                    if (solidRunStart != -1)
                    {
                        solidRuns.Add(new SolidRun { StartIndex = solidRunStart, Cells = solidRunData.ToArray() });
                        solidRunData.Clear();
                        solidRunStart = -1;
                    }

                    // flush block/material runs
                    if (currentBlock != null)
                    {
                        blockRuns[currentBlock].Add(new Run { StartIndex = currentBlockRunStart, Count = i - currentBlockRunStart });
                        currentBlock = null;
                    }
                    if (currentMaterial != null)
                    {
                        materialRuns[currentMaterial].Add(new Run { StartIndex = currentMaterialRunStart, Count = i - currentMaterialRunStart });
                        currentMaterial = null;
                    }

                    continue;
                }

                // --- Non-air cell ---
                if (airRunStart != -1)
                {
                    airRuns.Add(new AirRun { StartIndex = airRunStart, Count = airRunCount, Discovered = airRunDiscovered });
                    airRunStart = -1;
                    airRunCount = 0;
                }

                // --- Solid run ---
                if (solidRunStart == -1) solidRunStart = i;
                solidRunData.Add(cell.Data);

                // --- BlockDef run ---
                if (currentBlock != cell.Block.BlockDef)
                {
                    if (currentBlock != null)
                        blockRuns[currentBlock].Add(new Run { StartIndex = currentBlockRunStart, Count = i - currentBlockRunStart });

                    currentBlock = cell.Block.BlockDef;
                    if (!blockRuns.ContainsKey(currentBlock))
                        blockRuns[currentBlock] = new List<Run>();
                    currentBlockRunStart = i;
                }

                // --- MaterialDef run ---
                if (currentMaterial != cell.Material)
                {
                    if (currentMaterial != null)
                        materialRuns[currentMaterial].Add(new Run { StartIndex = currentMaterialRunStart, Count = i - currentMaterialRunStart });

                    currentMaterial = cell.Material;
                    if (!materialRuns.ContainsKey(currentMaterial))
                        materialRuns[currentMaterial] = new List<Run>();
                    currentMaterialRunStart = i;
                }
            }

            // --- Final flushes ---
            if (airRunStart != -1)
                airRuns.Add(new AirRun { StartIndex = airRunStart, Count = airRunCount, Discovered = airRunDiscovered });
            if (solidRunStart != -1)
                solidRuns.Add(new SolidRun { StartIndex = solidRunStart, Cells = solidRunData.ToArray() });
            if (currentBlock != null)
                blockRuns[currentBlock].Add(new Run { StartIndex = currentBlockRunStart, Count = length - currentBlockRunStart });
            if (currentMaterial != null)
                materialRuns[currentMaterial].Add(new Run { StartIndex = currentMaterialRunStart, Count = length - currentMaterialRunStart });

            // convert dictionary to RunIndex[]
            var blockDefRuns = blockRuns.Select(kv => new RunIndex<BlockDef> { Key = kv.Key, Runs = kv.Value.ToArray() }).ToArray();
            var materialDefRuns = materialRuns.Select(kv => new RunIndex<MaterialDef> { Key = kv.Key, Runs = kv.Value.ToArray() }).ToArray();

            return new ChunkSnapshot
            {
                AirRuns = airRuns.ToArray(),
                SolidRuns = solidRuns.ToArray(),
                BlockDefRuns = blockDefRuns,
                MaterialDefRuns = materialDefRuns
            };
        }
    
        public void Serialize(Chunk chunk, SaveTag chunkTag)
        {
            var snapshot = this.BuildSnapshot(chunk);

            //var chunkTag = new SaveTag(SaveTag.Types.Compound, "Chunk");

            // --- Air runs ---
            var airTag = new SaveTag(SaveTag.Types.List, "Air", SaveTag.Types.Compound);
            foreach (var run in snapshot.AirRuns)
            {
                var runTag = new SaveTag(SaveTag.Types.Compound);
                runTag.Add(new SaveTag(SaveTag.Types.Int, "StartIndex", run.StartIndex));
                runTag.Add(new SaveTag(SaveTag.Types.Int, "Count", run.Count));
                runTag.Add(new SaveTag(SaveTag.Types.Bool, "Discovered", run.Discovered));
                airTag.Add(runTag);
            }
            chunkTag.Add(airTag);

            // --- Solid runs ---
            var solidTag = new SaveTag(SaveTag.Types.List, "Solid", SaveTag.Types.Compound);
            foreach (var run in snapshot.SolidRuns)
            {
                var runTag = new SaveTag(SaveTag.Types.Compound);
                runTag.Add(new SaveTag(SaveTag.Types.Int, "StartIndex", run.StartIndex));

                var dataTag = new SaveTag(SaveTag.Types.List, "Data", SaveTag.Types.Int);
                foreach (var cell in run.Cells)
                    dataTag.Add(new SaveTag(SaveTag.Types.Int, "Data", cell.Data)); // BitVector32 as int

                runTag.Add(dataTag);
                solidTag.Add(runTag);
            }
            chunkTag.Add(solidTag);

            // --- BlockDef runs ---
            var blockTag = new SaveTag(SaveTag.Types.Compound, "IndicesByBlock");
            foreach (var blockRun in snapshot.BlockDefRuns)
            {
                var blockNameTag = new SaveTag(SaveTag.Types.List, blockRun.Key.Name, SaveTag.Types.Compound);
                foreach (var run in blockRun.Runs)
                {
                    var runTag = new SaveTag(SaveTag.Types.Compound);
                    runTag.Add(new SaveTag(SaveTag.Types.Int, "StartIndex", run.StartIndex));
                    runTag.Add(new SaveTag(SaveTag.Types.Int, "Count", run.Count));
                    blockNameTag.Add(runTag);
                }
                blockTag.Add(blockNameTag);
            }
            chunkTag.Add(blockTag);

            // --- Material runs ---
            var materialTag = new SaveTag(SaveTag.Types.Compound, "IndicesByMaterial");
            foreach (var matRun in snapshot.MaterialDefRuns)
            {
                var matNameTag = new SaveTag(SaveTag.Types.List, matRun.Key.Name, SaveTag.Types.Compound);
                foreach (var run in matRun.Runs)
                {
                    var runTag = new SaveTag(SaveTag.Types.Compound);
                    runTag.Add(new SaveTag(SaveTag.Types.Int, "StartIndex", run.StartIndex));
                    runTag.Add(new SaveTag(SaveTag.Types.Int, "Count", run.Count));
                    matNameTag.Add(runTag);
                }
                materialTag.Add(matNameTag);
            }
            chunkTag.Add(materialTag);

            //return chunkTag;
        }
        public void Deserialize(Chunk chunk, SaveTag chunkTag)
        {
            var snapshot = new ChunkSnapshot();

            // --- Air runs ---
            var airRunsTag = chunkTag.GetList("Air");
            var airRuns = new AirRun[airRunsTag.Count];
            for (int i = 0; i < airRunsTag.Count; i++)
            {
                var runTag = airRunsTag[i];
                airRuns[i] = new AirRun
                {
                    StartIndex = runTag.GetInt("StartIndex"),
                    Count = runTag.GetInt("Count"),
                    Discovered = runTag.GetBool("Discovered")
                };
            }
            snapshot.AirRuns = airRuns;

            // --- Solid runs ---
            var solidRunsTag = chunkTag.GetList("Solid");
            var solidRuns = new SolidRun[solidRunsTag.Count];
            for (int i = 0; i < solidRunsTag.Count; i++)
            {
                var runTag = solidRunsTag[i];
                int startIndex = runTag.GetInt("StartIndex");

                var dataListTag = runTag.GetList("Data");
                var cells = new BitVector32[dataListTag.Count];
                for (int j = 0; j < dataListTag.Count; j++)
                    cells[j] = new BitVector32(dataListTag[j].GetInt());

                solidRuns[i] = new SolidRun
                {
                    StartIndex = startIndex,
                    Cells = cells
                };
            }
            snapshot.SolidRuns = solidRuns;

            // --- BlockDef runs ---
            var blockRunsTag = chunkTag.GetCompound("IndicesByBlock");
            var blockRunList = new List<RunIndex<BlockDef>>();
            foreach (var kvp in blockRunsTag)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                    continue;

                var blockDef = Def.GetDef<BlockDef>(kvp.Key);
                var runTags = kvp.Value.GetList();
                var runs = new Run[runTags.Count];
                for (int i = 0; i < runTags.Count; i++)
                {
                    runs[i] = new Run
                    {
                        StartIndex = runTags[i].GetInt("StartIndex"),
                        Count = runTags[i].GetInt("Count")
                    };
                }

                blockRunList.Add(new RunIndex<BlockDef>
                {
                    Key = blockDef,
                    Runs = runs
                });
            }
            snapshot.BlockDefRuns = blockRunList.ToArray();

            // --- MaterialDef runs ---
            var materialRunsTag = chunkTag.GetCompound("IndicesByMaterial");
            var materialRunList = new List<RunIndex<MaterialDef>>();
            foreach (var kvp in materialRunsTag)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                    continue;

                var matDef = Def.GetDef<MaterialDef>(kvp.Key);
                var runTags = kvp.Value.GetList();
                var runs = new Run[runTags.Count];
                for (int i = 0; i < runTags.Count; i++)
                {
                    runs[i] = new Run
                    {
                        StartIndex = runTags[i].GetInt("StartIndex"),
                        Count = runTags[i].GetInt("Count")
                    };
                }

                materialRunList.Add(new RunIndex<MaterialDef>
                {
                    Key = matDef,
                    Runs = runs
                });
            }
            snapshot.MaterialDefRuns = materialRunList.ToArray();

            // --- Apply snapshot to chunk ---
            this.ApplySnapshot(chunk, snapshot);
        }

        public void Serialize(Chunk chunk, IDataWriter writer)
        {
            var snapshot = BuildSnapshot(chunk);

            // --- Serialize air runs ---
            writer.Write(snapshot.AirRuns.Length);
            foreach (var run in snapshot.AirRuns)
            {
                writer.Write(run.StartIndex);
                writer.Write(run.Count);
                writer.Write(run.Discovered);
            }

            // --- Serialize solid runs ---
            writer.Write(snapshot.SolidRuns.Length);
            foreach (var run in snapshot.SolidRuns)
            {
                writer.Write(run.StartIndex);
                writer.Write(run.Cells.Length);
                foreach (var cell in run.Cells)
                    writer.Write(cell.Data);
            }

            // --- Serialize block runs ---
            writer.Write(snapshot.BlockDefRuns.Length);
            foreach (var blockRun in snapshot.BlockDefRuns)
            {
                writer.Write(blockRun.Key.Name);
                writer.Write(blockRun.Runs.Length);
                foreach (var run in blockRun.Runs)
                {
                    writer.Write(run.StartIndex);
                    writer.Write(run.Count);
                }
            }

            // --- Serialize material runs ---
            writer.Write(snapshot.MaterialDefRuns.Length);
            foreach (var matRun in snapshot.MaterialDefRuns)
            {
                writer.Write(matRun.Key.Name);
                writer.Write(matRun.Runs.Length);
                foreach (var run in matRun.Runs)
                {
                    writer.Write(run.StartIndex);
                    writer.Write(run.Count);
                }
            }
        }
        public void Deserialize(Chunk chunk, IDataReader reader)
        {
            // --- Build snapshot by reading from reader ---
            var snapshot = new ChunkSnapshot();

            // --- Air runs ---
            int airCount = reader.ReadInt32();
            var airRuns = new AirRun[airCount];
            for (int i = 0; i < airCount; i++)
            {
                airRuns[i] = new AirRun
                {
                    StartIndex = reader.ReadInt32(),
                    Count = reader.ReadInt32(),
                    Discovered = reader.ReadBoolean()
                };
            }
            snapshot.AirRuns = airRuns;

            // --- Solid runs ---
            int solidCount = reader.ReadInt32();
            var solidRuns = new SolidRun[solidCount];
            for (int i = 0; i < solidCount; i++)
            {
                int startIndex = reader.ReadInt32();
                int cellCount = reader.ReadInt32();
                var cells = new BitVector32[cellCount];
                for (int j = 0; j < cellCount; j++)
                    cells[j] = new BitVector32(reader.ReadInt32());

                solidRuns[i] = new SolidRun { StartIndex = startIndex, Cells = cells };
            }
            snapshot.SolidRuns = solidRuns;

            // --- Block runs ---
            int blockRunCount = reader.ReadInt32();
            var blockRuns = new RunIndex<BlockDef>[blockRunCount];
            for (int i = 0; i < blockRunCount; i++)
            {
                string blockName = reader.ReadString();
                var blockDef = Def.GetDef<BlockDef>(blockName);
                int runCount = reader.ReadInt32();
                var runs = new Run[runCount];
                for (int j = 0; j < runCount; j++)
                {
                    runs[j] = new Run
                    {
                        StartIndex = reader.ReadInt32(),
                        Count = reader.ReadInt32()
                    };
                }
                blockRuns[i] = new RunIndex<BlockDef> { Key = blockDef, Runs = runs };
            }
            snapshot.BlockDefRuns = blockRuns;

            // --- Material runs ---
            int materialRunCount = reader.ReadInt32();
            var materialRuns = new RunIndex<MaterialDef>[materialRunCount];
            for (int i = 0; i < materialRunCount; i++)
            {
                string matName = reader.ReadString();
                var matDef = Def.GetDef<MaterialDef>(matName);
                int runCount = reader.ReadInt32();
                var runs = new Run[runCount];
                for (int j = 0; j < runCount; j++)
                {
                    runs[j] = new Run
                    {
                        StartIndex = reader.ReadInt32(),
                        Count = reader.ReadInt32()
                    };
                }
                materialRuns[i] = new RunIndex<MaterialDef> { Key = matDef, Runs = runs };
            }
            snapshot.MaterialDefRuns = materialRuns;

            // --- Apply snapshot to chunk ---
            this.ApplySnapshot(chunk, snapshot);
        }

        private void ApplySnapshot(Chunk chunk, ChunkSnapshot snapshot)
        {
            var cells = chunk.Cells;

            // --- Apply air runs ---
            foreach (var run in snapshot.AirRuns)
            {
                for (int i = 0; i < run.Count; i++)
                {
                    var cell = cells[run.StartIndex + i];
                    cell.Block = BlockDefOf.Air.Block;
                    cell.Discovered = run.Discovered;
                    // BitVector32 / material not needed for air
                }
            }

            // --- Apply solid runs (BitVector32 data) ---
            foreach (var run in snapshot.SolidRuns)
            {
                for (int i = 0; i < run.Cells.Length; i++)
                {
                    var cell = cells[run.StartIndex + i];
                    cell.Data = run.Cells[i];
                    // BlockDef and MaterialDef will be filled below
                }
            }

            // --- Apply block runs ---
            foreach (var runIndex in snapshot.BlockDefRuns)
            {
                var blockDef = runIndex.Key;
                foreach (var run in runIndex.Runs)
                {
                    for (int i = 0; i < run.Count; i++)
                    {
                        var cell = cells[run.StartIndex + i];
                        cell.Block = blockDef.Block;
                    }
                }
            }

            // --- Apply material runs ---
            foreach (var runIndex in snapshot.MaterialDefRuns)
            {
                var matDef = runIndex.Key;
                foreach (var run in runIndex.Runs)
                {
                    for (int i = 0; i < run.Count; i++)
                    {
                        var cell = cells[run.StartIndex + i];
                        cell.Material = matDef;
                    }
                }
            }
        }

    }
}
