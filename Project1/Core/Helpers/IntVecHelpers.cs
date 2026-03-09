using Project1.Core.Simulation;
using Project1.Framework;

namespace Project1.Core.Helpers
{
    internal static class IntVecHelpers
    {
        public static IntVec3 ToGlobal(this IntVec3 vec, Chunk chunk)
        {
            return new IntVec3(chunk.Start.X + vec.X, chunk.Start.Y + vec.Y, vec.Z);
        }

        public static IntVec3 ToLocal(this IntVec3 vec)
        {
            float lx, ly;
            lx = vec.X & 15;
            ly = vec.Y & 15;
            //lx = vec.X & (Chunk.Size - 1); 
            //ly = vec.Y & (Chunk.Size - 1);

            //lx = vec.X % Chunk.Size;
            //lx = lx < 0 ? lx + Chunk.Size : lx;
            //ly = vec.Y % Chunk.Size;
            //ly = ly < 0 ? ly + Chunk.Size : ly;

       

            return new IntVec3(lx, ly, vec.Z);
        }

        public static IntVec2 GetChunkCoords(this IntVec2 vec)
        {
            int chunkX = vec.X / Chunk.Size;
            int chunkY = vec.Y / Chunk.Size;
            return new IntVec2(chunkX, chunkY);
        }
    }
}
