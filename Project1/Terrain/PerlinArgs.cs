namespace Start_a_Town_
{
    class PerlinArgs
    {
        public int Octaves;
        public double Persistence;

        public PerlinArgs(int octaves, double persistence)
        {
            Octaves = octaves;
            Persistence = persistence;
        }

        public static PerlinArgs Default
        {
            get { return new PerlinArgs(8, 0.5); }
        }
        public static PerlinArgs Grass
        {
            get { return new PerlinArgs(4, 0.45); }
        }
        public static PerlinArgs Interesting
        {
            get { return new PerlinArgs(6, 0.45f); }
        }
    }
}
