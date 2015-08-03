using LiveSplit.Model;
using System;
using System.Text;

namespace LiveSplit.ASL
{
    public class ASLMethod
    {
        protected dynamic CompiledCode { get; set; }

        public ASLMethod(String code)
        {
            using (var provider =
                new Microsoft.CSharp.CSharpCodeProvider())
            {
                var builder = new StringBuilder();
                builder
                    .AppendLine("using System;")
                    .AppendLine("using System.Collections.Generic;")
                    .AppendLine("using System.Linq;")
                    .AppendLine("using System.Reflection;")
                    .AppendLine("using System.Text;")
                    .AppendLine("public class CompiledScript")
                    .AppendLine("{")
                        .AppendLine("public dynamic Execute(dynamic timer, dynamic old, dynamic current)")
                        .AppendLine("{")
                            .Append(code)
                            .Append("return null;")
                        .AppendLine("}")
                    .AppendLine("}");

                var parameters = new System.CodeDom.Compiler.CompilerParameters()
                    {
                        GenerateInMemory = true,
                        CompilerOptions = "/optimize",
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

                var res = provider.CompileAssemblyFromSource(parameters, builder.ToString());

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

        public dynamic Run(LiveSplitState timer, ASLState old, ASLState current)
        {
            return CompiledCode.Execute(timer, old.Data, current.Data);
        }
    }
}
