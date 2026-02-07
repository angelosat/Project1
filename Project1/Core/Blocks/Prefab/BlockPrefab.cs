using Project1.Core.Blocks;
using Project1.Core.Graphics;

namespace Project1.Core
{
    partial class BlockPrefab : Block
    {
        static readonly AtlasDepthNormals.Node.Token Token = Block.Atlas.Load("blocks/blockblueprint");
        public BlockPrefab()
            : base("Prefab", 0, 1, true, true)
        {

        }
        public override AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            return Token;
        }
    }
}
