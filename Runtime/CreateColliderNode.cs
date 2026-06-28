using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Importer;

namespace USDGameReady
{
    [NodeMetadata("CreateColliderNode", 0)]
    public class CreateColliderNode : Node<CreateColliderNode.InputPort, CreateColliderNode.OutputPort>
    {
        public class InputPort : InputPorts
        {
            public Dictionary<string, GameObject> gameObjects;
            public Dictionary<string, string> colliderPrimPaths;
            public Dictionary<string, Vector3> colliderSizes;
            public Dictionary<string, Vector3> colliderCenters;
            public HashSet<string> triggerPaths;
            public HashSet<string> characterControllerPaths;
            public Dictionary<string, string> colliderApproximations;
            public Dictionary<string, int> capsuleAxes;
            public bool enabled = true;
        }

        public class OutputPort : OutputPorts
        {
            public Dictionary<string, GameObject> gameObjects;
        }

        public override void Run()
        {
            Output.gameObjects = Input.gameObjects;

            if (!Input.enabled || Input.colliderPrimPaths == null || Input.gameObjects == null)
                return;

            foreach (var kvp in Input.colliderPrimPaths)
            {
                // Player(CharacterController)인 prim은 Collider 부착 skip — 사이즈는 Player 노드가 이미 받음
                if (Input.characterControllerPaths != null && Input.characterControllerPaths.Contains(kvp.Key))
                    continue;

                if (!Input.gameObjects.TryGetValue(kvp.Key, out var go))
                    continue;

                Vector3? explicitSize = null;
                if (Input.colliderSizes != null && Input.colliderSizes.TryGetValue(kvp.Key, out var s))
                    explicitSize = s;

                Vector3? explicitCenter = null;
                if (Input.colliderCenters != null && Input.colliderCenters.TryGetValue(kvp.Key, out var ctr))
                    explicitCenter = ctr;

                int capsuleAxis = 1; // Unity 기본 Y
                if (Input.capsuleAxes != null && Input.capsuleAxes.TryGetValue(kvp.Key, out var ax))
                    capsuleAxis = ax;

                string approximation = null;
                if (Input.colliderApproximations != null)
                    Input.colliderApproximations.TryGetValue(kvp.Key, out approximation);

                Collider collider = null;
                switch (kvp.Value)
                {
                    case "Box":     collider = AttachBox(go, explicitSize, explicitCenter); break;
                    case "Sphere":  collider = AttachSphere(go, explicitSize, explicitCenter); break;
                    case "Capsule": collider = AttachCapsule(go, explicitSize, capsuleAxis, explicitCenter); break;
                    case "Mesh":    collider = AttachMesh(go, approximation); break;
                    default:
                        Debug.LogWarning($"[USDGameReady] 알 수 없는 Collider 타입 '{kvp.Value}' (prim: {kvp.Key})");
                        break;
                }

                if (collider != null && Input.triggerPaths != null && Input.triggerPaths.Contains(kvp.Key))
                {
                    if (collider is MeshCollider mc)
                        mc.convex = true;
                    collider.isTrigger = true;
                }
            }
        }

        static Bounds GetMeshBounds(GameObject go)
        {
            var mf = go.GetComponentInChildren<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
                return mf.sharedMesh.bounds;
            return new Bounds(Vector3.zero, Vector3.one);
        }

        static BoxCollider AttachBox(GameObject go, Vector3? size, Vector3? center)
        {
            var c = go.AddComponent<BoxCollider>();
            if (size.HasValue)
            {
                c.size = size.Value;
                if (center.HasValue) c.center = center.Value;
            }
            else
            {
                var b = GetMeshBounds(go);
                c.center = center ?? b.center;
                c.size = b.size;
            }
            return c;
        }

        static SphereCollider AttachSphere(GameObject go, Vector3? size, Vector3? center)
        {
            var c = go.AddComponent<SphereCollider>();
            if (size.HasValue)
            {
                c.radius = size.Value.x;
                if (center.HasValue) c.center = center.Value;
            }
            else
            {
                var b = GetMeshBounds(go);
                c.center = center ?? b.center;
                c.radius = Mathf.Max(b.extents.x, b.extents.y, b.extents.z);
            }
            return c;
        }

        static CapsuleCollider AttachCapsule(GameObject go, Vector3? size, int direction, Vector3? center)
        {
            var c = go.AddComponent<CapsuleCollider>();
            c.direction = direction;
            if (size.HasValue)
            {
                c.radius = size.Value.x;
                c.height = size.Value.z;
                if (center.HasValue) c.center = center.Value;
            }
            else
            {
                var b = GetMeshBounds(go);
                c.center = center ?? b.center;
                c.radius = Mathf.Max(b.extents.x, b.extents.z);
                c.height = b.size.y;
            }
            return c;
        }

        // physics:approximation 분기:
        //   "boundingCube"        → BoxCollider auto-fit (mesh bounds)
        //   "boundingSphere"      → SphereCollider auto-fit (mesh bounds)
        //   "convexHull"          → MeshCollider.convex = true
        //   "convexDecomposition" → Unity 기본 미지원 → convexHull로 대체 (경고)
        //   "meshSimplification"  → 단순화 미구현 → 원본 mesh 사용 (경고)
        //   "none" / null         → MeshCollider.convex = false
        static Collider AttachMesh(GameObject go, string approximation)
        {
            switch (approximation)
            {
                case "boundingCube":
                {
                    var c = go.AddComponent<BoxCollider>();
                    var b = GetMeshBounds(go);
                    c.center = b.center;
                    c.size = b.size;
                    return c;
                }
                case "boundingSphere":
                {
                    var c = go.AddComponent<SphereCollider>();
                    var b = GetMeshBounds(go);
                    c.center = b.center;
                    c.radius = Mathf.Max(b.extents.x, b.extents.y, b.extents.z);
                    return c;
                }
                case "convexHull":
                {
                    var c = go.AddComponent<MeshCollider>();
                    c.convex = true;
                    return c;
                }
                case "convexDecomposition":
                {
                    Debug.LogWarning($"[USDGameReady] '{go.name}': convexDecomposition은 Unity 기본 지원이 아닙니다. convexHull로 대체합니다.");
                    var c = go.AddComponent<MeshCollider>();
                    c.convex = true;
                    return c;
                }
                case "meshSimplification":
                {
                    Debug.LogWarning($"[USDGameReady] '{go.name}': meshSimplification은 별도 처리가 필요합니다. 원본 mesh를 사용합니다.");
                    var c = go.AddComponent<MeshCollider>();
                    c.convex = false;
                    return c;
                }
                default:
                {
                    var c = go.AddComponent<MeshCollider>();
                    c.convex = false;
                    return c;
                }
            }
        }
    }
}
