using Game.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public class LevelSelectionScreen : MonoBehaviour
    {
        [SerializeField] private LevelButton[] levelButtons;

        private const string PREF_CURRENT_LEVEL = "CurrentLevel";

        void Start()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            int unlockedLevel = GetUnlockedLevel();

            for (int i = 0; i < levelButtons.Length; i++)
            {
                bool isUnlocked = i <= unlockedLevel;
                levelButtons[i].Setup(i, isUnlocked, this);
            }
        }

        public void OnLevelButtonClicked(int levelIndex)
        {
            // Load the level
            LevelManager.Instance.LoadLevel(levelIndex);

            // Hide level selection UI
            gameObject.SetActive(false);
        }

        public static int GetUnlockedLevel()
        {
            return PlayerPrefs.GetInt(PREF_CURRENT_LEVEL, 0);
        }

        public static void UnlockNextLevel(int completedLevel)
        {
            int currentUnlocked = GetUnlockedLevel();
            if (completedLevel >= currentUnlocked)
            {
                PlayerPrefs.SetInt(PREF_CURRENT_LEVEL, completedLevel + 1);
                PlayerPrefs.Save();
            }
        }
    }
}
