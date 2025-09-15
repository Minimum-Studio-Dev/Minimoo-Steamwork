using UnityEditor;
using Minimoo.SteamWork;

namespace Minimoo.SteamWork.Editor
{
    /// <summary>
    /// Steam Manager 테스트 모드 에디터 메뉴
    /// </summary>
    [InitializeOnLoad]
    public class SteamManagerTestModeEditor
    {
        private const string ENABLE_STEAM_TEST_MENU = "MINIMOO/Steam/Enable Steam Test";
        private const string DISABLE_STEAM_TEST_MENU = "MINIMOO/Steam/Disable Steam Test";

        [MenuItem(ENABLE_STEAM_TEST_MENU, true)]
        private static bool ShowEnableSteamTestMenu()
        {
            return !SteamManagerTestMode.IsEnabled;
        }

        [MenuItem(ENABLE_STEAM_TEST_MENU)]
        private static void EnableSteamTest()
        {
            SteamManagerTestMode.IsEnabled = true;
            EditorUtility.DisplayDialog("Steam Manager 테스트 모드",
                "Steam Manager 테스트 모드가 활성화되었습니다.\n게임을 시작하면 Steam 기능이 활성화됩니다.",
                "확인");
        }

        [MenuItem(DISABLE_STEAM_TEST_MENU, true)]
        private static bool ShowDisableSteamTestMenu()
        {
            return SteamManagerTestMode.IsEnabled;
        }

        [MenuItem(DISABLE_STEAM_TEST_MENU)]
        private static void DisableSteamTest()
        {
            SteamManagerTestMode.IsEnabled = false;
            EditorUtility.DisplayDialog("Steam Manager 테스트 모드",
                "Steam Manager 테스트 모드가 비활성화되었습니다.\n게임을 시작하면 Steam 기능이 비활성화됩니다.",
                "확인");
        }
    }
}
