using Project1.Core.Blocks;
using Project1.Core.Graphics.Particles;
using Project1.Framework.Math;

namespace Project1.Core
{
    partial class BlockCampfire
    {
        class BlockCampfireEntity : BlockEntity
        {
            public BlockCampfireEntity(BlockDef def, IntVec3 originGlobal)
                : base(def, originGlobal)
            {
                var switchable = new BlockEntityCompSwitchable();
                this.AddComp(switchable);
                var refuel = new BlockEntityCompRefuelable(100);
                this.AddComp(refuel);
                this.AddComp(new BlockEntityCompWorkstationOld(IsWorkstation.Types.Baking));
                var lightComp = new BlockEntityLuminance(15, refuel, 1, switchable.IsSwitchedOn);
                this.AddComp(lightComp);
                this.AddComp(new BlockEntityCompParticles(ParticleEmitter.Fire.SetRateFunc(() => (lightComp.Powered && switchable.SwitchedOn) ? 1 : 0)));
            }
        }
    }
}
