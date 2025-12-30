using System;
using System.Collections.Generic;
using System.Text;

namespace Start_a_Town_
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
