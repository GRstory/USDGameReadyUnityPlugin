using System.Collections.Generic;
using System.Linq;
using USDGameReady;
using Unity.Importer.USD;
using UnityEditor;
using UnityEngine;
using UnityEngine.Importer;

namespace USDGameReady.Editor
{
    public static class USDGameReadyGraphBuilder
    {
        private const string k_SourceGraphPath =
            "Packages/com.unity.importer.usd/Unity.Importer.USD.Editor/ImportGraph/usdImporter.asset";

        private const string k_OutputDir =
            "Assets/USDGameReady";

        private const string k_OutputGraphPath =
            "Assets/USDGameReady/usdGameReadyImporter.asset";

        [MenuItem("USDGameReady/Build USD Importer Graph")]
        public static void BuildGraph()
        {
            var sourceGraph = AssetDatabase.LoadAssetAtPath<ImporterGraph>(k_SourceGraphPath);
            if (sourceGraph == null)
            {
                Debug.LogError($"[USDGameReady] 기본 그래프를 찾을 수 없습니다: {k_SourceGraphPath}");
                return;
            }

            var graph = Object.Instantiate(sourceGraph);

            var filterNPCNode      = new FilterNPCPrimsNode();
            var createAgentNode    = new CreateNavMeshAgentNode();
            var filterColliderNode = new FilterColliderPrimsNode();
            var createColliderNode = new CreateColliderNode();

            var buildHierarchyNode = graph.Nodes.OfType<BuildHierarchyNode>().FirstOrDefault();
            if (buildHierarchyNode == null)
            {
                Debug.LogError("[USDGameReady] BuildHierarchyNode를 그래프에서 찾을 수 없습니다.");
                return;
            }

            var hierarchyEdge = graph.Edges.FirstOrDefault(e =>
                e.Destination.Node == (INode<InputPorts, OutputPorts>)buildHierarchyNode &&
                e.Origin.Node is IDictionaryAggregatorNode<Dictionary<string, GameObject>>);

            if (hierarchyEdge.Origin.Node == null)
            {
                Debug.LogError("[USDGameReady] aggregator → BuildHierarchyNode 엣지를 찾을 수 없습니다.");
                return;
            }

            var finalAggregator = hierarchyEdge.Origin.Node;

            var stageOpenNode = graph.Nodes.OfType<UsdStageOpenNode>().FirstOrDefault();
            if (stageOpenNode == null)
            {
                Debug.LogError("[USDGameReady] UsdStageOpenNode를 그래프에서 찾을 수 없습니다.");
                return;
            }

            graph.RemoveEdge(hierarchyEdge);

            graph.AddNode(filterNPCNode);
            graph.AddNode(createAgentNode);
            graph.AddNode(filterColliderNode);
            graph.AddNode(createColliderNode);

            graph.AddEdge(new Edge(
                stageOpenNode,  nameof(UsdStageOpenNode.Output.stage),
                filterNPCNode,  nameof(FilterNPCPrimsNode.Input.stage)));

            graph.AddEdge(new Edge(
                finalAggregator,  nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Output.output),
                createAgentNode,  nameof(CreateNavMeshAgentNode.Input.gameObjects)));

            graph.AddEdge(new Edge(
                filterNPCNode,  nameof(FilterNPCPrimsNode.Output.npcPrimPaths),
                createAgentNode, nameof(CreateNavMeshAgentNode.Input.npcPrimPaths)));

            graph.AddEdge(new Edge(
                createAgentNode,    nameof(CreateNavMeshAgentNode.Output.gameObjects),
                createColliderNode, nameof(CreateColliderNode.Input.gameObjects)));

            graph.AddEdge(new Edge(
                stageOpenNode,      nameof(UsdStageOpenNode.Output.stage),
                filterColliderNode, nameof(FilterColliderPrimsNode.Input.stage)));

            graph.AddEdge(new Edge(
                filterColliderNode, nameof(FilterColliderPrimsNode.Output.colliderPrimPaths),
                createColliderNode, nameof(CreateColliderNode.Input.colliderPrimPaths)));

            graph.AddEdge(new Edge(
                createColliderNode, nameof(CreateColliderNode.Output.gameObjects),
                buildHierarchyNode, nameof(BuildHierarchyNode.Input.gameObjects)));

            System.IO.Directory.CreateDirectory(k_OutputDir);
            AssetDatabase.CreateAsset(graph, k_OutputGraphPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[USDGameReady] 커스텀 임포터 그래프 저장 완료: {k_OutputGraphPath}");
        }
    }
}
