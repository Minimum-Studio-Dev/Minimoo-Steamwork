using UnityEngine;
using Minimoo.SteamWork;
using Minimoo;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class LeaderboardExample : MonoBehaviour
{
    private int currentScore = 0;
    private string[] sampleLeaderboards = {
        "HIGH_SCORE",
        "FASTEST_TIME",
        "MOST_KILLS",
        "TOTAL_PLAYTIME"
    };

    private void Start()
    {
        D.Log("리더보드 예제 시작!");
        ShowLeaderboardInfo();
    }

    private async void Update()
    {
        // 스페이스바로 점수 증가
        if (Input.GetKeyDown(KeyCode.Space))
        {
            IncreaseScore();
        }

        // 1-4 키로 리더보드에 점수 업로드
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            await UploadScoreToLeaderboard(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            await UploadScoreToLeaderboard(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            await UploadScoreToLeaderboard(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            await UploadScoreToLeaderboard(3);
        }

        // G 키로 글로벌 리더보드 조회
        if (Input.GetKeyDown(KeyCode.G))
        {
            await ShowGlobalLeaderboard(0);
        }

        // F 키로 친구 리더보드 조회
        if (Input.GetKeyDown(KeyCode.F))
        {
            await ShowFriendsLeaderboard(0);
        }

        // U 키로 사용자 자신의 순위 조회
        if (Input.GetKeyDown(KeyCode.U))
        {
            await ShowUserEntry(0);
        }

        // I 키로 리더보드 정보 조회
        if (Input.GetKeyDown(KeyCode.I))
        {
            await ShowLeaderboardInfo();
        }
    }

    private void IncreaseScore()
    {
        currentScore += Random.Range(1, 100);
        D.Log($"점수 증가: {currentScore}");
    }

    private async UniTask UploadScoreToLeaderboard(int index)
    {
        if (index < 0 || index >= sampleLeaderboards.Length) return;

        string leaderboardName = sampleLeaderboards[index];
        D.Log($"리더보드 '{leaderboardName}'에 점수 업로드 중: {currentScore}");

        bool success = await SteamLeaderboards.UploadScore(leaderboardName, currentScore);
        if (success)
        {
            D.Log($"점수 업로드 성공: {leaderboardName} - {currentScore}");
        }
        else
        {
            D.Error($"점수 업로드 실패: {leaderboardName}");
        }
    }

    private async UniTask ShowGlobalLeaderboard(int index)
    {
        if (index < 0 || index >= sampleLeaderboards.Length) return;

        string leaderboardName = sampleLeaderboards[index];
        D.Log($"글로벌 리더보드 조회 중: {leaderboardName}");

        var entries = await SteamLeaderboards.GetLeaderboardEntries(leaderboardName, 1, 10);

        if (entries.Count > 0)
        {
            D.Log($"=== {leaderboardName} 리더보드 (상위 10위) ===");
            foreach (var entry in entries)
            {
                D.Log($"{entry.Rank}위: {entry.UserName} - {entry.Score}점");
            }
        }
        else
        {
            D.Log($"{leaderboardName} 리더보드에 엔트리가 없습니다.");
        }
    }

    private async UniTask ShowFriendsLeaderboard(int index)
    {
        if (index < 0 || index >= sampleLeaderboards.Length) return;

        string leaderboardName = sampleLeaderboards[index];
        D.Log($"친구 리더보드 조회 중: {leaderboardName}");

        var entries = await SteamLeaderboards.GetFriendsLeaderboardEntries(leaderboardName);

        if (entries.Count > 0)
        {
            D.Log($"=== {leaderboardName} 친구 리더보드 ===");
            foreach (var entry in entries)
            {
                D.Log($"{entry.Rank}위: {entry.UserName} - {entry.Score}점");
            }
        }
        else
        {
            D.Log($"{leaderboardName} 친구 리더보드에 엔트리가 없습니다.");
        }
    }

    private async UniTask ShowUserEntry(int index)
    {
        if (index < 0 || index >= sampleLeaderboards.Length) return;

        string leaderboardName = sampleLeaderboards[index];
        D.Log($"사용자 순위 조회 중: {leaderboardName}");

        var userEntry = await SteamLeaderboards.GetUserLeaderboardEntry(leaderboardName);

        if (userEntry.HasValue)
        {
            D.Log($"내 순위: {userEntry.Value.Rank}위 - {userEntry.Value.Score}점");
        }
        else
        {
            D.Log($"{leaderboardName} 리더보드에 내 점수가 없습니다.");
        }
    }

    private async UniTask ShowLeaderboardInfo()
    {
        D.Log("리더보드 정보 조회 중...");

        foreach (string leaderboardName in sampleLeaderboards)
        {
            var info = await SteamLeaderboards.GetLeaderboardInfo(leaderboardName);
            if (info.HasValue)
            {
                D.Log($"{leaderboardName}: {info.Value.EntryCount}개 엔트리");
            }
            else
            {
                D.Log($"{leaderboardName}: 리더보드를 찾을 수 없음");
            }
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 400, 20), $"현재 점수: {currentScore}");

        GUI.Label(new Rect(10, 40, 400, 20), "키 설명:");
        GUI.Label(new Rect(10, 60, 400, 20), "스페이스바: 점수 증가 (1-100점 랜덤)");
        GUI.Label(new Rect(10, 80, 400, 20), "1-4: 해당 리더보드에 점수 업로드");
        GUI.Label(new Rect(10, 100, 400, 20), "G: 글로벌 리더보드 조회 (상위 10위)");
        GUI.Label(new Rect(10, 120, 400, 20), "F: 친구 리더보드 조회");
        GUI.Label(new Rect(10, 140, 400, 20), "U: 내 순위 조회");
        GUI.Label(new Rect(10, 160, 400, 20), "I: 리더보드 정보 조회");

        int yPos = 190;
        foreach (var leaderboardName in sampleLeaderboards)
        {
            GUI.Label(new Rect(10, yPos, 400, 20), $"{leaderboardName}");
            yPos += 20;
        }
    }
}
