using System;

namespace Game.Core
{
    /// <summary>
    /// Static event system for game-wide communication using the Observer pattern.
    /// Subscribe: GameEvents.OnLevelStarted += HandleLevelStarted;
    /// Unsubscribe: GameEvents.OnLevelStarted -= HandleLevelStarted;
    /// Always unsubscribe in OnDestroy/OnDisable to prevent memory leaks.
    /// </summary>
    public static class GameEvents
    {
        #region Level Events

        public static event Action<int> OnLevelStarted;
        public static event Action<int, float, int> OnLevelCompleted; // levelNumber, completionTime, totalRotations
        public static event Action<int> OnLevelFailed;

        public static void TriggerLevelStarted(int levelNumber)
        {
            OnLevelStarted?.Invoke(levelNumber);
        }

        public static void TriggerLevelCompleted(int levelNumber, float completionTime, int totalRotations)
        {
            OnLevelCompleted?.Invoke(levelNumber, completionTime, totalRotations);
        }

        public static void TriggerLevelFailed(int levelNumber)
        {
            OnLevelFailed?.Invoke(levelNumber);
        }

        #endregion

        #region Node Events

        public static event Action<object> OnNodeTapped;
        public static event Action<object, int> OnNodeRotated; // node, rotationCount
        public static event Action<object> OnNodePowered;
        public static event Action<object> OnNodeUnpowered;

        public static void TriggerNodeTapped(object node)
        {
            OnNodeTapped?.Invoke(node);
        }

        public static void TriggerNodeRotated(object node, int rotationCount)
        {
            OnNodeRotated?.Invoke(node, rotationCount);
        }

        public static void TriggerNodePowered(object node)
        {
            OnNodePowered?.Invoke(node);
        }

        public static void TriggerNodeUnpowered(object node)
        {
            OnNodeUnpowered?.Invoke(node);
        }

        #endregion

        #region Connection Events

        public static event Action OnConnectionsUpdated;
        public static event Action<bool> OnWinConditionChanged; // isWinning

        public static void TriggerConnectionsUpdated()
        {
            OnConnectionsUpdated?.Invoke();
        }

        public static void TriggerWinConditionChanged(bool isWinning)
        {
            OnWinConditionChanged?.Invoke(isWinning);
        }

        #endregion

        #region UI Events

        public static event Action<bool> OnPauseToggled; // isPaused
        public static event Action OnSettingsOpened;
        public static event Action OnSettingsClosed;

        public static void TriggerPauseToggled(bool isPaused)
        {
            OnPauseToggled?.Invoke(isPaused);
        }

        public static void TriggerSettingsOpened()
        {
            OnSettingsOpened?.Invoke();
        }

        public static void TriggerSettingsClosed()
        {
            OnSettingsClosed?.Invoke();
        }

        #endregion

        #region Event Management

        // Clear all event subscriptions to prevent memory leaks during scene transitions
        public static void ClearAllEvents()
        {
            OnLevelStarted = null;
            OnLevelCompleted = null;
            OnLevelFailed = null;

            OnNodeTapped = null;
            OnNodeRotated = null;
            OnNodePowered = null;
            OnNodeUnpowered = null;

            OnConnectionsUpdated = null;
            OnWinConditionChanged = null;

            OnPauseToggled = null;
            OnSettingsOpened = null;
            OnSettingsClosed = null;
        }

        #endregion
    }
}
