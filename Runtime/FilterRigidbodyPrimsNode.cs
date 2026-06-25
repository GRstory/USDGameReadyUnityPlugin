using System.Collections.Generic;
using System.Globalization;
using pxr;
using UnityEngine;
using UnityEngine.Importer;

namespace USDGameReady
{
    // 표준 UsdPhysics(PhysicsRigidBodyAPI + PhysicsMassAPI) 1순위, gameReady:* fallback.
    [NodeMetadata("FilterRigidbodyPrimsNode", 0)]
    public class FilterRigidbodyPrimsNode : Node<FilterRigidbodyPrimsNode.InputPort, FilterRigidbodyPrimsNode.OutputPort>
    {
        public class InputPort : InputPorts
        {
            public UsdStage stage;
        }

        public class OutputPort : OutputPorts
        {
            public HashSet<string> rigidbodyPaths = new();
            public Dictionary<string, float> masses = new();
            public HashSet<string> kinematicPaths = new();
            public Dictionary<string, Vector3> linearVelocities = new();
            public Dictionary<string, Vector3> angularVelocities = new();
            public Dictionary<string, Vector3> centerOfMass = new();
            public HashSet<string> useGravityFalsePaths = new(); // gameReady fallback에서만 채워짐
        }

        static readonly TfToken k_RigidBodyAPI = new("PhysicsRigidBodyAPI");
        static readonly TfToken k_MassAPI = new("PhysicsMassAPI");

        static readonly TfToken k_RBEnabled = new("physics:rigidBodyEnabled");
        static readonly TfToken k_Kinematic = new("physics:kinematicEnabled");
        static readonly TfToken k_Mass = new("physics:mass");
        static readonly TfToken k_Velocity = new("physics:velocity");
        static readonly TfToken k_AngVelocity = new("physics:angularVelocity");
        static readonly TfToken k_CenterOfMass = new("physics:centerOfMass");

        static readonly TfToken k_GRHasRigidbody = new("gameReady:hasRigidbody");
        static readonly TfToken k_GRMass = new("gameReady:mass");
        static readonly TfToken k_GRKinematic = new("gameReady:isKinematic");
        static readonly TfToken k_GRUseGravity = new("gameReady:useGravity");

        public override void Run()
        {
            var prims = Input.stage.Traverse(Usd.UsdTraverseInstanceProxies());

            foreach (var prim in prims)
            {
                var path = prim.GetPath().ToString();
                bool hasRigidbody = false;

                // 1순위: 표준 UsdPhysics
                if (prim.HasAPI(k_RigidBodyAPI))
                {
                    var enAttr = prim.GetAttribute(k_RBEnabled);
                    if (enAttr.IsValid() && enAttr.HasValue())
                    {
                        var s = enAttr.Get(UsdTimeCode.EarliestTime()).ToString();
                        if (s == "0" || s.Equals("false", System.StringComparison.OrdinalIgnoreCase))
                            continue;
                    }
                    hasRigidbody = true;

                    var kAttr = prim.GetAttribute(k_Kinematic);
                    if (kAttr.IsValid() && kAttr.HasValue())
                    {
                        var s = kAttr.Get(UsdTimeCode.EarliestTime()).ToString();
                        if (s == "1" || s.Equals("true", System.StringComparison.OrdinalIgnoreCase))
                            Output.kinematicPaths.Add(path);
                    }

                    var velAttr = prim.GetAttribute(k_Velocity);
                    if (velAttr.IsValid() && velAttr.HasValue())
                    {
                        var v = Vt.VtValueToGfVec3f(velAttr.Get(UsdTimeCode.EarliestTime()));
                        Output.linearVelocities[path] = new Vector3(v[0], v[1], v[2]);
                    }

                    var angAttr = prim.GetAttribute(k_AngVelocity);
                    if (angAttr.IsValid() && angAttr.HasValue())
                    {
                        var v = Vt.VtValueToGfVec3f(angAttr.Get(UsdTimeCode.EarliestTime()));
                        Output.angularVelocities[path] = new Vector3(v[0], v[1], v[2]);
                    }

                    if (prim.HasAPI(k_MassAPI))
                    {
                        var massAttr = prim.GetAttribute(k_Mass);
                        if (massAttr.IsValid() && massAttr.HasValue())
                            Output.masses[path] = ReadFloat(massAttr, 1.0f);

                        var comAttr = prim.GetAttribute(k_CenterOfMass);
                        if (comAttr.IsValid() && comAttr.HasValue())
                        {
                            var v = Vt.VtValueToGfVec3f(comAttr.Get(UsdTimeCode.EarliestTime()));
                            Output.centerOfMass[path] = new Vector3(v[0], v[1], v[2]);
                        }
                    }
                }

                // 2순위 (fallback): gameReady:hasRigidbody
                if (!hasRigidbody)
                {
                    var grAttr = prim.GetAttribute(k_GRHasRigidbody);
                    if (grAttr.IsValid() && grAttr.HasValue())
                    {
                        var s = grAttr.Get(UsdTimeCode.EarliestTime()).ToString();
                        if (s == "1" || s.Equals("true", System.StringComparison.OrdinalIgnoreCase))
                        {
                            hasRigidbody = true;

                            var grMass = prim.GetAttribute(k_GRMass);
                            if (grMass.IsValid() && grMass.HasValue())
                                Output.masses[path] = ReadFloat(grMass, 1.0f);

                            var grKin = prim.GetAttribute(k_GRKinematic);
                            if (grKin.IsValid() && grKin.HasValue())
                            {
                                var ks = grKin.Get(UsdTimeCode.EarliestTime()).ToString();
                                if (ks == "1" || ks.Equals("true", System.StringComparison.OrdinalIgnoreCase))
                                    Output.kinematicPaths.Add(path);
                            }

                            var grGrav = prim.GetAttribute(k_GRUseGravity);
                            if (grGrav.IsValid() && grGrav.HasValue())
                            {
                                var gs = grGrav.Get(UsdTimeCode.EarliestTime()).ToString();
                                if (gs == "0" || gs.Equals("false", System.StringComparison.OrdinalIgnoreCase))
                                    Output.useGravityFalsePaths.Add(path);
                            }
                        }
                    }
                }

                if (hasRigidbody)
                    Output.rigidbodyPaths.Add(path);
            }
        }

        static float ReadFloat(UsdAttribute attr, float fallback)
        {
            if (!attr.IsValid() || !attr.HasValue()) return fallback;
            var s = attr.Get(UsdTimeCode.EarliestTime()).ToString();
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : fallback;
        }
    }
}
