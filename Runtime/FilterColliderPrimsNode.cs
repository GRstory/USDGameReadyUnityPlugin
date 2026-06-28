using System.Collections.Generic;
using System.Globalization;
using pxr;
using UnityEngine;
using UnityEngine.Importer;

namespace USDGameReady
{
    // 표준 UsdPhysics 1순위, gameReady:* 커스텀 attribute는 fallback.
    //
    // 출력 시그니처 (기존 + 추가):
    //   colliderPrimPaths      : path → "Box" | "Sphere" | "Capsule" | "Mesh"
    //   colliderSizes          : path → (x=radius, y=*, z=height) 또는 Box의 경우 full dimensions
    //   triggerPaths           : Trigger 마킹된 prim들
    //   colliderApproximations : path → "convexHull" | "boundingCube" | "boundingSphere" | "none" | ...
    //   capsuleAxes            : path → 0=X / 1=Y / 2=Z (Unity CapsuleCollider.direction)
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
            public Dictionary<string, Vector3> colliderCenters = new();
            public HashSet<string> triggerPaths = new();
            public Dictionary<string, string> colliderApproximations = new();
            public Dictionary<string, int> capsuleAxes = new();
        }

        static readonly TfToken k_CollisionAPI = new("PhysicsCollisionAPI");
        static readonly TfToken k_MeshCollisionAPI = new("PhysicsMeshCollisionAPI");
        static readonly TfToken k_CollisionEnabled = new("physics:collisionEnabled");
        static readonly TfToken k_Approximation = new("physics:approximation");

        static readonly TfToken k_SizeAttr = new("size");
        static readonly TfToken k_RadiusAttr = new("radius");
        static readonly TfToken k_HeightAttr = new("height");
        static readonly TfToken k_AxisAttr = new("axis");

        static readonly TfToken k_GRHasCollider = new("gameReady:hasCollider");
        static readonly TfToken k_GRColliderSize = new("gameReady:colliderSize");
        static readonly TfToken k_GRColliderCenter = new("gameReady:colliderCenter");
        static readonly TfToken k_GRIsTrigger = new("gameReady:isTrigger");

        public override void Run()
        {
            var scale = GetMetersPerUnit(Input.stage);
            var prims = Input.stage.Traverse(Usd.UsdTraverseInstanceProxies());

            foreach (var prim in prims)
            {
                var path = prim.GetPath().ToString();
                string colliderType = null;
                Vector3 size = Vector3.one;

                // 1순위: 표준 UsdPhysics
                if (prim.HasAPI(k_CollisionAPI))
                {
                    var enabledAttr = prim.GetAttribute(k_CollisionEnabled);
                    if (enabledAttr.IsValid() && enabledAttr.HasValue())
                    {
                        var s = enabledAttr.Get(UsdTimeCode.EarliestTime()).ToString();
                        if (s == "0" || s.Equals("false", System.StringComparison.OrdinalIgnoreCase))
                            continue;
                    }

                    var typeName = prim.GetTypeName().GetString();
                    switch (typeName)
                    {
                        case "Cube":
                        {
                            colliderType = "Box";
                            var sz = ReadFloat(prim.GetAttribute(k_SizeAttr), 2.0f);
                            size = new Vector3(sz, sz, sz);
                            break;
                        }
                        case "Sphere":
                        {
                            colliderType = "Sphere";
                            var r = ReadFloat(prim.GetAttribute(k_RadiusAttr), 1.0f);
                            size = new Vector3(r, r, r);
                            break;
                        }
                        case "Capsule":
                        case "Cylinder":
                        {
                            colliderType = "Capsule";
                            var r = ReadFloat(prim.GetAttribute(k_RadiusAttr), 0.5f);
                            var h = ReadFloat(prim.GetAttribute(k_HeightAttr), 1.0f);
                            // USD Capsule: height = cylinder portion only (반구 제외)
                            // Unity Capsule/CharacterController: height = total (반구 포함)
                            float unityHeight = (typeName == "Capsule") ? h + 2.0f * r : h;
                            size = new Vector3(r, unityHeight, unityHeight);

                            int axisIdx = 2; // USD Capsule 기본 axis는 "Z"
                            var axisAttr = prim.GetAttribute(k_AxisAttr);
                            if (axisAttr.IsValid() && axisAttr.HasValue())
                            {
                                var axisStr = Vt.VtValueToTfToken(axisAttr.Get(UsdTimeCode.EarliestTime())).GetString();
                                axisIdx = axisStr switch
                                {
                                    "X" => 0,
                                    "Y" => 1,
                                    "Z" => 2,
                                    _ => 2
                                };
                            }
                            Output.capsuleAxes[path] = axisIdx;
                            break;
                        }
                        case "Mesh":
                        {
                            colliderType = "Mesh";
                            if (prim.HasAPI(k_MeshCollisionAPI))
                            {
                                var approxAttr = prim.GetAttribute(k_Approximation);
                                if (approxAttr.IsValid() && approxAttr.HasValue())
                                {
                                    var approx = Vt.VtValueToTfToken(approxAttr.Get(UsdTimeCode.EarliestTime())).GetString();
                                    if (!string.IsNullOrEmpty(approx))
                                        Output.colliderApproximations[path] = approx;
                                }
                            }
                            break;
                        }
                    }
                }

                // 2순위 (fallback): gameReady:hasCollider
                if (colliderType == null)
                {
                    var typeAttr = prim.GetAttribute(k_GRHasCollider);
                    if (typeAttr.IsValid() && typeAttr.HasValue())
                    {
                        var typeValue = Vt.VtValueToTfToken(typeAttr.Get(UsdTimeCode.EarliestTime())).GetString();
                        if (!string.IsNullOrEmpty(typeValue))
                        {
                            colliderType = typeValue;

                            var sizeAttr = prim.GetAttribute(k_GRColliderSize);
                            if (sizeAttr.IsValid() && sizeAttr.HasValue())
                            {
                                var v = Vt.VtValueToGfVec3f(sizeAttr.Get(UsdTimeCode.EarliestTime()));
                                size = new Vector3(v[0], v[1], v[2]);
                            }
                        }
                    }
                }

                if (colliderType == null)
                    continue;

                Output.colliderPrimPaths[path] = colliderType;
                Output.colliderSizes[path] = size * scale;

                var centerAttr = prim.GetAttribute(k_GRColliderCenter);
                if (centerAttr.IsValid() && centerAttr.HasValue())
                {
                    var v = Vt.VtValueToGfVec3f(centerAttr.Get(UsdTimeCode.EarliestTime()));
                    Output.colliderCenters[path] = new Vector3(v[0], v[1], v[2]) * scale;
                }

                // Trigger: UsdPhysics 코어엔 isTrigger 없음 → gameReady:isTrigger 만 사용
                var triggerAttr = prim.GetAttribute(k_GRIsTrigger);
                if (triggerAttr.IsValid() && triggerAttr.HasValue())
                {
                    var s = triggerAttr.Get(UsdTimeCode.EarliestTime()).ToString();
                    if (s == "1" || s.Equals("true", System.StringComparison.OrdinalIgnoreCase))
                        Output.triggerPaths.Add(path);
                }
            }
        }

        static float ReadFloat(UsdAttribute attr, float fallback)
        {
            if (!attr.IsValid() || !attr.HasValue()) return fallback;
            var s = attr.Get(UsdTimeCode.EarliestTime()).ToString();
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : fallback;
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
