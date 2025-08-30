using UnityEngine;
using Minimoo.SteamWork;
using Minimoo;

public class ActivityExample : MonoBehaviour
{
    private int currentLevel = 1;
    private int playerCount = 1;
    private int maxPlayers = 4;

    private void Start()
    {
        // 초기 상태 설정
        SteamActivity.SetMenuActivity();
        D.Log("Steam Activity 예제 시작 - 메뉴 상태로 설정됨");
    }

    private void Update()
    {
        // 1 키로 메뉴 상태
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SteamActivity.SetMenuActivity();
            D.Log("메뉴 상태로 변경");
        }

        // 2 키로 게임 플레이 상태
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SteamActivity.SetPlayingActivity(currentLevel);
            D.Log($"게임 플레이 상태로 변경 - 레벨 {currentLevel}");
        }

        // 3 키로 로비 상태
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SteamActivity.SetLobbyActivity(playerCount, maxPlayers);
            D.Log($"로비 상태로 변경 - {playerCount}/{maxPlayers}");
        }

        // 4 키로 커스텀 상태
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetCustomActivity();
        }

        // C 키로 연결 정보 설정
        if (Input.GetKeyDown(KeyCode.C))
        {
            SteamActivity.SetConnectInfo("+connect 127.0.0.1:27015");
            D.Log("연결 정보 설정됨");
        }

        // X 키로 Rich Presence 클리어
        if (Input.GetKeyDown(KeyCode.X))
        {
            SteamActivity.ClearRichPresence();
            D.Log("Rich Presence 클리어됨");
        }

        // F 키로 친구 목록 표시
        if (Input.GetKeyDown(KeyCode.F))
        {
            ShowFriendsList();
        }

        // 마우스 휠로 레벨 조정
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0)
        {
            currentLevel = Mathf.Min(currentLevel + 1, 10);
            if (SteamActivity.GetFriendRichPresence(SteamManager.Instance.UserSteamId, "status").Contains("레벨"))
            {
                SteamActivity.SetPlayingActivity(currentLevel);
            }
        }
        else if (scroll < 0)
        {
            currentLevel = Mathf.Max(currentLevel - 1, 1);
            if (SteamActivity.GetFriendRichPresence(SteamManager.Instance.UserSteamId, "status").Contains("레벨"))
            {
                SteamActivity.SetPlayingActivity(currentLevel);
            }
        }
    }

    private void SetCustomActivity()
    {
        var activityInfo = new ActivityInfo
        {
            Status = "커스텀 모드",
            DisplayText = "커스텀 게임 플레이 중",
            ConnectString = "+connect_custom localhost:7777",
            CustomRichPresence = new Dictionary<string, string>()
        };

        // 커스텀 Rich Presence 추가
        activityInfo.CustomRichPresence["difficulty"] = "Hard";
        activityInfo.CustomRichPresence["game_mode"] = "Survival";
        activityInfo.CustomRichPresence["character"] = "Warrior";

        SteamActivity.SetGameActivity(activityInfo);
        D.Log("커스텀 Activity 설정됨");
    }

    private void ShowFriendsList()
    {
        var friends = SteamActivity.GetFriendList();
        D.Log($"친구 목록 ({friends.Count}명):");

        foreach (var friendId in friends)
        {
            string name = SteamActivity.GetFriendName(friendId);
            string status = SteamActivity.GetFriendGameStatus(friendId);
            string richPresence = SteamActivity.GetFriendRichPresence(friendId, "status");

            D.Log($"{name}: {status}");
            if (!string.IsNullOrEmpty(richPresence))
            {
                D.Log($"  Rich Presence: {richPresence}");
            }
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 400, 20), $"현재 레벨: {currentLevel}");
        GUI.Label(new Rect(10, 30, 400, 20), $"플레이어: {playerCount}/{maxPlayers}");

        string currentStatus = SteamActivity.GetFriendRichPresence(SteamManager.Instance.UserSteamId, "status");
        GUI.Label(new Rect(10, 50, 400, 20), $"현재 상태: {currentStatus}");

        GUI.Label(new Rect(10, 80, 400, 20), "키 설명:");
        GUI.Label(new Rect(10, 100, 400, 20), "1: 메뉴 상태");
        GUI.Label(new Rect(10, 120, 400, 20), "2: 게임 플레이 상태");
        GUI.Label(new Rect(10, 140, 400, 20), "3: 로비 상태");
        GUI.Label(new Rect(10, 160, 400, 20), "4: 커스텀 상태");
        GUI.Label(new Rect(10, 180, 400, 20), "C: 연결 정보 설정");
        GUI.Label(new Rect(10, 200, 400, 20), "X: Rich Presence 클리어");
        GUI.Label(new Rect(10, 220, 400, 20), "F: 친구 목록 표시");
        GUI.Label(new Rect(10, 240, 400, 20), "마우스 휠: 레벨 조정");
    }
}
