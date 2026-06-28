using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Importer;

namespace USDGameReady
{
    [NodeMetadata("SetMaterialNode", 0)]
    public class SetMaterialNode : Node<SetMaterialNode.InputPort, SetMaterialNode.OutputPort>
    {
        public class InputPort : InputPorts
        {
            public Dictionary<string, GameObject> gameObjects;
            public bool     enabled  = true;
            public Material material = null;
        }

        public class OutputPort : OutputPorts
        {
            public Dictionary<string, GameObject> gameObjects;
        }

        public override void Run()
        {
            Output.gameObjects = Input.gameObjects;

            if (!Input.enabled || Input.gameObjects == null)
                return;

            var mat = Input.material;
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit")
                          ?? Shader.Find("Standard");
                if (shader != null)
                    mat = new Material(shader);
            }

            if (mat == null)
                return;

            foreach (var kvp in Input.gameObjects)
            {
                var renderer = kvp.Value?.GetComponent<MeshRenderer>();
                if (renderer != null)
                    renderer.sharedMaterial = mat;
            }
        }
    }
}
