using UnityEngine;
using System.Collections.Generic;
using Game.Core;
using Game.Data;
using Game.Utilities;
using Game.Analytics;
using Game.UI;
using Game.Sounds;

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

        private LevelData currentLevel;
        private List<HexNode> sourceNodes = new List<HexNode>();
        private List<HexNode> goalNodes = new List<HexNode>();
        private float levelStartTime;
        private int totalRotationsThisLevel;
        private bool isLevelActive;
        private float levelCompletionTime;

        #endregion

        #region Properties
        public int CurrentLevelIndex { get; private set; } = 0;

        public LevelData[] AllLevels => allLevels;

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


            CurrentLevelIndex = levelIndex;
            currentLevel = allLevels[levelIndex];

            Debug.Log($"=== load Level {currentLevel.LevelNumber}");
            if (currentLevel == null)
            {
                Debug.LogError($"[LevelManager] Level at index {levelIndex} is null!");
                return;
            }

            // Track level start in Amplitude
            AmplitudeManager.Instance.TrackLevelStarted(
                currentLevel.LevelNumber,
                currentLevel.Difficulty
            );

            GridManager.Instance.ToggleGrid(true);
            UIController.Instance.ShowThisScreen(ScreenType.GamePlayScreen);
            StartLevel();
        }

        //public void RestartCurrentLevel()
        //{
        //    if (currentLevel == null) return;
        //    LoadLevel(CurrentLevelIndex);
        //}

        public void CleanupCurrentLevel()
        {
            // Stop level activity
            isLevelActive = false;

            GridManager.Instance.ClearGrid();
            sourceNodes.Clear();
            goalNodes.Clear();
            totalRotationsThisLevel = 0;

            Debug.Log("[LevelManager] Current level cleaned up");
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
            levelCompletionTime = Time.time - levelStartTime;

            // Increment total completed level
            IncrementTotalCompletedLevels();

            SoundManager.PlaySound(SoundManager.SoundType.Win);

            AmplitudeManager.Instance.TrackLevelCompleted(
                CurrentLevelIndex,
                levelCompletionTime,
                totalRotationsThisLevel
            );

            LevelSelectionScreen.UnlockNextLevel(CurrentLevelIndex);

            // Show level complete screen with stats
            GridManager.Instance.ToggleGrid(false);
            UIController.Instance.HideThisScreen(ScreenType.GamePlayScreen);
            UIController.Instance.ShowThisScreen(ScreenType.LevelCompleteScreen);

            Debug.Log($"=== LEVEL COMPLETE ===");
            Debug.Log($"Level: {currentLevel.LevelName} (#{currentLevel.LevelNumber})");
            Debug.Log($"[LevelManager] Level {CurrentLevelIndex + 1} completed! Time: {levelCompletionTime:F1}s, Moves: {totalRotationsThisLevel}");
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
            return currentLevel.LevelNumber;
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

        public LevelData GetLevelData(int index)
        {
            if (index < 0 || index >= allLevels.Length) return null;
            return allLevels[index];
        }

        public float GetLevelCompletionTime()
        {
            return levelCompletionTime;
        }

        #endregion

        #region Total Levels Completed Tracking

        private const string PREF_TOTAL_COMPLETED = "TotalLevelsCompleted";

        // Get total completed levels from PlayerPrefs (static for easy access)
        public static int GetTotalCompletedLevels()
        {
            return PlayerPrefs.GetInt(PREF_TOTAL_COMPLETED, 0);
        }

        // Increment total completed levels
        private void IncrementTotalCompletedLevels()
        {
            int currentTotal = GetTotalCompletedLevels();
            currentTotal++;
            PlayerPrefs.SetInt(PREF_TOTAL_COMPLETED, currentTotal);
            PlayerPrefs.Save();

            Debug.Log($"[LevelManager] Total completed levels: {currentTotal}");
        }

        #endregion
    }
}
