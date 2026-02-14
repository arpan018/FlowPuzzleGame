using UnityEngine;
using System.Collections.Generic;
using Game.Core;
using Game.Data;
using Game.Utilities;

namespace Game.Gameplay
{
    // Manager hadles level, gameplay flow and win check. Cordinate between various scripts
    public class LevelManager : MonoBehaviourSingleton<LevelManager>
    {
        #region Fields

        [Header("Level Configuration")]
        [Tooltip("Array of all level ScriptableObjects in order")]
        [SerializeField] private LevelData[] allLevels;

        [Header("Gameplay Settings")]
        [Tooltip("Automatically check win condition after node rotations")]
        [SerializeField] private bool autoCheckWinCondition = true;

        [Tooltip("Delay before checking win condition (allows animations to complete)")]
        [SerializeField] private float winCheckDelay = 0.3f;

        private int currentLevelIndex = 0;
        private LevelData currentLevel;
        private List<HexNode> sourceNodes = new List<HexNode>();
        private List<HexNode> goalNodes = new List<HexNode>();
        private float levelStartTime;
        private int totalRotationsThisLevel;
        private bool isLevelActive;

        #endregion

        #region Setup

        private void OnEnable()
        {
            GameEvents.OnNodeRotated += OnNodeRotated;
        }

        private void OnDisable()
        {
            GameEvents.OnNodeRotated -= OnNodeRotated;
        }

        private void Start()
        {
            // Auto-load first level for testing
            if (allLevels != null && allLevels.Length > 0)
                LoadLevel(0);
        }

        #endregion

        #region Level Loading

        public void LoadLevel(int levelIndex)
        {
            if (allLevels == null || allLevels.Length == 0)
            {
                Debug.LogError("[LevelManager] allLevels is empty!");
                return;
            }

            if (levelIndex < 0 || levelIndex >= allLevels.Length)
            {
                Debug.LogError($"[LevelManager] Invalid level index: {levelIndex}");
                return;
            }

            currentLevelIndex = levelIndex;
            currentLevel = allLevels[levelIndex];

            if (currentLevel == null)
            {
                Debug.LogError($"[LevelManager] Level at index {levelIndex} is null!");
                return;
            }

            StartLevel();
        }

        public void LoadNextLevel()
        {
            int nextIndex = currentLevelIndex + 1;

            if (nextIndex >= allLevels.Length)
            {
                Debug.Log("[LevelManager] All levels completed!");
                return;
            }

            LoadLevel(nextIndex);
        }

        public void RestartCurrentLevel()
        {
            if (currentLevel == null) return;
            LoadLevel(currentLevelIndex);
        }

        #endregion

        #region Level Flow

        // Start level: generate grid and find source/goal nodes
        private void StartLevel()
        {
            if (currentLevel == null) return;

            GridManager.Instance.GenerateGrid(currentLevel);
            // random at each start, enable if needed
            //ScrambleNodes();

            sourceNodes = GridManager.Instance.GetSourceNodes();
            goalNodes = GridManager.Instance.GetGoalNodes();

            if (sourceNodes.Count == 0)
                Debug.LogError($"[LevelManager] No source nodes in '{currentLevel.LevelName}'!");
            if (goalNodes.Count == 0)
                Debug.LogError($"[LevelManager] No goal nodes in '{currentLevel.LevelName}'!");

            levelStartTime = Time.time;
            totalRotationsThisLevel = 0;
            isLevelActive = true;

            GameEvents.TriggerLevelStarted(currentLevel.LevelNumber, currentLevel.Difficulty);
            UpdateConnections();
        }

        // Handle node rotation events
        private void OnNodeRotated(object nodeObj, int rotationCount)
        {
            HexNode node = nodeObj as HexNode;
            if (node == null) return;

            totalRotationsThisLevel++;

            if (autoCheckWinCondition && isLevelActive)
                CheckWinConditionDelayed();
        }

        // ADD THIS METHOD inside LevelManager.cs
        private void ScrambleNodes()
        {
            // Get all nodes from the GridManager
            Dictionary<Vector2Int, HexNode> gridNodes = GridManager.Instance.GetGridNodes();

            foreach (var node in gridNodes.Values)
            {
                // Skip nodes that shouldn't move (Sources, or locked tiles)
                if (!node.CanRotate) continue;

                // Pick a random rotation 0-5
                int randomRot = Random.Range(0, 6);

                // Apply it instantly
                node.SetInitialRotation(randomRot);
            }

            Debug.Log("[LevelManager] Level Scrambled!");
        }

