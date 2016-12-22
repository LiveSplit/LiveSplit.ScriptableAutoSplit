using System;
using System.CodeDom.Compiler;
using System.Text;
using System.Text.RegularExpressions;

namespace LiveSplit.ASL
{
    public class ASLCompilerException : Exception
    {
        public ASLMethod Method { get; }

        public CompilerErrorCollection CompilerErrors { get; }

        public ASLCompilerException(ASLMethod method, CompilerErrorCollection errors)
            : base(GetMessage(method, errors))
        {
            Method = method;
            CompilerErrors = errors;
        }

        static string GetMessage(ASLMethod method, CompilerErrorCollection errors)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            var sb = new StringBuilder($"'{method.Name ?? "(no name)"}' method compilation errors:");
            foreach (CompilerError error in errors)
            {
                error.Line = method.Line + error.Line - method.CompiledCodeLine;
                sb.Append($"\nLine {error.Line}, Col {error.Column}: {(error.IsWarning ? "warning" : "error")} {error.ErrorNumber}: {error.ErrorText}");
            }
            return sb.ToString();
        }
    }

    public class ASLRuntimeException : Exception
    {
        static readonly Regex _stack_trace_regex = new Regex(@".+:line (\d+)", RegexOptions.IgnoreCase);

        public ASLRuntimeException(ASLMethod method, Exception inner_exception)
            : base(GetMessage(method, inner_exception), inner_exception)
        { }

        static string GetMessage(ASLMethod method, Exception inner_exception)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (inner_exception == null)
                throw new ArgumentNullException(nameof(inner_exception));

            var line_str = string.Empty;
            var match = _stack_trace_regex.Match(inner_exception.StackTrace);
            if (match != null)
            {
                var line = int.Parse(match.Groups[1].Value);
                line = method.Line + line - method.CompiledCodeLine;
                line_str = $" at line {line}";
            }

            var exception_name = inner_exception.GetType().FullName;
            var method_name = method.Name ?? "(no name)";
            var message = inner_exception.Message;
            return $"Exception thrown: '{exception_name}' in '{method_name}' method{line_str}:\n{message}";
        }
    }
}
