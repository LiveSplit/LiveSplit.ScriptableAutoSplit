using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using LiveSplit.ComponentUtil;

namespace LiveSplit.ASL
{
    public class ASLParser
    {
        public static ASLScript Parse(String code)
        {
            var grammar = new ASLGrammar();
            var parser = new Parser(grammar);
            var tree = parser.Parse(code);

            if (tree.HasErrors())
                throw new Exception("ASL parse error(s): " + String.Join("\n", tree.ParserMessages));

            var rootChilds = tree.Root.ChildNodes;
            var methodsNode = rootChilds.First(x => x.Term.Name == "methodList");
            var statesNode = rootChilds.First(x => x.Term.Name == "stateList");

            var states = new Dictionary<string, List<ASLState>>();

            foreach (var stateNode in statesNode.ChildNodes)
            {
                var processName = (String)stateNode.ChildNodes[2].Token.Value;
                var version = stateNode.ChildNodes[3].ChildNodes.Skip(1).Select(x => (String)x.Token.Value).FirstOrDefault() ?? String.Empty;
                var valueDefinitionNodes = stateNode.ChildNodes[6].ChildNodes;

                var state = new ASLState();

                foreach (var valueDefinitionNode in valueDefinitionNodes.Where(x => x.ChildNodes.Count > 0))
                {
                    var childNodes = valueDefinitionNode.ChildNodes;
                    var type = (String)childNodes[0].Token.Value;
                    var identifier = (String)childNodes[1].Token.Value;
                    var module = childNodes[3].ChildNodes.Take(1).Select(x => (String)x.Token.Value).FirstOrDefault() ?? String.Empty;
                    var moduleBase = childNodes[4].ChildNodes.Select(x => (int)x.Token.Value).First();
                    var offsets = childNodes[4].ChildNodes.Skip(1).Select(x => (int)x.Token.Value).ToArray();
                    var valueDefinition = new ASLValueDefinition()
                    {
                        Identifier = identifier,
                        Type = type,
                        Pointer = new DeepPointer(module, moduleBase, offsets)
                    };
                    state.ValueDefinitions.Add(valueDefinition);
                }

                state.GameVersion = version;
                if (!states.ContainsKey(processName))
                    states.Add(processName, new List<ASLState>());
                states[processName].Add(state);
            }

            ASLMethod init = null, update = null, start = null, split = null, isLoading = null, gameTime = null, reset = null;
            foreach (var method in methodsNode.ChildNodes[0].ChildNodes)
            {
                var script = new ASLMethod((String)method.ChildNodes[2].Token.Value);
                var methodName = (String)method.ChildNodes[0].Token.Value;
                switch (methodName)
                {
                    case "init": init = script; break;
                    case "update": update = script; break;
                    case "start": start = script; break;
                    case "split": split = script; break;
                    case "isLoading": isLoading = script; break;
                    case "gameTime": gameTime = script; break;
                    case "reset": reset = script; break; 
                }
            }

            return new ASLScript(states, init, update, start, split, reset, isLoading, gameTime);
        }
    }
}
