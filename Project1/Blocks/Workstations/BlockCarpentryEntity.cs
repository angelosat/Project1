namespace Start_a_Town_
{
        class BlockCarpentryEntity : BlockEntityWorkstation// BlockEntity
        {
            public BlockCarpentryEntity(IntVec3 originGlobal)
                : base(originGlobal)
            {
                this.AddComp(new BlockEntityCompWorkstation(IsWorkstation.Types.Carpentry));
                //this.AddComp(new BlockEntityCompDeconstructible());
            }
        }
}
