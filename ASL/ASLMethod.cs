using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using LiveSplit.Model;

namespace LiveSplit.ASL
{
    public class ASLMethod
    {
        public bool IsEmpty { get; }

        private dynamic _compiled_code;

        public ASLMethod(string code)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            IsEmpty = string.IsNullOrWhiteSpace(code);
            code = code.Replace("return;", "return null;"); // hack

            var options = new Dictionary<string, string> {
                { "CompilerVersion", "v4.0" }
            };

            using (var provider = new Microsoft.CSharp.CSharpCodeProvider(options))
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
using System.Windows.Forms;
using LiveSplit.ComponentUtil;
using LiveSplit.Model;
public class CompiledScript
{{
    public string version;
    public double refreshRate;
    void print(string s)
    {{
        Trace.WriteLine(s);
    }}
    public dynamic Execute(LiveSplitState timer, dynamic old, dynamic current, dynamic vars, Process game, dynamic settings)
    {{
        var memory = game;
        var modules = game != null ? game.ModulesWow64Safe() : null;
	    { code }
	    return null;
    }}
}}";

                var parameters = new System.CodeDom.Compiler.CompilerParameters() {
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
                    throw new ArgumentException(errorMessage, nameof(code));
                }

                var type = res.CompiledAssembly.GetType("CompiledScript");
                _compiled_code = Activator.CreateInstance(type);
            }
        }

        public dynamic Call(LiveSplitState timer, ExpandoObject vars, ref string version, ref double refreshRate,
            dynamic settings, ExpandoObject old = null, ExpandoObject current = null, Process game = null)
        {
            // dynamic args can't be ref or out, this is a workaround
            _compiled_code.version = version;
            _compiled_code.refreshRate = refreshRate;
            var ret = _compiled_code.Execute(timer, old, current, vars, game, settings);
            version = _compiled_code.version;
            refreshRate = _compiled_code.refreshRate;
            return ret;
        }
    }
}
