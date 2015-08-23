using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using LiveSplit.ComponentUtil;

namespace LiveSplit.ASL
{
    public class ASLMethod
    {
        protected dynamic CompiledCode { get; set; }
        public bool IsEmpty { get; private set; }

        public ASLMethod(String code)
        {
            IsEmpty = string.IsNullOrWhiteSpace(code);
            code = code.Replace("return;", "return null;"); // hack

            using (var provider =
                new Microsoft.CSharp.CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } }))
            {
                string source = String.Format(@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
public class CompiledScript
{{
    public string version;
    void print(string s)
    {{
        System.Diagnostics.Trace.WriteLine(s);
    }}
    public dynamic Execute(dynamic timer, dynamic old, dynamic current, dynamic vars, dynamic game, dynamic modules, dynamic memory)
    {{
	    {0}
	    return null;
    }}
}}", code);

                var parameters = new System.CodeDom.Compiler.CompilerParameters()
                    {
                        GenerateInMemory = true,
                        CompilerOptions = "/optimize /d:TRACE",
                    };
                parameters.ReferencedAssemblies.Add("System.dll");
                parameters.ReferencedAssemblies.Add("System.Core.dll");
                parameters.ReferencedAssemblies.Add("System.Data.dll");
                parameters.ReferencedAssemblies.Add("System.Data.DataSetExtensions.dll");
                parameters.ReferencedAssemblies.Add("System.Drawing.dll");
                parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
                parameters.ReferencedAssemblies.Add("System.Xml.dll");
                parameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
                parameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll");

                var res = provider.CompileAssemblyFromSource(parameters, source);

                if (res.Errors.HasErrors)
                {
                    var errorMessage = "";
                    foreach (var error in res.Errors)
                    {
                        errorMessage += error + "\r\n";
                    }
                    throw new ArgumentException(errorMessage, "code");
                }

                var type = res.CompiledAssembly.GetType("CompiledScript");
                CompiledCode = Activator.CreateInstance(type);
            }
        }

        public dynamic Run(LiveSplitState timer, ASLState old, ASLState current, ExpandoObject vars, Process game,
            ProcessModuleWow64Safe[] modules, MemoryUtilWrapper memory, ref string version)
        {
            // dynamic args can't be ref or out, this is a workaround
            CompiledCode.version = version;
            var ret = CompiledCode.Execute(timer, old.Data, current.Data, vars, game, modules, memory);
            version = CompiledCode.version;
            return ret;
        }
    }
}
