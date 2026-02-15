using UnityEngine;
using System.Collections.Generic;
using Game.Data;
using Game.Core;
using UnityEngine.Rendering;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Gameplay
{
    // GridManager handles: grid spwawn and managing. nodes instantiation, positioning, and queries.
    public class GridManager : MonoBehaviourSingleton<GridManager>   
    {

        #region Serialized Fields

        [Header("Prefabs")]
        [Tooltip("The HexNode prefab to instantiate for each grid cell")]
        [SerializeField] private HexNode hexNodePrefab;

        [Header("Shared Sprites")]
        [Tooltip("Background hex sprite - same for all nodes (tile_bg.png)")]
        [SerializeField] private Sprite sharedBgSprite;
        
        [Header("Grid Settings")]
        [Tooltip("Size of each hex cell in world units")]
        [SerializeField] private float cellSize = 1.0f;

        [Tooltip("Additional spacing between cells")]
        [SerializeField] private float gridSpacing = 0.1f;

        private Dictionary<Vector2Int, HexNode> gridNodes = new Dictionary<Vector2Int, HexNode>();
        private LevelData currentLevelData;
        private Transform gridContainer;

        #endregion

        #region Setup

        private void Awake()
        {
            InitializeGridContainer();
        }

        private void InitializeGridContainer()
        {
            gridContainer = new GameObject("GridContainer").transform;
            gridContainer.SetParent(transform);
            gridContainer.localPosition = Vector3.zero;
        }

        #endregion

        #region Grid Generation

        // Generate grid from level data
        public void GenerateGrid(LevelData levelData)
        {
            if (levelData == null)
            {
                Debug.LogError("[GridManager] levelData is null!");
                return;
            }

            if (hexNodePrefab == null)
            {
                Debug.LogError("[GridManager] hexNodePrefab not assigned!");
                return;
            }

            if (sharedBgSprite == null)
            {
                Debug.LogError("[GridManager] sharedBgSprite not assigned!");
                return;
            }

            currentLevelData = levelData;
            ClearGrid();

            // Spawn nodes
            foreach (var nodeLayout in levelData.NodeLayouts)
            {
                if (nodeLayout == null || nodeLayout.connectionDataReference == null)
                    continue;

                HexNode nodeInstance = Instantiate(hexNodePrefab, gridContainer);
                nodeInstance.name = $"Node_{nodeLayout.x}_{nodeLayout.y}";

                Vector3 worldPosition = CalculateHexPosition(nodeLayout.x, nodeLayout.y);
                nodeInstance.transform.position = worldPosition;

                Vector2Int gridPosition = new Vector2Int(nodeLayout.x, nodeLayout.y);
                nodeInstance.Initialize(gridPosition, nodeLayout.connectionDataReference, nodeLayout.initialRotation, sharedBgSprite);

                gridNodes[gridPosition] = nodeInstance;
            }

            CenterGridOnScreen();
        }

        // Calculate hex world position - flat-top hex, corner-to-corner contact
        private Vector3 CalculateHexPosition(int gridX, int gridY)
        {
            // Flat-top hex with corner-to-corner contact:
            // Horizontal spacing = 1.5 × cellSize (distance between column centers)
            // Vertical spacing = 0.425 × cellSize (half of 0.85)
            // Odd-row offset = 0.75 × cellSize (half of horizontal spacing)
            
            float horizontalSpacing = cellSize * 1.5f + gridSpacing;
            float verticalSpacing = cellSize * 0.425f + gridSpacing;
            
            // Base position
            float x = gridX * horizontalSpacing;
            float y = gridY * verticalSpacing;
            
            // Odd-row offset: shift right by half the horizontal spacing
            if (gridY % 2 == 1)
            {
                x += cellSize * 0.75f;
            }
            
            return new Vector3(x, y, 0);
        }

        // Center camera on grid
        private void CenterGridOnScreen()
        {
            if (gridNodes.Count == 0) return;

            Camera mainCamera = Camera.main;
            if (mainCamera == null) return;

            // Calculate bounds
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (var kvp in gridNodes)
            {
                Vector3 pos = kvp.Value.transform.position;
                minX = Mathf.Min(minX, pos.x);
                maxX = Mathf.Max(maxX, pos.x);
                minY = Mathf.Min(minY, pos.y);
                maxY = Mathf.Max(maxY, pos.y);
            }

            // Position camera
            Vector3 gridCenter = new Vector3(
                (minX + maxX) / 2f,
                (minY + maxY) / 2f,
                mainCamera.transform.position.z
            );

            mainCamera.transform.position = gridCenter;

            // Adjust size to fit grid
            float gridWidth = maxX - minX + cellSize;
            float gridHeight = maxY - minY + cellSize;
            float aspectRatio = (float)Screen.width / Screen.height;
            float padding = 1.5f;

            float requiredHeightSize = gridHeight / 2f * padding;
            float requiredWidthSize = gridWidth / (2f * aspectRatio) * padding;

            mainCamera.orthographicSize = Mathf.Max(requiredHeightSize, requiredWidthSize);
        }

        #endregion

        #region Queries

        public HexNode GetNodeAt(Vector2Int gridPosition)
        {
            if (gridNodes.ContainsKey(gridPosition))
                return gridNodes[gridPosition];
            return null;
        }

        public HexNode GetNodeAt(int x, int y)
        {
            return GetNodeAt(new Vector2Int(x, y));
        }

        public List<HexNode> GetAllNodes()
        {
            return new List<HexNode>(gridNodes.Values);
        }

        public Dictionary<Vector2Int, HexNode> GetGridNodes()
        {
            return gridNodes;
        }

        public List<HexNode> GetSourceNodes()
        {
            List<HexNode> sources = new List<HexNode>();
            foreach (var node in gridNodes.Values)
            {
                if (node != null && node.IsSource)
                    sources.Add(node);
            }
            return sources;
        }

        public List<HexNode> GetGoalNodes()
        {
            List<HexNode> goals = new List<HexNode>();
            foreach (var node in gridNodes.Values)
            {
                if (node != null && node.IsGoal)
                    goals.Add(node);
            }
            return goals;
        }

        public Vector3 GetGridCenter()
        {
            if (gridNodes == null || gridNodes.Count == 0)
                return Vector3.zero;
            
            Vector3 sum = Vector3.zero;
            foreach (var node in gridNodes.Values)
            {
                if (node != null)
                    sum += node.transform.position;
            }
            
            return sum / gridNodes.Count;
        }

        #endregion

        #region Cleanup

        public void ClearGrid()
        {
            foreach (var kvp in gridNodes)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value.gameObject);
            }

            gridNodes.Clear();
            currentLevelData = null;
        }


        public void ToggleGrid(bool isShow) 
        {
            var order = isShow ? 1 : -10;
            GetComponent<SortingGroup>().sortingOrder = order;
        }
        #endregion

        #region Debug

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (gridNodes == null || gridNodes.Count == 0) return;

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (var kvp in gridNodes)
            {
                Vector3 pos = kvp.Value.transform.position;
                minX = Mathf.Min(minX, pos.x);
                maxX = Mathf.Max(maxX, pos.x);
                minY = Mathf.Min(minY, pos.y);
                maxY = Mathf.Max(maxY, pos.y);
            }

            // Draw bounds
            Gizmos.color = Color.cyan;
            Vector3 boundsCenter = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0);
            Vector3 boundsSize = new Vector3(maxX - minX + cellSize, maxY - minY + cellSize, 0.1f);
            Gizmos.DrawWireCube(boundsCenter, boundsSize);

            if (currentLevelData != null)
            {
                string info = $"Grid: {currentLevelData.GridWidth}x{currentLevelData.GridHeight}\n" +
                              $"Nodes: {gridNodes.Count}\n" +
                              $"Level: {currentLevelData.LevelName}";
                Handles.Label(boundsCenter + Vector3.up * (boundsSize.y / 2f + 0.5f), info);
            }

            // Draw cell outlines
            Gizmos.color = Color.gray;
            foreach (var kvp in gridNodes)
            {
                if (kvp.Value != null)
                    Gizmos.DrawWireSphere(kvp.Value.transform.position, cellSize / 2f);
            }
        }
#endif

        #endregion
    }
}
