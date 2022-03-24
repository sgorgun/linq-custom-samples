using System;

namespace LinqSamples.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class LinkedClassAttribute : Attribute
    {
        public LinkedClassAttribute(string className)
        {
            this.ClassName = className;
        }

        public string ClassName { get; set; }
    }
}
