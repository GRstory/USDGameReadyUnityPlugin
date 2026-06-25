using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Importer;

namespace USDGameReady
{
    [NodeMetadata("CreateRigidbodyNode", 0)]
    public class CreateRigidbodyNode : Node<CreateRigidbodyNode.InputPort, CreateRigidbodyNode.OutputPort>
    {
        public class InputPort : InputPorts
        {
            public Dictionary<string, GameObject> gameObjects;
            public HashSet<string> rigidbodyPaths;
            public Dictionary<string, float> masses;
            public HashSet<string> kinematicPaths;
            public Dictionary<string, Vector3> linearVelocities;
            public Dictionary<string, Vector3> angularVelocities;
            public Dictionary<string, Vector3> centerOfMass;
            public HashSet<string> useGravityFalsePaths;
            public HashSet<string> characterControllerPaths; // Player skip
            public bool enabled = true;
        }

        public class OutputPort : OutputPorts
        {
            public Dictionary<string, GameObject> gameObjects;
        }

        public override void Run()
        {
            Output.gameObjects = Input.gameObjects;

            if (!Input.enabled || Input.rigidbodyPaths == null || Input.gameObjects == null)
                return;

            foreach (var path in Input.rigidbodyPaths)
            {
                // CharacterController와 Rigidbody는 함께 쓰면 충돌 — Player prim은 skip
                if (Input.characterControllerPaths != null && Input.characterControllerPaths.Contains(path))
                    continue;

                if (!Input.gameObjects.TryGetValue(path, out var go))
                    continue;

                var rb = go.AddComponent<Rigidbody>();

                if (Input.masses != null && Input.masses.TryGetValue(path, out var m))
                    rb.mass = m;

                if (Input.kinematicPaths != null && Input.kinematicPaths.Contains(path))
                    rb.isKinematic = true;

                if (Input.linearVelocities != null && Input.linearVelocities.TryGetValue(path, out var v))
                    rb.linearVelocity = v;

                if (Input.angularVelocities != null && Input.angularVelocities.TryGetValue(path, out var av))
                    rb.angularVelocity = av;

                if (Input.centerOfMass != null && Input.centerOfMass.TryGetValue(path, out var com))
                    rb.centerOfMass = com;

                if (Input.useGravityFalsePaths != null && Input.useGravityFalsePaths.Contains(path))
                    rb.useGravity = false;
            }
        }
    }
}
