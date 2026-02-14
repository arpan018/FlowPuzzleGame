using DG.Tweening;
using Game.Sounds;
using MPUIKIT;
using UnityEngine;
using UnityEngine.UI;

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
                UIController.Instance.HideThisScreen(ScreenType.SplashScreen);
                UIController.Instance.ShowThisScreen(ScreenType.LevelSelectionScreen);
                SoundManager.PlaySoundLoop(SoundManager.SoundType.BgMusic);
            });
        }

    }
}
