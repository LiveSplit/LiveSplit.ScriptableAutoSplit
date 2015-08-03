using Irony.Parsing;
using System;
using System.Linq;

namespace LiveSplit.ASL
{
    public class ASLParser
    {
        public static ASLScript Parse(String code)
        {
            var grammar = new ASLGrammar();
            var parser = new Parser(grammar);
            var tree = parser.Parse(code);

            var rootChilds = tree.Root.ChildNodes;
            var stateNode = rootChilds.Where(x => x.Term.Name == "stateDef").First();
            var methodsNode = rootChilds.Where(x => x.Term.Name == "methodList").First();

            var processName = (String)stateNode.ChildNodes[2].Token.Value;
            var valueDefinitionNodes = stateNode.ChildNodes[5].ChildNodes;

            var state = new ASLState();

            foreach (var valueDefinitionNode in valueDefinitionNodes.Where(x => x.ChildNodes.Count > 0))
            {
                var childNodes = valueDefinitionNode.ChildNodes;
                var type = (String)childNodes[0].Token.Value;
                var identifier = (String)childNodes[1].Token.Value;
                var module = (String)childNodes[3].Token.Value;
                var moduleBase = childNodes[5].ChildNodes.Select(x => (int)x.Token.Value).First();
                var offsets = childNodes[5].ChildNodes.Skip(1).Select(x => (int)x.Token.Value).ToArray();
                var valueDefinition = new ASLValueDefinition() 
                { 
                    Identifier = identifier,
                    Type = type,
                    Pointer = new DeepPointer(module, moduleBase, offsets)
                };
                state.ValueDefinitions.Add(valueDefinition);
            }

            ASLMethod start = null, split = null, isLoading = null, gameTime = null, reset = null;
            foreach (var method in methodsNode.ChildNodes[0].ChildNodes)
            {
                var script = new ASLMethod((String)method.ChildNodes[2].Token.Value);
                var methodName = (String)method.ChildNodes[0].Token.Value;
                switch (methodName)
                {
                    case "start": start = script; break;
                    case "split": split = script; break;
                    case "isLoading": isLoading = script; break;
                    case "gameTime": gameTime = script; break;
                    case "reset": reset = script; break; 
                }
            }

            return new ASLScript(processName, state, start, split, reset, isLoading, gameTime);
        }
    }
}
