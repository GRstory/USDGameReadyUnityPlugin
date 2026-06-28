using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Importer;

namespace USDGameReady
{
    [NodeMetadata("CreateInteractableNode", 0)]
    public class CreateInteractableNode : Node<CreateInteractableNode.InputPort, CreateInteractableNode.OutputPort>
    {
        public class InputPort : InputPorts
        {
            public Dictionary<string, GameObject> gameObjects;
            public Dictionary<string, List<string>> interactTargets;
            public bool enabled = true;
        }

        public class OutputPort : OutputPorts
        {
            public Dictionary<string, GameObject> gameObjects;
        }

        public override void Run()
        {
            Output.gameObjects = Input.gameObjects;

            if (!Input.enabled || Input.interactTargets == null || Input.gameObjects == null)
                return;

            foreach (var kvp in Input.interactTargets)
            {
                if (!Input.gameObjects.TryGetValue(kvp.Key, out var go))
                    continue;

                var interactable = go.AddComponent<GameReadyInteractable>();

                foreach (var targetPath in kvp.Value)
                {
                    if (Input.gameObjects.TryGetValue(targetPath, out var targetGo))
                        interactable.targets.Add(targetGo);
                }
            }
        }
    }
}
