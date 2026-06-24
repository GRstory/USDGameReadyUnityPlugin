using System.Collections.Generic;
using pxr;
using UnityEngine.Importer;

namespace USDGameReady
{
    [NodeMetadata("FilterPlayerPrimsNode", 0)]
    public class FilterPlayerPrimsNode : Node<FilterPlayerPrimsNode.InputPort, FilterPlayerPrimsNode.OutputPort>
    {
        public class InputPort : InputPorts
        {
            public UsdStage stage;
        }

        public class OutputPort : OutputPorts
        {
            public HashSet<string> playerPrimPaths = new();
        }

        public override void Run()
        {
            var playerToken = new TfToken("gameReady:isPlayer");
            var prims = Input.stage.Traverse(Usd.UsdTraverseInstanceProxies());
            foreach (var prim in prims)
            {
                var attr = prim.GetAttribute(playerToken);
                if (attr.IsValid() && attr.HasValue())
                    Output.playerPrimPaths.Add(prim.GetPath().ToString());
            }
        }
    }
}
