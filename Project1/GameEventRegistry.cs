namespace Start_a_Town_
{
    public class GameEventRegistry
    {
        int _nextId = 1000;

        internal int Register()
        {
            return _nextId++;
        }
    }
}
