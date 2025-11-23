namespace Start_a_Town_
{
    public interface ISaveable 
    {
        SaveTag Save(string name = "");
        ISaveable Load(SaveTag tag);
    }
}
