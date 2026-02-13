using UnityEngine;
using System.Collections.Generic;
using Game.Core;
using Game.Utilities;
using NaughtyAttributes;

namespace Game.Data
{
    /// <summary>
    /// ScriptableObject defining complete level grid and node layout.
    /// Used to create level configuration assets for the hex grid puzzle game.
    /// </summary>
    [CreateAssetMenu(fileName = "Level_", menuName = "Game/Level Data", order = 0)]
    public class LevelData : ScriptableObject
    {
        #region Level Info

        [Header("Level Info")]
        [Tooltip("Unique level number identifier")]
        [SerializeField] private int levelNumber = 1;

        [Tooltip("Display name for this level")]
        [SerializeField] private string levelName = "New Level";

        //[Tooltip("Difficulty rating (1=Easy, 5=Very Hard)")]
        //[SerializeField] [Range(1, 5)] private int difficulty = 1;

        public int LevelNumber => levelNumber;
        public string LevelName => levelName;
        //public int Difficulty => difficulty;

        #endregion

        #region Grid Configuration

        [Header("Grid Configuration")]
        [Tooltip("Type of grid system (Square or Hex)")]
        [SerializeField] private GridType gridType = GridType.Hex;

        [Tooltip("Width of the grid (number of columns)")]
        [SerializeField] [Range(2, 6)] private int gridWidth = 5;

        [Tooltip("Height of the grid (number of rows)")]
        [SerializeField] [Range(2, 20)] private int gridHeight = 5;

        public GridType GridType => gridType;
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;

        #endregion

        #region Node Layout

        [Header("Node Layout")]
        [Tooltip("List of all node definitions for this level")]
        [SerializeField] private List<NodeLayoutData> nodeLayouts = new List<NodeLayoutData>();

        public List<NodeLayoutData> NodeLayouts => nodeLayouts;

        #endregion

        #region Nested Class

        // Layout and configuration data for a single node in the grid
        [System.Serializable]
        public class NodeLayoutData
        {
            [Tooltip("Grid X position (column)")]
            public int x;

            [Tooltip("Grid Y position (row)")]
            public int y;

            [Tooltip("Reference to the NodeConnectionData ScriptableObject")]
            public NodeConnectionData connectionDataReference;

            [Tooltip("Initial rotation state (0-5 for 60° increments)")]
            [Range(0, 5)] public int initialRotation = 0;

            // Check if node can be rotated (uses override if set, otherwise checks connectionData)
            public bool CanRotate()
            {
                if (connectionDataReference != null)
                    return connectionDataReference.CanRotate;

                return false;
            }
        }

        #endregion

        #region Validation

