using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Core;

namespace Game.UI
{
    public enum ScreenType
    {
        SplashScreen,
        SettingsScreen,
        LevelSelectionScreen,
        GamePlayScreen,
        LevelCompleteScreen
    }

    [Serializable]
    public struct ScreenCollection
    {
        public UIScreen _screen;
        public ScreenType _type;
    }

    public class UIController : MonoBehaviourSingleton<UIController>
    {
        [SerializeField] List<ScreenCollection> _allScreens;

        [SerializeField] ScreenType _currentScreen;
        [SerializeField] ScreenType _previousScreen;
        [SerializeField] ScreenType _initScreen;

        void Start()
        {
            ShowThisScreen(_initScreen);
        }

        public void ShowThisScreen(ScreenType _screenToShow, Action _tempAction = null)
        {
            _previousScreen = _currentScreen;
            UIScreen m_screen = FindScreen(_screenToShow);
            _currentScreen = _screenToShow;
            m_screen.AnimationPenal(true);
        }

        public void HideThisScreen(ScreenType _screenToHide, Action _tempAction = null)
        {
            UIScreen m_screen = FindScreen(_screenToHide);
            m_screen.AnimationPenal(false);
        }

        public void OpenPreviousScreen(Action _tempAction = null)
        {
            ShowThisScreen(_previousScreen, _tempAction);
        }

        public void HideCurrentScreen(Action _tempAction = null)
        {
            HideThisScreen(_currentScreen, _tempAction);
        }

        UIScreen FindScreen(ScreenType _type)
        {
            return _allScreens.Find(x => (x._type == _type))._screen;
        }

        public void RemoveScreen(ScreenType screenType)
        {

            _allScreens.RemoveAll(screen => screen._type == screenType);
        }

        public void AddScreen(ScreenCollection newScreen)
        {
            //Debug.Log($"UIController" + newScreen);
            if (!_allScreens.Exists(screen => screen._type == newScreen._type))
            {
                _allScreens.Add(newScreen);
            }
        }
    }
}
