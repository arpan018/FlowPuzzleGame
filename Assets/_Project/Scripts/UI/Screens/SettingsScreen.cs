using UnityEngine;
using UnityEngine.UI;
using Game.Sounds;
using Game.Gameplay;

namespace Game.UI
{
    public class SettingsScreen : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Toggle soundToggle;
        [SerializeField] private Button levelsButton;

        private void OnEnable()
        {
            closeButton.onClick.AddListener(OnCloseButtonClick);
            levelsButton.onClick.AddListener(OnLevelsButtonClick);

            soundToggle.isOn = SoundManager.IsSoundEnabled;
            soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
        }

        private void OnDisable()
        {
            closeButton.onClick.RemoveListener(OnCloseButtonClick);
            levelsButton.onClick.RemoveListener(OnLevelsButtonClick);
            soundToggle.onValueChanged.RemoveListener(OnSoundToggleChanged);
        }

        private void OnCloseButtonClick()
        {
            SoundManager.PlaySound(SoundManager.SoundType.Button);
            Time.timeScale = 1f;

            GridManager.Instance.ToggleGrid(true);
            UIController.Instance.HideThisScreen(ScreenType.SettingsScreen);
            GamePlayScreen gameplayScreen = FindObjectOfType<GamePlayScreen>();
            if (gameplayScreen != null)
                gameplayScreen.ResumeGame();
        }

        private void OnSoundToggleChanged(bool isOn)
        {
            SoundManager.ToggleSound();
            
            if (isOn)
                SoundManager.PlaySound(SoundManager.SoundType.Button);
        }

        private void OnLevelsButtonClick()
        {
            SoundManager.PlaySound(SoundManager.SoundType.Button);
            Time.timeScale = 1f;

            LevelManager.Instance.CleanupCurrentLevel();

            UIController.Instance.HideThisScreen(ScreenType.GamePlayScreen);
            UIController.Instance.HideThisScreen(ScreenType.SettingsScreen);
            UIController.Instance.ShowThisScreen(ScreenType.LevelSelectionScreen);
        }
    }
}
