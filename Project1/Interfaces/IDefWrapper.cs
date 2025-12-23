namespace Start_a_Town_
{
    //public interface IDefWrapper
    //{
    //    Def Def { get; }
    //}

    public interface IDefWrapper<T> where T : Def//, ISerializableNew<T>
    {
        T Def { get; }
    }
}
