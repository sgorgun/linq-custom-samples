using System;

namespace LinqSamples.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CategoryAttribute : Attribute
    {
        public CategoryAttribute(string category)
        {
            this.Category = category;
        }

        public string Category { get; set; }
    }
}
