using UnityEngine;
using System.Collections.Generic;
using Game.Core;
using Game.Gameplay;

namespace Game.Analytics
{
    public class AmplitudeManager : MonoBehaviourSingleton<AmplitudeManager>
    {
        private const string API_KEY = "6b427a9577f88e0d1c34b7b57a5a22e3";
        private static Amplitude amplitude;

        #region Mono Callbacks

        private void Start()
        {
            InitializeAmplitude();
        }

        #endregion
        
        private void InitializeAmplitude()
        {
            amplitude = Amplitude.getInstance();
            amplitude.setServerZone(AmplitudeServerZone.EU);
            //amplitude.setServerUrl("https://api.eu.amplitude.com");


            amplitude.logging = true;
            amplitude.trackSessionEvents(true);
            amplitude.init(API_KEY);
            amplitude.setEventUploadPeriodSeconds(10);

            //Debug.Log("[AmplitudeManager] Initialized with EU server");
            TrackGameStarted();
        }

        public void TrackGameStarted()
        {
            if (Instance == null || amplitude == null) return;

            amplitude.logEvent("game_started");
            amplitude.uploadEvents();
            //Debug.Log("[AmplitudeManager] Event: game_started");
        }

        public void TrackLevelStarted(int levelNumber, GameDifficulty difficulty)
        {
            if (Instance == null || amplitude == null) return;

            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                { "level_number", levelNumber },
                { "difficulty", difficulty.ToString() }
            };

            amplitude.logEvent("level_started", properties);
            amplitude.uploadEvents();

            //Debug.Log($"[AmplitudeManager] Event: level_started (Level {levelNumber}, {difficulty})");
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

            // Get total from LevelManager (single source of truth)
            int totalCompleted = LevelManager.GetTotalCompletedLevels();
            amplitude.addUserProperty("total_levels_completed", totalCompleted);

            amplitude.uploadEvents();
            //Debug.Log($"[AmplitudeManager] Event: level_completed (Level {levelNumber}, Time: {timeSeconds:F1}s, Moves: {rotations})");
            //Debug.Log($"[AmplitudeManager] User property: total_levels_completed = {totalCompleted}");
        }
    }
}
