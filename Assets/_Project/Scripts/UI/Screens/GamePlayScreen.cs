using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Core;
using Game.Gameplay;
using Game.Sounds;

namespace Game.UI
{
    public class GamePlayScreen : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI movesText;
        [SerializeField] private Button settingsButton;
        
        private float levelStartTime;
        private int moveCount;
        private bool isLevelActive;

        private void OnEnable()
        {
            GameEvents.OnLevelStarted += OnLevelStarted;
            GameEvents.OnNodeRotated += OnNodeRotated;
            settingsButton.onClick.AddListener(OnSettingsButtonClick);
        }
        
        private void OnDisable()
        {
            GameEvents.OnLevelStarted -= OnLevelStarted;
            GameEvents.OnNodeRotated -= OnNodeRotated;
            settingsButton.onClick.RemoveListener(OnSettingsButtonClick);
        }
        
        private void Update()
        {
            // Update timer continuously during active gameplay
            if (isLevelActive && Time.timeScale > 0)
            {
                UpdateTimeDisplay();
            }
        }
        
        private void OnLevelStarted(int levelNumber, GameDifficulty difficulty)
        {
            // Reset stats for new level
            levelStartTime = Time.time;
            moveCount = 0;
            isLevelActive = true;
            
            // Update all displays
            UpdateLevelDisplay();
            UpdateMovesDisplay();
            UpdateTimeDisplay();
        }
        
        private void OnNodeRotated(object node, int rotationCount)
        {
            // Increment move counter
            moveCount++;
            UpdateMovesDisplay();
        }
        
        private void UpdateLevelDisplay()
        {
            if (LevelManager.Instance != null)
            {
                int currentLevel = LevelManager.Instance.GetCurrentLevelNumber();
                levelText.text = $"Level {currentLevel}";
            }
        }
        
        private void UpdateTimeDisplay()
        {
            if (timeText != null)
            {
                float elapsed = Time.time - levelStartTime;
                int minutes = Mathf.FloorToInt(elapsed / 60f);
                int seconds = Mathf.FloorToInt(elapsed % 60f);
                timeText.text = $"Time: {minutes:00}:{seconds:00}";
            }
        }
        
        private void UpdateMovesDisplay()
        {
            if (movesText != null)
            {
                movesText.text = $"Moves: {moveCount}";
            }
        }
        
        private void OnSettingsButtonClick()
        {
            SoundManager.PlaySound(SoundManager.SoundType.Button);
            Time.timeScale = 0f;
            isLevelActive = false;
            GridManager.Instance.ToggleGrid(false);

            UIController.Instance.ShowThisScreen(ScreenType.SettingsScreen);
        }
        
        public void ResumeGame()
        {
            isLevelActive = true;
        }
    }
}
