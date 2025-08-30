using UnityEngine;
using Minimoo.SteamWork;

public class AchievementExample : MonoBehaviour
{
    private int score = 0;
    private string[] sampleAchievements = {
        "ACHIEVEMENT_FIRST_GAME",
        "ACHIEVEMENT_SCORE_100",
        "ACHIEVEMENT_SCORE_500",
        "ACHIEVEMENT_PERFECT_GAME"
    };

    private void Start()
    {
        Debug.Log("Achievement 예제 시작!");
        ShowAllAchievements();
    }

    private void Update()
    {
        // 스페이스바로 점수 증가
        if (Input.GetKeyDown(KeyCode.Space))
        {
            IncreaseScore();
        }

        // 1-4 키로 업적 잠금 해제
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UnlockAchievement(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UnlockAchievement(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            UnlockAchievement(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            UnlockAchievement(3);
        }

        // R 키로 모든 업적 리셋 (주의: 실제 게임에서는 사용하지 말 것)
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetAllAchievements();
        }

        // A 키로 모든 업적 정보 표시
        if (Input.GetKeyDown(KeyCode.A))
        {
            ShowAllAchievements();
        }
    }

    private void IncreaseScore()
    {
        score += 10;
        Debug.Log($"점수 증가: {score}");

        // 점수 기반 업적 체크
        if (score >= 100 && !SteamAchievements.IsAchievementUnlocked(sampleAchievements[1]))
        {
            SteamAchievements.UnlockAchievement(sampleAchievements[1]);
        }

        if (score >= 500 && !SteamAchievements.IsAchievementUnlocked(sampleAchievements[2]))
        {
            SteamAchievements.UnlockAchievement(sampleAchievements[2]);
        }
    }

    private void UnlockAchievement(int index)
    {
        if (index >= 0 && index < sampleAchievements.Length)
        {
            string achievementId = sampleAchievements[index];
            if (SteamAchievements.UnlockAchievement(achievementId))
            {
                Debug.Log($"업적 잠금 해제 성공: {achievementId}");
            }
            else
            {
                Debug.LogError($"업적 잠금 해제 실패: {achievementId}");
            }
        }
    }

    private void ShowAllAchievements()
    {
        var allAchievements = SteamAchievements.GetAllAchievements();
        var unlockedAchievements = SteamAchievements.GetUnlockedAchievements();
        var lockedAchievements = SteamAchievements.GetLockedAchievements();

        Debug.Log($"총 업적 수: {allAchievements.Count}");
        Debug.Log($"잠금 해제된 업적: {unlockedAchievements.Count}");
        Debug.Log($"잠금된 업적: {lockedAchievements.Count}");

        foreach (var achievementId in allAchievements)
        {
            string name = SteamAchievements.GetAchievementDisplayName(achievementId);
            string desc = SteamAchievements.GetAchievementDescription(achievementId);
            bool isUnlocked = SteamAchievements.IsAchievementUnlocked(achievementId);

            Debug.Log($"{achievementId}: {name} - {(isUnlocked ? "잠금 해제됨" : "잠금됨")}");
            Debug.Log($"  설명: {desc}");
        }
    }

    private void ResetAllAchievements()
    {
        if (SteamAchievements.ResetAllAchievements())
        {
            Debug.Log("모든 업적 리셋됨");
            score = 0;
        }
        else
        {
            Debug.LogError("업적 리셋 실패");
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 400, 20), $"현재 점수: {score}");

        GUI.Label(new Rect(10, 40, 400, 20), "키 설명:");
        GUI.Label(new Rect(10, 60, 400, 20), "스페이스바: 점수 증가 (10점)");
        GUI.Label(new Rect(10, 80, 400, 20), "1-4: 해당 업적 잠금 해제");
        GUI.Label(new Rect(10, 100, 400, 20), "A: 모든 업적 정보 표시");
        GUI.Label(new Rect(10, 120, 400, 20), "R: 모든 업적 리셋 (주의!)");

        int yPos = 150;
        foreach (var achievementId in sampleAchievements)
        {
            bool isUnlocked = SteamAchievements.IsAchievementUnlocked(achievementId);
            string name = SteamAchievements.GetAchievementDisplayName(achievementId);

            GUI.color = isUnlocked ? Color.green : Color.white;
            GUI.Label(new Rect(10, yPos, 400, 20), $"{achievementId}: {name} - {(isUnlocked ? "✓" : "✗")}");
            yPos += 20;
        }
        GUI.color = Color.white;
    }
}
