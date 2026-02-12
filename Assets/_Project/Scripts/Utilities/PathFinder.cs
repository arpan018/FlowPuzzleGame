using UnityEngine;
using System.Collections.Generic;
using Game.Core;
using Game.Gameplay;

namespace Game.Utilities
{
    /// <summary>
    /// Pathfinding and connection validation for hex grids using BFS.
    /// Performance: O(V + E) V = number of nodes, E = number of connections.
    /// </summary>
    public static class PathFinder
    {
        #region Pathfinding

        // Find all nodes connected to a source using BFS with bidirectional connection validation
        // Returns HashSet of all nodes reachable from the source node through valid connections.
        public static HashSet<HexNode> FindConnectedNodes(HexNode sourceNode, Dictionary<Vector2Int, HexNode> gridNodes)
        {
            if (sourceNode == null)
            {
                Debug.LogWarning("[PathFinder] sourceNode is null.");
                return new HashSet<HexNode>();
            }

            if (gridNodes == null || gridNodes.Count == 0)
            {
                Debug.LogWarning("[PathFinder] gridNodes is null or empty.");
                return new HashSet<HexNode>();
            }
            
            // Initialize BFS data structures
            Queue<HexNode> queue = new Queue<HexNode>();
            HashSet<HexNode> visited = new HashSet<HexNode>();

            // Start BFS from source node
            queue.Enqueue(sourceNode);
            visited.Add(sourceNode);

            while (queue.Count > 0)
            {
                HexNode currentNode = queue.Dequeue();
                bool[] currentConnections = currentNode.GetConnectionsAtCurrentRotation();

                // Check all 6 hex directions
                for (int i = 0; i < 6; i++)
                {
                    HexDirection direction = (HexDirection)i;

                    // Skip if current node has no connection in this direction
                    if (!currentConnections[i])
                        continue;

                    // Calculate neighbor's grid position
                    Vector2Int neighborPos = ConnectionChecker.GetNeighborPosition(currentNode.GridPosition, direction);

                    if (!gridNodes.ContainsKey(neighborPos))
                        continue;

                    HexNode neighborNode = gridNodes[neighborPos];

                    if (visited.Contains(neighborNode))
                        continue;

                    // Validate bidirectional connection
                    HexDirection oppositeDirection = ConnectionChecker.GetOppositeDirection(direction);
                    bool neighborHasOpposite = neighborNode.HasConnectionInDirection(oppositeDirection);
                    
                    
                    if (neighborHasOpposite)
                    {
                        visited.Add(neighborNode);
                        queue.Enqueue(neighborNode);
                    }
                }
            }

            Debug.Log($"[PathFinder] BFS complete. Total powered nodes: {visited.Count}");
            return visited;
        }

        #endregion

        #region Win Condition

        // Check if all goal nodes are in the powered nodes set
        // Returns True if ALL goal nodes are powered, false otherwise.
        public static bool CheckWinCondition(List<HexNode> goalNodes, HashSet<HexNode> poweredNodes)
        {
            if (goalNodes == null || goalNodes.Count == 0)
            {
                Debug.LogWarning("[PathFinder] goalNodes is null or empty.");
                return false;
            }

            if (poweredNodes == null || poweredNodes.Count == 0)
                return false;

            // Check if ALL goal nodes are in the powered set
            foreach (HexNode goal in goalNodes)
            {
                if (goal == null) continue;

                if (!poweredNodes.Contains(goal))
                    return false;
            }

            return true;
        }

        #endregion

        #region Power Management

        // Update all node power states based on powered set (sources always stay powered)
        public static void UpdatePoweredStates(HashSet<HexNode> poweredNodes, Dictionary<Vector2Int, HexNode> allNodes)
        {
            if (allNodes == null || allNodes.Count == 0)
            {
                Debug.LogWarning("[PathFinder] allNodes is null or empty.");
                return;
            }

            if (poweredNodes == null)
                poweredNodes = new HashSet<HexNode>();

            // Update all nodes
            foreach (var kvp in allNodes)
            {
                HexNode node = kvp.Value;
                
                if (node == null) continue;

                if (node.IsSource)
                {
                    node.SetPowered(true);
                    continue;
                }

                // Set power state based on whether node is in powered set
                bool shouldBePowered = poweredNodes.Contains(node);
                node.SetPowered(shouldBePowered);
            }
        }

        #endregion

        #region Debug Helpers

        // Find path between two nodes using BFS with parent tracking for visualization
        // Returns List of grid positions forming the path, or empty list if no path exists.
        public static List<Vector2Int> GetPathPositions(HexNode from, HexNode to, Dictionary<Vector2Int, HexNode> gridNodes)
        {
            List<Vector2Int> path = new List<Vector2Int>();

            if (from == null || to == null || gridNodes == null)
            {
                Debug.LogWarning("[PathFinder] GetPathPositions called with null parameters.");
                return path;
            }

            if (from == to)
            {
                path.Add(from.GridPosition);
                return path;
            }

            // BFS with parent tracking
            Queue<HexNode> queue = new Queue<HexNode>();
            HashSet<HexNode> visited = new HashSet<HexNode>();
            Dictionary<HexNode, HexNode> parents = new Dictionary<HexNode, HexNode>();

            queue.Enqueue(from);
            visited.Add(from);
            parents[from] = null;

            bool pathFound = false;

            while (queue.Count > 0)
            {
                HexNode currentNode = queue.Dequeue();

                if (currentNode == to)
                {
                    pathFound = true;
                    break;
                }

                // Get current node's connections
                bool[] currentConnections = currentNode.GetConnectionsAtCurrentRotation();

                // Explore neighbors
                for (int i = 0; i < 6; i++)
                {
                    HexDirection direction = (HexDirection)i;

                    if (!currentConnections[i])
                        continue;

                    Vector2Int neighborPos = ConnectionChecker.GetNeighborPosition(currentNode.GridPosition, direction);

                    if (!gridNodes.ContainsKey(neighborPos))
                        continue;

                    HexNode neighborNode = gridNodes[neighborPos];

                    if (visited.Contains(neighborNode))
                        continue;

                    // Check bidirectional connection
                    HexDirection oppositeDirection = ConnectionChecker.GetOppositeDirection(direction);
                    
                    if (neighborNode.HasConnectionInDirection(oppositeDirection))
                    {
                        visited.Add(neighborNode);
                        parents[neighborNode] = currentNode;
                        queue.Enqueue(neighborNode);
                    }
                }
            }

            // Reconstruct path if found
            if (pathFound)
            {
                HexNode current = to;
                while (current != null)
                {
                    path.Insert(0, current.GridPosition);
                    parents.TryGetValue(current, out current);
                }
            }

            return path;
        }

        // Count valid bidirectional connections for a node (useful for debugging)
        public static int CountValidConnections(HexNode node, Dictionary<Vector2Int, HexNode> gridNodes)
        {
            if (node == null || gridNodes == null)
                return 0;

            int count = 0;
            bool[] connections = node.GetConnectionsAtCurrentRotation();

            for (int i = 0; i < 6; i++)
            {
                if (!connections[i])
                    continue;

                HexDirection direction = (HexDirection)i;
                Vector2Int neighborPos = ConnectionChecker.GetNeighborPosition(node.GridPosition, direction);

                if (!gridNodes.ContainsKey(neighborPos))
                    continue;

                HexNode neighbor = gridNodes[neighborPos];
                HexDirection oppositeDir = ConnectionChecker.GetOppositeDirection(direction);

                if (neighbor.HasConnectionInDirection(oppositeDir))
                    count++;
            }

            return count;
        }

        #endregion
    }
}
