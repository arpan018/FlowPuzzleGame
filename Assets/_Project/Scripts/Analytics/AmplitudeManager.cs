using UnityEngine;
using System.Collections.Generic;
using Game.Core;
using System;

namespace Game.Analytics
{
    public class AmplitudeManager : MonoBehaviourSingleton<AmplitudeManager>
    {
        private const string API_KEY = "KEY_HERE";
        private static Amplitude amplitude;

        private const string PREF_TOTAL_COMPLETED = "TotalLevelsCompleted";

        #region Mono Callbacks


        private void Start()
        {
            InitializeAmplitude();
        }

        private void OnEnable()
        {
            GameEvents.OnLevelStarted += OnLevelStarted;
            GameEvents.OnLevelCompleted += OnLevelCompleted;
        }

        private void OnDisable()
        {
            GameEvents.OnLevelStarted -= OnLevelStarted;
            GameEvents.OnLevelCompleted -= OnLevelCompleted;
        }

        private void OnLevelCompleted(int levelNumber, float time, int rotations)
        {
            TrackLevelCompleted(levelNumber, time, rotations);
        }

        private void OnLevelStarted(int levelNumber, GameDifficulty difficulty)
        {
            TrackLevelStarted(levelNumber, difficulty);
        }

        #endregion
        private void InitializeAmplitude()
        {
            amplitude = Amplitude.getInstance();
            amplitude.setServerZone(AmplitudeServerZone.EU);
            //amplitude.setServerUrl("https://api.eu.amplitude.com/2/httpapi");

            amplitude.logging = true;
            amplitude.trackSessionEvents(true);
            amplitude.init(API_KEY);

            Debug.Log("[AmplitudeManager] Initialized with EU server");
        }

        public void TrackGameStarted()
        {
            if (Instance == null || amplitude == null) return;

            amplitude.logEvent("game_started");
            Debug.Log("[AmplitudeManager] Event: game_started");
        }

        public void TrackLevelStarted(int levelNumber, Game.Core.GameDifficulty difficulty)
        {
            if (Instance == null || amplitude == null) return;

            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                { "level_number", levelNumber },
                { "difficulty", difficulty.ToString() }
            };

            amplitude.logEvent("level_started", properties);
            Debug.Log($"[AmplitudeManager] Event: level_started (Level {levelNumber}, {difficulty})");
        }

        public void TrackLevelCompleted(int levelNumber, float timeSeconds, int rotations)
        {
            if (Instance == null || amplitude == null) return;

            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                { "level_number", levelNumber },
                { "time", timeSeconds },
                { "rotations", rotations }
            };

            amplitude.logEvent("level_completed", properties);

            IncrementCompletedLevels();

            Debug.Log($"[AmplitudeManager] Event: level_completed (Level {levelNumber}, Time: {timeSeconds:F1}s, Moves: {rotations})");
        }

        private int GetTotalCompletedLevels()
        {
            return PlayerPrefs.GetInt(PREF_TOTAL_COMPLETED, 0);
        }

        private void IncrementCompletedLevels()
        {
            int currentTotal = GetTotalCompletedLevels();
            currentTotal++;
            PlayerPrefs.SetInt(PREF_TOTAL_COMPLETED, currentTotal);
            PlayerPrefs.Save();

            Dictionary<string, object> values = new Dictionary<string, object>();
            amplitude.addUserProperty("total_levels_completed", currentTotal);
            Debug.Log($"[AmplitudeManager] User property updated: total_levels_completed = {currentTotal}");
        }
    }
}
