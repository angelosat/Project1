namespace Project1.Framework.Helpers
{
    public static class StringExtentions
    {
        static public int GetStableHash(this string s)
        {
            unchecked
            {
                int hash = 23;
                foreach (char c in s)
                    hash = hash * 31 + c;
                return hash;
            }
        }
    }
}
