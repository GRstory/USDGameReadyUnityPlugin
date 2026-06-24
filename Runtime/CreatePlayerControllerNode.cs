using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Importer;

namespace USDGameReady
{
    [NodeMetadata("CreatePlayerControllerNode", 0)]
    public class CreatePlayerControllerNode : Node<CreatePlayerControllerNode.InputPort, CreatePlayerControllerNode.OutputPort>
    {
        public class InputPort : InputPorts
        {
            public Dictionary<string, GameObject> gameObjects;
            public HashSet<string> playerPrimPaths;
            public Dictionary<string, Vector3> colliderSizes;
            public Dictionary<string, float> slopeAngleLimits;
            public Dictionary<string, float> stepHeights;
            public bool enabled = true;
            public ComponentTypeRef componentType;
        }

        public class OutputPort : OutputPorts
        {
            public Dictionary<string, GameObject> gameObjects;
            public HashSet<string> characterControllerPaths = new();
        }

        public override void Run()
        {
            Output.gameObjects = Input.gameObjects;

            if (!Input.enabled || Input.playerPrimPaths == null || Input.gameObjects == null)
                return;

            var customType = Input.componentType?.Resolve();

            foreach (var path in Input.playerPrimPaths)
            {
                if (!Input.gameObjects.TryGetValue(path, out var go))
                    continue;

                if (customType != null)
                {
                    go.AddComponent(customType);
                }
                else
                {
                    var cc = go.AddComponent<CharacterController>();
                    Output.characterControllerPaths.Add(path);

                    if (Input.colliderSizes != null && Input.colliderSizes.TryGetValue(path, out var sz))
                    {
                        cc.radius = sz.x;
                        cc.height = sz.z;
                    }
                    if (Input.slopeAngleLimits != null && Input.slopeAngleLimits.TryGetValue(path, out var slope))
                        cc.slopeLimit = slope;
                    if (Input.stepHeights != null && Input.stepHeights.TryGetValue(path, out var step))
                        cc.stepOffset = step;
                }
            }
        }
    }
}
