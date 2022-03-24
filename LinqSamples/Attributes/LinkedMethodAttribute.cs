using System;

namespace LinqSamples.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class LinkedMethodAttribute : Attribute
    {
        public LinkedMethodAttribute(string methodName)
        {
            this.MethodName = methodName;
        }

        public string MethodName { get; set; }
    }
}
