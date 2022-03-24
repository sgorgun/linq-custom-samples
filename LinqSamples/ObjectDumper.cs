using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace LinqSamples
{
    public class ObjectDumper
    {
        private readonly TextWriter writer;
        private readonly int depth;
        private int position;
        private int level;

        private ObjectDumper(int depth) => (this.writer, this.depth) = (Console.Out, depth);

        public static void Write(object obj) => Write(obj, 0);

        public static void Write(object obj, int depth)
        {
            ObjectDumper dumper = new (depth);
            dumper.WriteObject(null!, obj);
        }

        private void Write(string s)
        {
            if (s != null)
            {
               this.writer.Write(s);
               this.position += s.Length;
            }
        }

        private void WriteIndent()
        {
            for (int i = 0; i < this.level; i++)
            {
                this.writer.Write("  ");
            }
        }

        private void WriteLine()
        {
            this.writer.WriteLine();
            this.position = 0;
        }

        private void WriteTab()
        {
            this.Write("  ");
            while (this.position % 8 != 0)
            {
                this.Write(" ");
            }
        }

        private void WriteObject(string prefix, object obj)
        {
            if (obj is null || obj is ValueType || obj is string)
            {
                this.WriteIndent();
                this.Write(prefix);
                this.WriteValue(obj);
                this.WriteLine();
            }
            else if (obj is IEnumerable enumerable)
            {
                foreach (object element in enumerable)
                {
                    if (element is IEnumerable && element is not string)
                    {
                        this.WriteIndent();
                        this.Write(prefix);
                        this.Write("...");
                        this.WriteLine();
                        if (this.level < this.depth)
                        {
                            this.level++;
                            this.WriteObject(prefix, element);
                            this.level--;
                        }
                    }
                    else
                    {
                        this.WriteObject(prefix, element);
                    }
                }
            }
            else
            {
                MemberInfo[] members = obj.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
                this.WriteIndent();
                this.Write(prefix);
                bool propWritten = false;
                foreach (MemberInfo m in members)
                {
                    FieldInfo f = m as FieldInfo;
                    PropertyInfo p = m as PropertyInfo;
                    if (f != null || p != null)
                    {
                        if (propWritten)
                        {
                            this.WriteTab();
                        }
                        else
                        {
                            propWritten = true;
                        }

                        this.Write(m.Name);
                        this.Write("=");
                        Type t = f != null ? f.FieldType : p.PropertyType;
                        if (t.IsValueType || t == typeof(string))
                        {
                            this.WriteValue(f != null ? f.GetValue(obj) : p.GetValue(obj, null));
                        }
                        else
                        {
                            this.Write(typeof(IEnumerable).IsAssignableFrom(t) ? "..." : "{ }");
                        }
                    }
                }

                if (propWritten)
                {
                    this.WriteLine();
                }

                if (this.level < this.depth)
                {
                    foreach (MemberInfo m in members)
                    {
                        FieldInfo f = m as FieldInfo;
                        PropertyInfo p = m as PropertyInfo;
                        if (f != null || p != null)
                        {
                            Type t = f != null ? f.FieldType : p.PropertyType;
                            if (!(t.IsValueType || t == typeof(string)))
                            {
                                object value = f != null ? f.GetValue(obj) : p.GetValue(obj, null);
                                if (value != null)
                                {
                                    this.level++;
                                    this.WriteObject(m.Name + ": ", value);
                                    this.level--;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void WriteValue(object o)
        {
            if (o == null)
            {
                this.Write("null");
            }
            else if (o is DateTime)
            {
                this.Write(((DateTime)o).ToShortDateString());
            }
            else if (o is ValueType || o is string)
            {
                this.Write(o.ToString());
            }
            else if (o is IEnumerable)
            {
                this.Write("...");
            }
            else
            {
                this.Write("{ }");
            }
        }
    }
}
