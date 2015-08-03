using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;

namespace LiveSplit.ASL
{
    public class ASLState : ICloneable
    {
        public ExpandoObject Data { get; set; }
        public List<ASLValueDefinition> ValueDefinitions { get; set; }

        public ASLState()
        {
            Data = new ExpandoObject();
            ValueDefinitions = new List<ASLValueDefinition>();
        }

        public ASLState RefreshValues(Process p)
        {
            var clone = (ASLState)Clone();

            var dict = ((IDictionary<string, object>)Data);
            foreach (var valueDefinition in ValueDefinitions)
            {
                var value = GetValue(p, valueDefinition.Type, valueDefinition.Pointer);
                if (dict.ContainsKey(valueDefinition.Identifier))
                {
                    dict[valueDefinition.Identifier] = value;
                }
                else
                {
                    dict.Add(valueDefinition.Identifier, value);
                }
            }
            return clone;
        }

        private dynamic GetValue(Process p, String type, DeepPointer pointer)
        {
            if (type == "int")
            {
                int x;
                pointer.Deref<int>(p, out x);
                return x;
            }
            else if (type == "uint")
            {
                uint x;
                pointer.Deref<uint>(p, out x);
                return x;
            }
            else if (type == "float")
            {
                float x;
                pointer.Deref<float>(p, out x);
                return x;
            }
            else if (type == "double")
            {
                double x;
                pointer.Deref<double>(p, out x);
                return x;
            }
            else if (type == "byte")
            {
                byte x;
                pointer.Deref<byte>(p, out x);
                return x;
            }
            else if (type == "bool")
            {
                bool x;
                pointer.Deref<bool>(p, out x);
                return x;
            }
            else if (type == "short")
            {
                short x;
                pointer.Deref<short>(p, out x);
                return x;
            }
            else if (type == "sbyte")
            {
                sbyte x;
                pointer.Deref<sbyte>(p, out x);
                return x;
            }
            else if (type.StartsWith("string"))
            {
                String x;
                var length = Int32.Parse(type.Substring("string".Length));
                pointer.Deref(p, out x, length);
                return x;
            }
            else if (type.StartsWith("byte"))
            {
                byte[] x;
                var length = Int32.Parse(type.Substring("byte".Length));
                pointer.Deref(p, out x, length);
                return x;
            }
            throw new ArgumentException(string.Format("The provided type, '{0}', is not supported", type));
        }

        public object Clone()
        {
            var clone = new ExpandoObject();
            foreach (var pair in (IDictionary<string, object>)Data)
            {
                ((IDictionary<string, object>)clone).Add(pair);
            }
            return new ASLState() { Data = clone, ValueDefinitions = new List<ASLValueDefinition>(ValueDefinitions) };
        }
    }
}
