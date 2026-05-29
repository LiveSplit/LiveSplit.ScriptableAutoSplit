using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;

using Basic.Reference.Assemblies;

using LiveSplit.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace LiveSplit.ASL;

public class ASLMethod
{
    public ASLScript.Methods ScriptMethods { get; set; }

    public string Name { get; }

    public bool IsEmpty { get; }

    public Module Module { get; }

    private readonly dynamic _compiled_code;

    public ASLMethod(string code, string name = null, int scriptLine = 1)
    {
        if (code == null)
        {
            throw new ArgumentNullException(nameof(code));
        }

        if (scriptLine < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(scriptLine), "Must be greater than or equal to 1.");
        }

        Name = name;
        IsEmpty = string.IsNullOrWhiteSpace(code);
        code = code.Replace("return;", "return null;"); // hack

        string source = $$"""
            using System;
            using System.Buffers;
            using System.Collections.Generic;
            using System.Diagnostics;
            using System.Dynamic;
            using System.IO;
            using System.Linq;
            using System.Memory;
            using System.Reflection;
            using System.Text;
            using System.Text.Json;
            using System.Text.Json.Nodes;
            using System.Text.RegularExpressions;
            using System.Threading;
            using System.Windows.Forms;

            using LiveSplit.ComponentUtil;
            using LiveSplit.Model;
            using LiveSplit.Options;

            public class CompiledScript
            {
                public string version;
                public double refreshRate;

                private void print(string s) => Log.Info(s);

                public dynamic Execute(LiveSplitState timer, dynamic old, dynamic current, dynamic vars, Process game, dynamic settings)
                {
                    var memory = game;
                    var modules = game != null ? game.ModulesWow64Safe() : null;

                    #line {{scriptLine}}
                    {{code}};

                    return null;
                }
            }
            """;

        var compilation_options = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            optimizationLevel: OptimizationLevel.Release,
            allowUnsafe: true);

        var compilation = CSharpCompilation.Create(
            $"ASLCompiledScript_{Guid.NewGuid():N}",
            [CSharpSyntaxTree.ParseText(source, encoding: System.Text.Encoding.UTF8)],
            options: compilation_options,
            references: ScriptReferences);

        // PDB is required. Contains the line numbers for the ASLRuntimeException stack trace.
        using var assemblyStream = new MemoryStream();
        using var pdbStream = new MemoryStream();

        EmitResult emitResult = compilation.Emit(
            assemblyStream,
            pdbStream,
            options: new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb));

        if (!emitResult.Success)
        {
            throw new ASLCompilerException(this, ToCompilerErrors(emitResult.Diagnostics));
        }

        byte[] assemblyBytes = assemblyStream.ToArray();
        byte[] pdbBytes = pdbStream.ToArray();

        Assembly assembly = Assembly.Load(assemblyBytes, pdbBytes);
        Module = assembly.ManifestModule;

        Type type = assembly.GetType("CompiledScript");
        _compiled_code = Activator.CreateInstance(type);
    }

    public dynamic Call(LiveSplitState timer, ExpandoObject vars, ref string version, ref double refreshRate,
        dynamic settings, ExpandoObject old = null, ExpandoObject current = null, Process game = null)
    {
        // dynamic args can't be ref or out, this is a workaround
        _compiled_code.version = version;
        _compiled_code.refreshRate = refreshRate;
        dynamic ret;
        try
        {
            ret = _compiled_code.Execute(timer, old, current, vars, game, settings);
        }
        catch (Exception ex)
        {
            throw new ASLRuntimeException(this, ex);
        }

        version = _compiled_code.version;
        refreshRate = _compiled_code.refreshRate;
        return ret;
    }

    private static MetadataReference[] _scriptReferences;
    private static MetadataReference[] ScriptReferences => _scriptReferences ??=
    [
        .. ReferenceAssemblies.Net472,
        MetadataReference.CreateFromFile(typeof(Span<byte>).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(System.Text.Json.JsonSerializer).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(LiveSplitState).Assembly.Location),
    ];

    private static CompilerErrorCollection ToCompilerErrors(IEnumerable<Diagnostic> diagnostics)
    {
        var errors = new CompilerErrorCollection();
        foreach (Diagnostic diagnostic in diagnostics)
        {
            if (diagnostic.Severity is not DiagnosticSeverity.Error and not DiagnosticSeverity.Warning)
            {
                continue;
            }

            FileLinePositionSpan span = diagnostic.Location.GetMappedLineSpan();
            errors.Add(new CompilerError
            {
                Line = span.IsValid ? span.StartLinePosition.Line + 1 : 0,
                Column = span.IsValid ? span.StartLinePosition.Character + 1 : 0,
                ErrorNumber = diagnostic.Id,
                ErrorText = diagnostic.GetMessage(),
                IsWarning = diagnostic.Severity == DiagnosticSeverity.Warning,
            });
        }

        return errors;
    }
}