        private void OnValidate()
        {
            // Ensure level number is positive
            if (levelNumber < 1)
                levelNumber = 1;

            // Clamp grid dimensions if needed
            //gridWidth = Mathf.Clamp(gridWidth, 2, 8);
            //gridHeight = Mathf.Clamp(gridHeight, 2, 8);

            // Validate node positions within grid bounds
            if (nodeLayouts != null)
            {
                foreach (var nodeLayout in nodeLayouts)
                {
                    if (nodeLayout != null)
                    {
                        // Clamp x position
                        if (nodeLayout.x < 0 || nodeLayout.x >= gridWidth)
                        {
                            Debug.LogWarning($"[{levelName}] Node at ({nodeLayout.x}, {nodeLayout.y}) has invalid X. Must be 0-{gridWidth - 1}.");
                            nodeLayout.x = Mathf.Clamp(nodeLayout.x, 0, gridWidth - 1);
                        }

                        // Clamp y position
                        if (nodeLayout.y < 0 || nodeLayout.y >= gridHeight)
                        {
                            Debug.LogWarning($"[{levelName}] Node at ({nodeLayout.x}, {nodeLayout.y}) has invalid Y. Must be 0-{gridHeight - 1}.");
                            nodeLayout.y = Mathf.Clamp(nodeLayout.y, 0, gridHeight - 1);
                        }

                        // Clamp initial rotation
                        nodeLayout.initialRotation = Mathf.Clamp(nodeLayout.initialRotation, 0, 5);
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        // Get node layout data at specific grid position
        public NodeLayoutData GetNodeAt(int x, int y)
        {
            if (nodeLayouts == null)
                return null;

            foreach (var nodeLayout in nodeLayouts)
            {
                if (nodeLayout != null && nodeLayout.x == x && nodeLayout.y == y)
                    return nodeLayout;
            }

            return null;
        }

        // Get all nodes of specific type
        public List<NodeLayoutData> GetNodesByType(NodeType type)
        {
            List<NodeLayoutData> matchingNodes = new List<NodeLayoutData>();

            if (nodeLayouts == null)
                return matchingNodes;

            foreach (var nodeLayout in nodeLayouts)
            {
                if (nodeLayout != null && 
                    nodeLayout.connectionDataReference != null && 
                    nodeLayout.connectionDataReference.NodeType == type)
                {
                    matchingNodes.Add(nodeLayout);
                }
            }

            return matchingNodes;
        }

        // Count source nodes in this level
        public int GetSourceCount()
        {
            return GetNodesByType(NodeType.Source).Count;
        }

        // Count goal nodes in this level
        public int GetGoalCount()
        {
            return GetNodesByType(NodeType.Goal).Count;
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        // Generate empty grid by creating Empty nodes for the entire grid size
        [ContextMenu("Generate Empty Grid")]
        private void GenerateEmptyGrid()
        {
            if (nodeLayouts == null)
                nodeLayouts = new List<NodeLayoutData>();
            else
                nodeLayouts.Clear();

            // Create empty node for each grid position
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    NodeLayoutData newNode = new NodeLayoutData
                    {
                        x = x,
                        y = y,
                        connectionDataReference = null,
                        initialRotation = 0,
                    };
                    nodeLayouts.Add(newNode);
                }
            }

            Debug.Log($"[{levelName}] Generated empty grid of {gridWidth}x{gridHeight} = {nodeLayouts.Count} nodes.");
            UnityEditor.EditorUtility.SetDirty(this);
        }

        // Validate level configuration and log warnings for potential issues
        [ContextMenu("Validate Level")]
        private void ValidateLevel()
        {
            Debug.Log($"=== Validating Level: {levelName} (#{levelNumber}) ===");

            // Check for sources
            int sourceCount = GetSourceCount();
            if (sourceCount == 0)
                Debug.LogWarning($"[{levelName}] No source nodes found! At least one source is required.");
            else
                Debug.Log($"[{levelName}] Sources: {sourceCount}");

            // Check for goals
            int goalCount = GetGoalCount();
            if (goalCount == 0)
                Debug.LogWarning($"[{levelName}] No goal nodes found! At least one goal is required.");
            else
                Debug.Log($"[{levelName}] Goals: {goalCount}");

            // Log general info
            Debug.Log($"[{levelName}] Grid: {gridWidth}x{gridHeight} ({gridType})");
            Debug.Log($"[{levelName}] Total Nodes: {(nodeLayouts != null ? nodeLayouts.Count : 0)}");
            //Debug.Log($"[{levelName}] Difficulty: {difficulty}/5");

            // Check for nodes without connection data
            int nodesWithoutData = 0;
            if (nodeLayouts != null)
            {
                foreach (var node in nodeLayouts)
                {
                    if (node != null && node.connectionDataReference == null)
                        nodesWithoutData++;
                }
            }

            if (nodesWithoutData > 0)
                Debug.LogWarning($"[{levelName}] {nodesWithoutData} nodes have no connection data assigned.");

            Debug.Log($"=== Validation Complete ===");
        }
#endif

        #endregion

        #region Editor Tools

        [Button("Auto-Solve Rotations")]
        private void SolveLevel()
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Auto-Solve Level");
#endif
            Debug.Log($"[{levelName}] Starting Auto-Solve...");

            // Brute force: Iterate multiple times to propagate connections from Source -> End
            // We need enough passes for the "wave" to travel across the grid
            for (int pass = 0; pass < 10; pass++) 
            {
                int changes = 0;
                foreach (var node in nodeLayouts)
                {
                    if (node == null || node.connectionDataReference == null) continue;
                    if (!node.CanRotate()) continue; // Skip Fixed Nodes (Sources)

                    // Try all 6 rotations and score them
                    int bestRotation = node.initialRotation;
                    int bestScore = -999;

                    for (int r = 0; r < 6; r++)
                    {
                        node.initialRotation = r; // Temporarily apply
                        int score = EvaluateFit(node);

                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestRotation = r;
                        }
                    }

                    // Apply the winner
                    if (node.initialRotation != bestRotation)
                    {
                        node.initialRotation = bestRotation;
                        changes++;
                    }
                }
                
                // Optimization: If nothing changed this pass, we are done
                if (changes == 0) break;
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            Debug.Log($"[{levelName}] Auto-Solve Complete!");
        }

        // Returns a score for the current rotation:
        // +10 for connecting to a valid neighbor
        // -100 for pointing into a Void/Wall
        // -10 for pointing into a Fixed neighbor that blocks us
        private int EvaluateFit(NodeLayoutData node)
        {
            int score = 0;
            bool connectedToSomething = false;

            for (int i = 0; i < 6; i++)
            {
                HexDirection dir = (HexDirection)i;
                
                // 1. Do I output here?
                bool myOutput = HasOutputInDirection(node, i);

                // 2. Who is there?
                Vector2Int neighborPos = ConnectionChecker.GetNeighborPosition(new Vector2Int(node.x, node.y), dir);
                var neighborNode = GetNodeAt(neighborPos.x, neighborPos.y);

                // --- CASE A: Pointing into Void (Nothing there) ---
                if (neighborNode == null || neighborNode.connectionDataReference == null)
                {
                    if (myOutput) score -= 100; // Penalize leaking into empty space
                    continue; 
                }

                // 3. Does neighbor point back?
                HexDirection oppositeDir = ConnectionChecker.GetOppositeDirection(dir);
                bool neighborInput = HasOutputInDirection(neighborNode, (int)oppositeDir);

                // --- CASE B: Valid Connection ---
                if (myOutput && neighborInput)
                {
                    score += 10;
                    connectedToSomething = true;
                }
                // --- CASE C: Mismatch (One points, one doesn't) ---
                else if (myOutput != neighborInput)
                {
                    // This is the CRITICAL FIX:
                    // If the neighbor CAN rotate, we don't penalize heavily yet. 
                    // We assume they might fix themselves in the next pass.
                    
                    if (!neighborNode.CanRotate())
                    {
                        // Neighbor is FIXED (e.g. Wall/Source) and blocking us.
                        // This is a hard error.
                        score -= 50; 
                    }
                    else
                    {
                        // Neighbor is ROTATABLE but wrong. 
                        // Small penalty to prefer open ports, but don't rule it out.
                        // (Actually, usually better to ignore penalty so we don't shy away from them)
                    }
                }
            }

            // Bonus: If we haven't connected to ANYTHING, that's usually bad for a pipe game
            if (!connectedToSomething) score -= 5;

            return score;
        }

        private bool HasOutputInDirection(NodeLayoutData node, int dirIndex)
        {
            if (node.connectionDataReference == null) return false;

            bool[] baseConns = node.connectionDataReference.GetConnections();
            
            // Formula: (WorldDirection - Rotation + 6) % 6
            int baseIndex = (dirIndex - node.initialRotation + 6) % 6;
            
            return baseConns[baseIndex];
        }


        [Button("Scramble Rotations")]
        private void ScrambleRotations()
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Scramble Level");
#endif
            int count = 0;
            foreach (var node in nodeLayouts)
            {
                // Only rotate nodes that are allowed to move
                if (node != null && node.CanRotate())
                {
                    // Pick a random rotation (0 to 5)
                    node.initialRotation = Random.Range(0, 6);
                    count++;
                }
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            Debug.Log($"[{levelName}] Scrambled {count} nodes!");
        }
        #endregion
    }
}
