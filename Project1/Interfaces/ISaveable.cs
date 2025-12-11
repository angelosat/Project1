namespace Start_a_Town_
{
    public interface ISaveableNew<T> where T : ISaveableNew<T>
    {
        SaveTag Save(string name = "");
        static abstract T Create(SaveTag tag);
        T Load(SaveTag tag);
    }
    public interface ISaveableNew
    {
        SaveTag Save(string name = "");
        static abstract ISaveableNew Create(SaveTag tag);
    }
    public interface ISaveable 
    {
        SaveTag Save(string name = "");
        ISaveable Load(SaveTag tag);
    }
}
