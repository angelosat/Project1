namespace Start_a_Town_
{
    public partial class Map
    {
        public struct MapSize
        {
            public string Name { get; set; }
            public int Size { get; set; }
            public MapSize(string name, int size)
                : this()
            {
                Name = name;
                Size = size;
            }
        }
    }
}