        #endregion

        #region Connections

        // Update all node power states based on connections from sources
        public HashSet<HexNode> UpdateConnections()
        {
            Dictionary<Vector2Int, HexNode> gridNodes = GridManager.Instance.GetGridNodes();

            if (gridNodes == null || gridNodes.Count == 0)
                return new HashSet<HexNode>();

            HashSet<HexNode> allPoweredNodes = new HashSet<HexNode>();

            // Find all nodes connected to each source
            foreach (HexNode source in sourceNodes)
            {
                if (source == null) continue;

                HashSet<HexNode> connectedNodes = PathFinder.FindConnectedNodes(source, gridNodes);

                foreach (HexNode node in connectedNodes)
                    allPoweredNodes.Add(node);
            }

            PathFinder.UpdatePoweredStates(allPoweredNodes, gridNodes);
            GameEvents.TriggerConnectionsUpdated();

            return allPoweredNodes;
        }

        #endregion

        #region Win Condition

        private void CheckWinConditionDelayed()
        {
            CancelInvoke(nameof(CheckWinCondition));
            Invoke(nameof(CheckWinCondition), winCheckDelay);
        }

        // Check if all goals are powered
        private void CheckWinCondition()
        {
            if (!isLevelActive) return;

            HashSet<HexNode> poweredNodes = UpdateConnections();
            bool isWinning = PathFinder.CheckWinCondition(goalNodes, poweredNodes);

            GameEvents.TriggerWinConditionChanged(isWinning);

            if (isWinning)
                OnLevelComplete();
        }

        #endregion

        #region Completion

        private void OnLevelComplete()
        {
            if (!isLevelActive) return;

            isLevelActive = false;
            float completionTime = Time.time - levelStartTime;

            GameEvents.TriggerLevelCompleted(currentLevel.LevelNumber, completionTime, totalRotationsThisLevel);

            Debug.Log($"=== LEVEL COMPLETE ===");
            Debug.Log($"Level: {currentLevel.LevelName} (#{currentLevel.LevelNumber})");
            Debug.Log($"Time: {completionTime:F2}s");
            Debug.Log($"Rotations: {totalRotationsThisLevel}");
            Debug.Log($"Stars: {CalculateStars(totalRotationsThisLevel)}");
        }

        private int CalculateStars(int rotations)
        {
            if (rotations <= 10) return 3;
            if (rotations <= 20) return 2;
            return 1;
        }

        #endregion

        #region Queries

        public int GetCurrentLevelNumber()
        {
            return currentLevel?.LevelNumber ?? 0;
        }

        public int GetTotalRotations()
        {
            return totalRotationsThisLevel;
        }

        public float GetElapsedTime()
        {
            if (!isLevelActive) return 0f;
            return Time.time - levelStartTime;
        }

        public bool IsLevelActive()
        {
            return isLevelActive;
        }

        public LevelData GetCurrentLevel()
        {
            return currentLevel;
        }

        public LevelData GetLevelData(int levelNum)
        {
            if (allLevels.Length >= levelNum)
                return allLevels[levelNum];
            else
                return null;
        }
        #endregion

        #region Debug

        [ContextMenu("Force Win Check")]
        private void ForceWinCheck()
        {
            CheckWinCondition();
        }

        [ContextMenu("Print Level Stats")]
        private void PrintStats()
        {
            if (currentLevel == null)
            {
                Debug.Log("[LevelManager] No level loaded.");
                return;
            }

            Debug.Log($"=== LEVEL STATS ===");
            Debug.Log($"Level: {currentLevel.LevelName} (#{currentLevel.LevelNumber})");
            Debug.Log($"Active: {isLevelActive}");
            Debug.Log($"Elapsed Time: {GetElapsedTime():F2}s");
            Debug.Log($"Rotations: {totalRotationsThisLevel}");
            Debug.Log($"Sources: {sourceNodes.Count}");
            Debug.Log($"Goals: {goalNodes.Count}");

            int poweredGoals = 0;
            foreach (var goal in goalNodes)
            {
                if (goal != null && goal.IsPowered)
                    poweredGoals++;
            }
            Debug.Log($"Powered Goals: {poweredGoals}/{goalNodes.Count}");
        }

        [ContextMenu("Update Connections")]
        private void DebugUpdateConnections()
        {
            HashSet<HexNode> powered = UpdateConnections();
            Debug.Log($"[LevelManager] Connections updated. Powered nodes: {powered.Count}");
        }

        #endregion
    }
}
