using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using MPUIKIT;

namespace Game.UI
{
    public class SplashScreen : MonoBehaviour
    {
        [SerializeField] private MPImage progressBar;
        [SerializeField] private Text loading;
        [SerializeField] private float loadingtime = 2f;


        private void Start()
        {
            loading.DOText("Loading...", loadingtime).SetLoops(-1, LoopType.Restart);
            progressBar.DOFillAmount(1, loadingtime).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                // after splash
                UIController.Instance.HideThisScreen(ScreenType.SplashScreen);
                UIController.Instance.ShowThisScreen(ScreenType.LevelSelectionScreen);

                // after level selection
                UIController.Instance.HideThisScreen(ScreenType.LevelSelectionScreen);
                UIController.Instance.ShowThisScreen(ScreenType.GamePlayScreen);

                // setting button click, do not hide gameplay, just overlay setting screen. pause game timer and calculation.
                UIController.Instance.ShowThisScreen(ScreenType.SettingsScreen);

                // setting screen close button, just hide overlay, resume game
                UIController.Instance.HideThisScreen(ScreenType.SettingsScreen);

                // seting screen level button: go to level selection screen. Reset all level relatedstuff, remove tiles, events
                UIController.Instance.HideThisScreen(ScreenType.GamePlayScreen);
                UIController.Instance.HideThisScreen(ScreenType.SettingsScreen);
                UIController.Instance.ShowThisScreen(ScreenType.LevelSelectionScreen);

                // on level complete
                UIController.Instance.ShowThisScreen(ScreenType.LevelCompleteScreen);

                // level complete screen next/continue button: load next level and hide level complete screen,since gameplay is already active. 
                UIController.Instance.HideThisScreen(ScreenType.LevelCompleteScreen);

                // gameplay screen elemnts: current level text, setting button
                // setting popup: sound toggle, level selection screen button
                // level complete screen: last completed level number text (int) with labels, continue button to play next level
            });
        }

    }
}
