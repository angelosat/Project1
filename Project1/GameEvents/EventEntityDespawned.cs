namespace Start_a_Town_.GameEvents
{
    class EventEntityDespawned
    {
        static public object[] Write(GameObject entity)
        {
            return new object[] { entity };
        }
        static public void Read(object[] p, out GameObject entity)
        {
            entity = p[0] as GameObject;
        }
    }
}
