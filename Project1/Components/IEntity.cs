namespace Start_a_Town_.Components
{
    public interface IEntity
    {
        T GetComponent<T>(string name) where T : EntityComponent;
        bool TryGetComponent<T>(string name, out T component) where T : EntityComponent;
        void Update();
    }
}
