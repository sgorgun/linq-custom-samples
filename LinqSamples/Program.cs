using System;
using System.Collections.Generic;
using System.IO;
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
            string path = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\Data\"));
            LinqQueries linqHarness = new LinqQueries(path);
            harnesses.Add(linqHarness);
            CustomSamples linqSamples = new CustomSamples(path);
            harnesses.Add(linqSamples);

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
