using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using LiveSplit.ComponentUtil;

namespace LiveSplit.ASL
{
    public class ASLState : ICloneable
    {
        public ExpandoObject Data { get; set; }
        public List<ASLValueDefinition> ValueDefinitions { get; set; }
        public string GameVersion { get; set; }

        public ASLState()
        {
            Data = new ExpandoObject();
            ValueDefinitions = new List<ASLValueDefinition>();
            GameVersion = string.Empty;
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

        private dynamic GetValue(Process p, string type, DeepPointer pointer)
        {
            if (type == "int")
                return pointer.Deref<int>(p);
            else if (type == "uint")
                return pointer.Deref<uint>(p);
            else if (type == "float")
                return pointer.Deref<float>(p);
            else if (type == "double")
                return pointer.Deref<double>(p);
            else if (type == "byte")
                return pointer.Deref<byte>(p);
            else if (type == "sbyte")
                return pointer.Deref<sbyte>(p);
            else if (type == "short")
                return pointer.Deref<short>(p);
            else if (type == "ushort")
                return pointer.Deref<ushort>(p);
            else if (type == "bool")
                return pointer.Deref<bool>(p);
            else if (type.StartsWith("string"))
            {
                var length = int.Parse(type.Substring("string".Length));
                return pointer.DerefString(p, length);
            }
            else if (type.StartsWith("byte"))
            {
                var length = int.Parse(type.Substring("byte".Length));
                return pointer.DerefBytes(p, length);
            }
            throw new ArgumentException(string.Format("The provided type, '{0}', is not supported", type));
        }

        public object Clone()
        {
            var clone = new ExpandoObject();
            foreach (var pair in Data)
            {
                ((IDictionary<string, object>)clone).Add(pair);
            }
            return new ASLState() { Data = clone, ValueDefinitions = new List<ASLValueDefinition>(ValueDefinitions) };
        }
    }
}
