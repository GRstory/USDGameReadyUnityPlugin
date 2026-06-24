using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Importer;

namespace USDGameReady
{
    [NodeMetadata("CreateAudioSourceNode", 0)]
    public class CreateAudioSourceNode : Node<CreateAudioSourceNode.InputPort, CreateAudioSourceNode.OutputPort>
    {
        public class InputPort : InputPorts
        {
            public Dictionary<string, GameObject> gameObjects;
            public HashSet<string> audioSourcePaths;
            public bool enabled = true;
        }

        public class OutputPort : OutputPorts
        {
            public Dictionary<string, GameObject> gameObjects;
        }

        public override void Run()
        {
            Output.gameObjects = Input.gameObjects;

            if (!Input.enabled || Input.audioSourcePaths == null || Input.gameObjects == null)
                return;

            foreach (var path in Input.audioSourcePaths)
            {
                if (Input.gameObjects.TryGetValue(path, out var go))
                    go.AddComponent<AudioSource>();
            }
        }
    }
}
