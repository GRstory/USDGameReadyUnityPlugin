using System.Collections.Generic;
using pxr;
using UnityEngine.Importer;

namespace USDGameReady
{
    [NodeMetadata("FilterAudioSourcePrimsNode", 0)]
    public class FilterAudioSourcePrimsNode : Node<FilterAudioSourcePrimsNode.InputPort, FilterAudioSourcePrimsNode.OutputPort>
    {
        public class InputPort : InputPorts
        {
            public UsdStage stage;
        }

        public class OutputPort : OutputPorts
        {
            public HashSet<string> audioSourcePaths = new();
        }

        public override void Run()
        {
            var token = new TfToken("gameReady:hasAudioSource");
            var prims = Input.stage.Traverse(Usd.UsdTraverseInstanceProxies());

            foreach (var prim in prims)
            {
                var attr = prim.GetAttribute(token);
                if (!attr.IsValid() || !attr.HasValue())
                    continue;

                var s = attr.Get(UsdTimeCode.EarliestTime()).ToString();
                if (s == "1" || s.Equals("true", System.StringComparison.OrdinalIgnoreCase))
                    Output.audioSourcePaths.Add(prim.GetPath().ToString());
            }
        }
    }
}
