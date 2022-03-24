using System;
using System.Collections.Generic;

namespace LinqSamples.Comparers
{
    public class AnagramEqualityComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            if (x is null && y is null)
            {
                return true;
            }
            else
            {
                return this.GetCanonicalString(x) == this.GetCanonicalString(y);
            }
        }

        public int GetHashCode(string obj)
        {
            return this.GetCanonicalString(obj).GetHashCode();
        }

        private string GetCanonicalString(string word)
        {
            char[] wordChars = word.ToCharArray();
            Array.Sort(wordChars);
            return new string(wordChars);
        }
    }
}
