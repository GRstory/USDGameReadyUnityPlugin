using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Importer;

namespace USDGameReady
{
    [NodeMetadata("CreateNavMeshAgentNode", 0)]
    public class CreateNavMeshAgentNode : Node<CreateNavMeshAgentNode.InputPort, CreateNavMeshAgentNode.OutputPort>
    {
        public class InputPort : InputPorts
        {
            public Dictionary<string, GameObject> gameObjects;
            public HashSet<string> npcPrimPaths;
            public Dictionary<string, Vector3> colliderSizes;
            public bool enabled = true;
            public ComponentTypeRef componentType;
        }

        public class OutputPort : OutputPorts
        {
            public Dictionary<string, GameObject> gameObjects;
            public HashSet<string> navMeshAgentPaths = new();
        }

        public override void Run()
        {
            Output.gameObjects = Input.gameObjects;

            if (!Input.enabled || Input.npcPrimPaths == null || Input.gameObjects == null)
                return;

            var customType = Input.componentType?.Resolve();

            foreach (var path in Input.npcPrimPaths)
            {
                if (!Input.gameObjects.TryGetValue(path, out var go))
                    continue;

                if (customType != null)
                {
                    go.AddComponent(customType);
                }
                else
                {
                    var agent = go.AddComponent<NavMeshAgent>();
                    Output.navMeshAgentPaths.Add(path);

                    if (Input.colliderSizes != null && Input.colliderSizes.TryGetValue(path, out var sz))
                    {
                        agent.radius = sz.x;
                        agent.height = sz.z;
                    }
                }
            }
        }
    }
}
