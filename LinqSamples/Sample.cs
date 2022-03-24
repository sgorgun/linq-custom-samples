using System.Reflection;

namespace LinqSamples
{
    public class Sample
    {
        private readonly SampleHarness harness;
        private readonly MethodInfo method;

        public Sample(SampleHarness harness, MethodInfo method, string category, string title, string description, string code)
        {
            this.harness = harness;
            this.method = method;
            this.Category = category;
            this.Title = title;
            this.Description = description;
            this.Code = code;
        }

        public SampleHarness Harness => this.harness;

        public MethodInfo Method => this.method;

        public string Category { get; }

        public string Title { get; }

        public string Description { get; }

        public string Code { get; }

        public void Invoke()
        {
            this.harness.InitSample();
            this.method.Invoke(this.harness, null);
        }

        public void InvokeSafe()
        {
            try
            {
                this.Invoke();
            }
            catch (TargetInvocationException e)
            {
                this.harness.HandleException(e.InnerException);
            }
        }

        public override string ToString()
        {
            return this.Title;
        }
    }
}
