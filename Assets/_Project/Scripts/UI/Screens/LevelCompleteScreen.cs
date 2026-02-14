using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Gameplay;
using Game.Sounds;

namespace Game.UI
{
    public class LevelCompleteScreen : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI levelNumberText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI movesText;
        [SerializeField] private Button continueButton;

        private int completedLevelIndex;
        private float completionTime;
        private int totalMoves;

        void Start()
        {

        }

        private void OnEnable()
        {
            continueButton.onClick.AddListener(OnContinueButtonClick);

            // Get completion stats from LevelManager
            if (LevelManager.Instance != null)
            {
                completedLevelIndex = LevelManager.Instance.CurrentLevelIndex;
                completionTime = LevelManager.Instance.GetLevelCompletionTime();
                totalMoves = LevelManager.Instance.GetTotalRotations();

                UpdateStatsDisplay();
            }
        }

        private void OnDisable()
        {
            continueButton.onClick.RemoveListener(OnContinueButtonClick);
        }

        public void UpdateData() 
        {
            completedLevelIndex = LevelManager.Instance.CurrentLevelIndex;
            completionTime = LevelManager.Instance.GetLevelCompletionTime();
            totalMoves = LevelManager.Instance.GetTotalRotations();

            UpdateStatsDisplay();
        }

        private void UpdateStatsDisplay()
        {
            // Display level completed stats
            levelNumberText.text = (completedLevelIndex + 1).ToString();
            int minutes = Mathf.FloorToInt(completionTime / 60f);
            int seconds = Mathf.FloorToInt(completionTime % 60f);
            timeText.text = $"Time: {minutes:00}:{seconds:00}";
            movesText.text = $"Moves: {totalMoves}";
        }

        private void OnContinueButtonClick()
        {
            SoundManager.PlaySound(SoundManager.SoundType.Button);
            UIController.Instance.HideThisScreen(ScreenType.LevelCompleteScreen);


            int nextLevelIndex = completedLevelIndex + 1;
            if (LevelManager.Instance != null &&
                nextLevelIndex < LevelManager.Instance.AllLevels.Length)
            {
                LevelManager.Instance.LoadLevel(nextLevelIndex);
            }
            else
            {
                UIController.Instance.HideThisScreen(ScreenType.GamePlayScreen);
                UIController.Instance.ShowThisScreen(ScreenType.LevelSelectionScreen);
            }
        }
    }
}
