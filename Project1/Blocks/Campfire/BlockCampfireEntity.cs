using Start_a_Town_.Particles;

namespace Start_a_Town_
{
    partial class BlockCampfire
    {
        class BlockCampfireEntity : BlockEntity
        {
            public BlockCampfireEntity(IntVec3 originGlobal)
                : base(originGlobal)
            {
                var switchable = new BlockEntityCompSwitchable();
                this.AddComp(switchable);
                var refuel = new BlockEntityCompRefuelable(100);
                this.AddComp(refuel);
                this.AddComp(new BlockEntityCompWorkstation(IsWorkstation.Types.Baking));
                var lightComp = new BlockEntityLuminance(15, refuel, 1, switchable.IsSwitchedOn);
                this.AddComp(lightComp);
                this.AddComp(new BlockEntityCompParticles(ParticleEmitter.Fire.SetRateFunc(() => (lightComp.Powered && switchable.SwitchedOn) ? 1 : 0)));
                //this.AddComp(new BlockEntityCompDeconstructible());
            }
        }
    }
}
