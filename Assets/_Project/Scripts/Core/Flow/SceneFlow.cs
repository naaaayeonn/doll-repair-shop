using System;
using UnityEngine;

namespace DollShop.Core
{
    /// <summary>
    /// Boot → Title → Tutorial → Shop_Main 씬 전환 API.
    /// 실제 씬 로드는 DEV-B(View)가 이 이벤트를 구독해서 처리한다.
    /// Core는 씬 이름을 직접 로드하지 않는다.
    /// </summary>
    public static class SceneFlow
    {
        public enum Screen
        {
            Boot,
            Title,
            Tutorial,
            Game,   // Shop_Main
            Settlement,
            Ending
        }

        public static Screen CurrentScreen { get; private set; } = Screen.Boot;

        /// <summary>화면 전환 요청. View가 구독하여 실제 씬 로드를 수행한다.</summary>
        public static event Action<Screen> OnTransitionRequested;

        public static void GoTo(Screen screen)
        {
            if (CurrentScreen == screen) return;
            CurrentScreen = screen;
            OnTransitionRequested?.Invoke(screen);
        }

        public static void GoToGame()       => GoTo(Screen.Game);
        public static void GoToTitle()      => GoTo(Screen.Title);
        public static void GoToTutorial()   => GoTo(Screen.Tutorial);
        public static void GoToSettlement() => GoTo(Screen.Settlement);
        public static void GoToEnding()     => GoTo(Screen.Ending);
    }
}
