using Game.Data;
using Game.Gameplay;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI levelNumberText;
        [SerializeField] private TextMeshProUGUI difficultyText;
        [SerializeField] private GameObject lockIcon;

        [ShowNonSerializedField] private int levelIndex;
        private LevelSelectionScreen manager;

        public void Setup(int index, bool isUnlocked, LevelSelectionScreen mgr)
        {
            levelIndex = index;
            manager = mgr;

            // Set level number
            levelNumberText.text = (index + 1).ToString();

            // Set difficulty from LevelData
            LevelData levelData = LevelManager.Instance.GetLevelData(index);
            difficultyText.text = levelData.Difficulty.ToString();

            // Set lock state
            button.interactable = isUnlocked;
            lockIcon.SetActive(!isUnlocked);

            // Add click listener
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            manager.OnLevelButtonClicked(levelIndex);
        }
    }
}
