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
            public bool enabled = true;
            public ComponentTypeRef componentType;
        }

        public class OutputPort : OutputPorts
        {
            public Dictionary<string, GameObject> gameObjects;
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
                    go.AddComponent(customType);
                else
                    go.AddComponent<CharacterController>();
            }
        }
    }
}
