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
            public HashSet<string> triggerPaths;
            public HashSet<string> characterControllerPaths;
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
                if (Input.characterControllerPaths != null && Input.characterControllerPaths.Contains(kvp.Key))
                    continue;

                if (!Input.gameObjects.TryGetValue(kvp.Key, out var go))
                    continue;

                Vector3? explicitSize = null;
                if (Input.colliderSizes != null && Input.colliderSizes.TryGetValue(kvp.Key, out var s))
                    explicitSize = s;

                Collider collider = null;
                switch (kvp.Value)
                {
                    case "Box":     collider = AttachBox(go, explicitSize);     break;
                    case "Sphere":  collider = AttachSphere(go, explicitSize);  break;
                    case "Capsule": collider = AttachCapsule(go, explicitSize); break;
                    case "Mesh":    collider = go.AddComponent<MeshCollider>(); break;
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

        static BoxCollider AttachBox(GameObject go, Vector3? size)
        {
            var c = go.AddComponent<BoxCollider>();
            if (size.HasValue)
            {
                c.size = size.Value;
            }
            else
            {
                var b = GetMeshBounds(go);
                c.center = b.center;
                c.size = b.size;
            }
            return c;
        }

        static SphereCollider AttachSphere(GameObject go, Vector3? size)
        {
            var c = go.AddComponent<SphereCollider>();
            if (size.HasValue)
            {
                c.radius = size.Value.x;
            }
            else
            {
                var b = GetMeshBounds(go);
                c.center = b.center;
                c.radius = Mathf.Max(b.extents.x, b.extents.y, b.extents.z);
            }
            return c;
        }

        static CapsuleCollider AttachCapsule(GameObject go, Vector3? size)
        {
            var c = go.AddComponent<CapsuleCollider>();
            if (size.HasValue)
            {
                c.radius = size.Value.x;
                c.height = size.Value.z;
            }
            else
            {
                var b = GetMeshBounds(go);
                c.center = b.center;
                c.radius = Mathf.Max(b.extents.x, b.extents.z);
                c.height = b.size.y;
            }
            return c;
        }
    }
}
