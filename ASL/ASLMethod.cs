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

        public ASLMethod(string code) : this(code, false)
        {

        }

        public ASLMethod(string code, bool preInit)
        {
            IsEmpty = string.IsNullOrWhiteSpace(code);
            code = code.Replace("return;", "return null;"); // hack

            // Decide if this code already has access to the game
            var codeRegular = preInit ? "" : code;
            var codePreInit = preInit ? code : "";

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
    void print(string s)
    {{
        Trace.WriteLine(s);
    }}
    public dynamic Execute(LiveSplitState timer, dynamic old, dynamic current, dynamic vars, Process game, dynamic Enabled)
    {{
        var memory = game;
        var modules = game.ModulesWow64Safe();
	    { codeRegular }
	    return null;
    }}

    public dynamic Execute(LiveSplitState timer, dynamic vars, dynamic addSetting)
    {{
	    { codePreInit }
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

        public dynamic Run(LiveSplitState timer, ASLState old, ASLState current, ExpandoObject vars, Process game, ref string version, ref double refreshRate,
            ASLSettings settings)
        {
            // dynamic args can't be ref or out, this is a workaround
            CompiledCode.version = version;
            CompiledCode.refreshRate = refreshRate;

            Func<string, bool> getSetting = (name) =>
            {
                return settings.GetSettingValue(name);
            };

            var ret = CompiledCode.Execute(timer, old.Data, current.Data, vars, game, getSetting);
            version = CompiledCode.version;
            refreshRate = CompiledCode.refreshRate;
            return ret;
        }

        public dynamic Run(LiveSplitState timer, ExpandoObject vars, ref string version, ref double refreshRate,
            ASLSettings settings)
        {
            // dynamic args can't be ref or out, this is a workaround
            CompiledCode.version = version;
            CompiledCode.refreshRate = refreshRate;

            Action<string, bool, string> addSetting = (name, value, label) =>
            {
                settings.AddSetting(name, value, label);
            };

            var ret = CompiledCode.Execute(timer, vars, addSetting);
            version = CompiledCode.version;
            refreshRate = CompiledCode.refreshRate;
            return ret;
        }

    }
}
