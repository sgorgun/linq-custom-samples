using System.Collections;
using System.Reflection;
using System.Text;
using LinqSamples.Attributes;

namespace LinqSamples
{
    public class SampleHarness : IEnumerable<Sample>
    {
        private readonly IDictionary<int, Sample> samples = new Dictionary<int, Sample>();

        public SampleHarness()
        {
            Type samplesType = this.GetType();

            this.Title = "Samples";
            string prefix = "Sample";
            string codeFile = $"{samplesType.Name}.cs";

            foreach (Attribute a in samplesType.GetCustomAttributes(false))
            {
                if (a is TitleAttribute attribute)
                {
                    this.Title = attribute.Title;
                }
                else if (a is PrefixAttribute prefixAttribute)
                {
                    prefix = prefixAttribute.Prefix;
                }
            }

            string path = Path.GetFullPath(Path.Combine(Application.StartupPath, $@"..\..\..\{codeFile}"));

            string allCode = ReadFile(path);

            var methods =
                from sm in samplesType.GetMethods(BindingFlags.Public | BindingFlags.Instance |
                                                  BindingFlags.DeclaredOnly | BindingFlags.Static)
                where sm.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                orderby sm.MetadataToken
                select sm;

            int m = 1;
            foreach (var method in methods)
            {
                string methodCategory = "Miscellaneous";
                string methodTitle = $"{prefix} Sample {m}";
                string methodDescription = "See code.";
                List<MethodInfo> linkedMethods = new List<MethodInfo>();
                List<Type> linkedClasses = new List<Type>();

                foreach (Attribute a in method.GetCustomAttributes(false))
                {
                    if (a is CategoryAttribute attribute)
                    {
                        methodCategory = attribute.Category;
                    }
                    else if (a is TitleAttribute titleAttribute)
                    {
                        methodTitle = titleAttribute.Title;
                    }
                    else if (a is DescriptionAttribute descriptionAttribute)
                    {
                        methodDescription = descriptionAttribute.Description;
                    }
                    else if (a is LinkedMethodAttribute methodAttribute)
                    {
                        MethodInfo linked = samplesType.GetMethod(methodAttribute.MethodName, (BindingFlags.Public | BindingFlags.NonPublic) | (BindingFlags.Static | BindingFlags.Instance));

                        if (linked != null)
                        {
                            linkedMethods.Add(linked);
                        }
                    }
                    else if (a is LinkedClassAttribute classAttribute)
                    {
                        Type linked = samplesType.GetNestedType(classAttribute.ClassName);
                        if (linked != null)
                        {
                            linkedClasses.Add(linked);
                        }
                    }
                }

                StringBuilder methodCode = new StringBuilder();
                methodCode.Append(GetCodeBlock(allCode, "void " + method.Name));

                foreach (MethodInfo lm in linkedMethods)
                {
                    methodCode.Append(Environment.NewLine);
                    methodCode.Append(GetCodeBlock(allCode, $"{ShortTypeName(lm.ReturnType.FullName)} {lm?.Name}"));
                }

                foreach (Type lt in linkedClasses)
                {
                    methodCode.Append(Environment.NewLine);
                    methodCode.Append(GetCodeBlock(allCode, $"class {lt?.Name}"));
                }

                Sample sample = new Sample(this, method, methodCategory, methodTitle, methodDescription, methodCode.ToString());

                this.samples.Add(m, sample);
                m++;
            }
        }

        public string Title { get; }

        public StreamWriter OutputStreamWriter { get; } = new (new MemoryStream());

        public Sample this[int index] => this.samples[index];

        public virtual void InitSample() { }

        public virtual void HandleException(Exception exception)
        {
            Console.Write(exception);
        }

        public void RunAllSamples()
        {
            TextWriter oldConsoleOut = Console.Out;
            Console.SetOut(StreamWriter.Null);

            foreach (Sample sample in this)
            {
                sample.Invoke();
            }

            Console.SetOut(oldConsoleOut);
        }

        IEnumerator<Sample> IEnumerable<Sample>.GetEnumerator()
        {
            return this.samples.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.samples.Values.GetEnumerator();
        }

        private static string ReadFile(string path)
        {
            string fileContents = null!;

            if (File.Exists(path))
            {
                using StreamReader reader = File.OpenText(path);
                fileContents = reader.ReadToEnd();
            }
            else
            {
                fileContents = string.Empty;
            }

            return fileContents;
        }

        private static string ShortTypeName(string typeName)
        {
            bool isAssemblyQualified = typeName?[0] == '[';
            if (isAssemblyQualified)
            {
                if (typeName != null)
                {
                    int commaPos = typeName.IndexOf(',', StringComparison.InvariantCulture);
                    return ShortTypeName(typeName.Substring(1, commaPos - 1));
                }
            }

            bool isGeneric = typeName != null && typeName.Contains("`", StringComparison.InvariantCulture);
            if (isGeneric)
            {
                if (typeName != null)
                {
                    int backTickPos = typeName.IndexOf('`', StringComparison.InvariantCulture);
                    int leftBracketPos = typeName.IndexOf('[', StringComparison.InvariantCulture);
                    string typeParam =
                        ShortTypeName(typeName.Substring(leftBracketPos + 1, typeName.Length - leftBracketPos - 2));
                    return ShortTypeName($"{typeName.Substring(0, backTickPos)}<{typeParam}>");
                }
            }

            switch (typeName)
            {
                case "System.Void": return "void";
                case "System.Int16": return "short";
                case "System.Int32": return "int";
                case "System.Int64": return "long";
                case "System.Single": return "float";
                case "System.Double": return "double";
                case "System.String": return "string";
                case "System.Char": return "char";
                case "System.Boolean": return "bool";
                /* other primitive types omitted */
                default:
                    int lastDotPos = typeName.LastIndexOf('.');
                    int lastPlusPos = typeName.LastIndexOf('+');
                    int startPos = Math.Max(lastDotPos, lastPlusPos) + 1;
                    return typeName.Substring(startPos, typeName.Length - startPos);
            }
        }

        private static string GetCodeBlock(string allCode, string blockName)
        {
            int blockStart = allCode.IndexOf(blockName, StringComparison.OrdinalIgnoreCase);

            if (blockStart == -1)
            {
                return "// " + blockName + " code not found";
            }

            blockStart = allCode.LastIndexOf(Environment.NewLine, blockStart, StringComparison.OrdinalIgnoreCase);
            if (blockStart == -1)
            {
                blockStart = 0;
            }
            else
            {
                blockStart += Environment.NewLine.Length;
            }

            int pos = blockStart;
            int braceCount = 0;
            char c;
            do
            {
                pos++;

                c = allCode[pos];
                switch (c)
                {
                    case '{':
                        braceCount++;
                        break;

                    case '}':
                        braceCount--;
                        break;
                }
            }
            while (pos < allCode.Length && !(c == '}' && braceCount == 0));

            int blockEnd = pos;

            string blockCode = allCode.Substring(blockStart, blockEnd - blockStart + 1);

            return RemoveIndent(blockCode);
        }

        private static string RemoveIndent(string code)
        {
            int indentSpaces = 0;
            while (code[indentSpaces] == ' ')
            {
                indentSpaces++;
            }

            StringBuilder builder = new StringBuilder();
            string[] codeLines = code.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (string line in codeLines)
            {
                if (indentSpaces < line.Length)
                {
                    builder.AppendLine(line.Substring(indentSpaces));
                }
                else
                {
                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }
    }
}
