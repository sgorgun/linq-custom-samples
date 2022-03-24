using System;

namespace LinqSamples.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class PrefixAttribute : Attribute
    {
        public PrefixAttribute(string prefix)
        {
            this.Prefix = prefix;
        }

        public string Prefix { get; set; }
    }
}
