using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LinqSamples
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            List<SampleHarness> harnesses = new List<SampleHarness>();
            ////Path.Combine(@"C:\Users\MIB\Desktop\LinqQueries", @"\Data\");
            LinqQueries linqHarness = new LinqQueries(@"C:\Users\MIB\Desktop\LinqSamples\LinqSamples\Data\");
            harnesses.Add(linqHarness);

            if (args.Length >= 1 && args[0] == "/runall")
            {
                foreach (SampleHarness harness in harnesses)
                {
                    harness.RunAllSamples();
                }
            }
            else
            {
                Application.EnableVisualStyles();

                using SampleForm form = new SampleForm("LINQ Project Sample Query Explorer", harnesses);

                form.ShowDialog();
            }
        }
    }
}
