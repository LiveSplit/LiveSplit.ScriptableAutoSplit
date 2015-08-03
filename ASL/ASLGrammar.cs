using Irony.Parsing;
using System.Linq;

namespace LiveSplit.ASL
{
    public class ASLGrammar : Grammar
    {
        public ASLGrammar()
            : base(true)
        {
            var stringLit = TerminalFactory.CreateCSharpString("string");
            var number = TerminalFactory.CreateCSharpNumber("number");
            var identifier = TerminalFactory.CreateCSharpIdentifier("identifier");
            var code = new CustomTerminal("code", MatchCodeTerminal);
            var singleLineComment = new CommentTerminal("SingleLineComment", "//", "\r", "\n", "\u2085", "\u2028", "\u2029");
            var delimitedComment = new CommentTerminal("DelimitedComment", "/*", "*/");
            NonGrammarTerminals.Add(singleLineComment);
            NonGrammarTerminals.Add(delimitedComment);

            var state = new KeyTerm("state", "state");
            var start = new KeyTerm("start", "start");
            var split = new KeyTerm("split", "split");
            var reset = new KeyTerm("reset", "reset");
            var isLoading = new KeyTerm("isLoading", "isLoading");
            var gameTime = new KeyTerm("gameTime", "gameTime");
            var comma = ToTerm(",", "comma");
            var semi = ToTerm(";", "semi");

            var root = new NonTerminal("root");
            var stateDef = new NonTerminal("stateDef");
            var methodList = new NonTerminal("methodList");
            var varList = new NonTerminal("varList");
            var var = new NonTerminal("var");
            var method = new NonTerminal("method");
            var offsetList = new NonTerminal("offsetList");
            var offset = new NonTerminal("offset");
            var methodType = new NonTerminal("methodType");

            root.Rule = stateDef + methodList;
            stateDef.Rule = state + "(" + stringLit + ")" + "{" + varList + "}";
            methodList.Rule = MakeStarRule(methodList, method);
            varList.Rule = MakeStarRule(varList, semi, var);
            var.Rule = (identifier + identifier + ":" + stringLit + comma + offsetList) | Empty;
            method.Rule = (methodType + "{" + code + "}") | Empty;
            offsetList.Rule = MakePlusRule(offsetList, comma, offset);
            offset.Rule = number;
            methodType.Rule = start | split | isLoading | gameTime | reset;

            this.Root = root;

            MarkTransient(varList, methodList, offset, methodType);

            this.LanguageFlags = LanguageFlags.NewLineBeforeEOF;
        }

        Token MatchCodeTerminal(Terminal terminal, ParsingContext context, ISourceStream source)
        {
            var remaining = source.Text.Substring(source.Location.Position);
            var stack = 1;
            var token = "";
            while (stack > 0)
            {
                var index = remaining.IndexOf('}') + 1;
                var cut = remaining.Substring(0, index);
                token += cut;
                remaining = remaining.Substring(index);
                stack += cut.Count(x => x == '{');
                stack--;
            }
            token = token.Substring(0, token.Length - 1);
            source.PreviewPosition += token.Length;
            return source.CreateToken(terminal.OutputTerminal);
        }
    }
}