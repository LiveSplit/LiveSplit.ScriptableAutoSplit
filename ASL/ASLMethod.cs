using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;

namespace LiveSplit.ASL
{
    public class ASLMethod
    {
        protected dynamic CompiledCode { get; set; }
        public bool IsEmpty { get; }
        public dynamic options;

        public ASLMethod(string code)
        {
            options = new ExpandoObject();
            IsEmpty = string.IsNullOrWhiteSpace(code);
            code = code.Replace("return;", "return null;"); // hack

            using (var provider =
                new Microsoft.CSharp.CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } }))
            {
                string source = $@"
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using LiveSplit.ComponentUtil;
using LiveSplit.Model;
public class CompiledScript
{{
    public string version;
    public double refreshRate;
    public bool pauseOnExit;
    void print(string s)
    {{
        Trace.WriteLine(s);
    }}
    public dynamic Execute(LiveSplitState timer, dynamic old, dynamic current, dynamic vars, Process game)
    {{
        var memory = game;
        var modules = game.ModulesWow64Safe();
        { code }
        return null;
    }}
}}";

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
                parameters.ReferencedAssemblies.Add("LiveSplit.Core.dll");

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

        public dynamic Run(LiveSplitState timer, ASLState old, ASLState current, ExpandoObject vars, Process game)
        {
            // dynamic args can't be ref or out, this is a workaround
            CompiledCode.version = options.version;
            CompiledCode.refreshRate = options.refreshRate;
            CompiledCode.pauseOnExit = options.pauseOnExit;
            var ret = CompiledCode.Execute(timer, old.Data, current.Data, vars, game);
            options.version = CompiledCode.version;
            options.refreshRate = CompiledCode.refreshRate;
            options.pauseOnExit = CompiledCode.pauseOnExit;
            return ret;
        }
    }
}
