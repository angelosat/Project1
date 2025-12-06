namespace Start_a_Town_
{
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
