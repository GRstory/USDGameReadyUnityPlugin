using System.Collections.Generic;
using System.Globalization;
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
            public Dictionary<string, float> slopeAngleLimits = new();
            public Dictionary<string, float> stepHeights = new();
        }

        public override void Run()
        {
            var scale = GetMetersPerUnit(Input.stage);
            var playerToken = new TfToken("gameReady:isPlayer");
            var slopeToken  = new TfToken("gameReady:slopeAngleLimit");
            var stepToken   = new TfToken("gameReady:stepHeight");
            var prims = Input.stage.Traverse(Usd.UsdTraverseInstanceProxies());

            foreach (var prim in prims)
            {
                var attr = prim.GetAttribute(playerToken);
                if (!attr.IsValid() || !attr.HasValue())
                    continue;

                var path = prim.GetPath().ToString();
                Output.playerPrimPaths.Add(path);

                var slopeAttr = prim.GetAttribute(slopeToken);
                if (slopeAttr.IsValid() && slopeAttr.HasValue()
                    && TryParseFloat(slopeAttr.Get(UsdTimeCode.EarliestTime()).ToString(), out var slope))
                    Output.slopeAngleLimits[path] = slope;

                var stepAttr = prim.GetAttribute(stepToken);
                if (stepAttr.IsValid() && stepAttr.HasValue()
                    && TryParseFloat(stepAttr.Get(UsdTimeCode.EarliestTime()).ToString(), out var step))
                    Output.stepHeights[path] = step * scale;
            }
        }

        static bool TryParseFloat(string s, out float value)
        {
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        static readonly VtValue s_FloatTemplate = new VtValue(0.0f);

        static float GetMetersPerUnit(UsdStage stage)
        {
            try
            {
                var val = new VtValue();
                if (stage.GetMetadata(UsdGeomTokens.metersPerUnit, val))
                    return VtValue.CastToTypeOf(val, s_FloatTemplate);
            }
            catch { }
            return 1f;
        }
    }
}
