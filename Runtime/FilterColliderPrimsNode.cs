using System.Collections.Generic;
using pxr;
using UnityEngine;
using UnityEngine.Importer;

namespace USDGameReady
{
    [NodeMetadata("FilterColliderPrimsNode", 0)]
    public class FilterColliderPrimsNode : Node<FilterColliderPrimsNode.InputPort, FilterColliderPrimsNode.OutputPort>
    {
        public class InputPort : InputPorts
        {
            public UsdStage stage;
        }

        public class OutputPort : OutputPorts
        {
            public Dictionary<string, string> colliderPrimPaths = new();
            public Dictionary<string, Vector3> colliderSizes = new();
            public HashSet<string> triggerPaths = new();
        }

        public override void Run()
        {
            var typeToken    = new TfToken("gameReady:hasCollider");
            var sizeToken    = new TfToken("gameReady:colliderSize");
            var triggerToken = new TfToken("gameReady:isTrigger");
            var prims = Input.stage.Traverse(Usd.UsdTraverseInstanceProxies());

            foreach (var prim in prims)
            {
                var typeAttr = prim.GetAttribute(typeToken);
                if (!typeAttr.IsValid() || !typeAttr.HasValue())
                    continue;

                var typeValue = Vt.VtValueToTfToken(typeAttr.Get(UsdTimeCode.EarliestTime())).GetString();
                if (string.IsNullOrEmpty(typeValue))
                    continue;

                var path = prim.GetPath().ToString();
                Output.colliderPrimPaths[path] = typeValue;

                var sizeAttr = prim.GetAttribute(sizeToken);
                if (sizeAttr.IsValid() && sizeAttr.HasValue())
                {
                    var v = Vt.VtValueToGfVec3f(sizeAttr.Get(UsdTimeCode.EarliestTime()));
                    Output.colliderSizes[path] = new Vector3(v[0], v[1], v[2]);
                }

                var triggerAttr = prim.GetAttribute(triggerToken);
                if (triggerAttr.IsValid() && triggerAttr.HasValue())
                {
                    var s = triggerAttr.Get(UsdTimeCode.EarliestTime()).ToString();
                    if (s == "1" || s.Equals("true", System.StringComparison.OrdinalIgnoreCase))
                        Output.triggerPaths.Add(path);
                }
            }
        }
    }
}
