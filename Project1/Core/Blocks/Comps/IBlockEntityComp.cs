namespace Start_a_Town_
{
    public interface IBlockEntityComp
    {
        //void Tick(MapBase map, IBlockEntityCompContainer entity);
        void Draw(Camera camera, MapBase map, IntVec3 global);
       
        void Load(SaveTag tag);
        SaveTag Save(string name);
    }
}
