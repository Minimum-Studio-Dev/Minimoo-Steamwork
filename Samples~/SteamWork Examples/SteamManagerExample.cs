using UnityEngine;
using Minimoo.SteamWork;
using Minimoo;

public class SteamManagerExample : MonoBehaviour
{
    private void Start()
    {
        // SteamManager는 자동으로 초기화됩니다
        if (SteamManager.Instance.IsSteamInitialized)
        {
            D.Log("Steam 초기화 성공!");
            D.Log($"사용자 이름: {SteamManager.Instance.UserPersonaName}");
            D.Log($"Steam ID: {SteamManager.Instance.UserSteamId}");
        }
        else
        {
            D.Error("Steam 초기화 실패!");
        }
    }

    private void Update()
    {
        // ESC 키로 Steam 오버레이 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SteamLogin.OpenSteamOverlay();
        }

        // F1 키로 친구 목록 오버레이
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SteamLogin.OpenFriendsOverlay();
        }

        // F2 키로 프로필 오버레이
        if (Input.GetKeyDown(KeyCode.F2))
        {
            SteamLogin.OpenProfileOverlay();
        }
    }
}
