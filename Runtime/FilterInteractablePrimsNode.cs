using System.Collections.Generic;
using pxr;
using UnityEngine.Importer;

namespace USDGameReady
{
    [NodeMetadata("FilterInteractablePrimsNode", 0)]
    public class FilterInteractablePrimsNode : Node<FilterInteractablePrimsNode.InputPort, FilterInteractablePrimsNode.OutputPort>
    {
        public class InputPort : InputPorts
        {
            public UsdStage stage;
        }

        public class OutputPort : OutputPorts
        {
            public Dictionary<string, List<string>> interactTargets = new();
        }

        static readonly TfToken k_IsInteractable  = new("gameReady:isInteractable");
        static readonly TfToken k_InteractTargets = new("gameReady:interactTargets");

        public override void Run()
        {
            var prims = Input.stage.Traverse(Usd.UsdTraverseInstanceProxies());

            foreach (var prim in prims)
            {
                var attr = prim.GetAttribute(k_IsInteractable);
                if (!attr.IsValid() || !attr.HasValue())
                    continue;

                var s = attr.Get(UsdTimeCode.EarliestTime()).ToString();
                if (s != "1" && !s.Equals("true", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                var path = prim.GetPath().ToString();
                var targetPaths = new List<string>();

                var rel = prim.GetRelationship(k_InteractTargets);
                if (rel.IsValid())
                {
                    var sdfPaths = rel.GetTargets();
                    foreach (var sdfPath in sdfPaths)
                        targetPaths.Add(sdfPath.ToString());
                }

                Output.interactTargets[path] = targetPaths;
            }
        }
    }
}
