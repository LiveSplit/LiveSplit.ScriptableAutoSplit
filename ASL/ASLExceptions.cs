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
        static readonly Regex _stackTraceRegex = new Regex(@".+:line (\d+)", RegexOptions.IgnoreCase);

        public ASLRuntimeException(ASLMethod method, Exception innerException)
            : base(GetMessage(method, innerException), innerException)
        { }

        static string GetMessage(ASLMethod method, Exception innerException)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (innerException == null)
                throw new ArgumentNullException(nameof(innerException));

            var lineStr = string.Empty;
            var match = _stackTraceRegex.Match(innerException.StackTrace);
            if (match != null)
            {
                var line = int.Parse(match.Groups[1].Value);
                line = method.Line + line - method.CompiledCodeLine;
                lineStr = $" at line {line}";
            }

            var exceptionName = innerException.GetType().FullName;
            var methodName = method.Name ?? "(no name)";
            var message = innerException.Message;
            return $"Exception thrown: '{exceptionName}' in '{methodName}' method{lineStr}:\n{message}";
        }
    }
}
