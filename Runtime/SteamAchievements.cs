using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Minimoo.SteamWork
{
    public static class SteamAchievements
    {
        private static bool isInitialized = false;
        private static Dictionary<string, bool> achievementCache = new Dictionary<string, bool>();

        public static void Initialize()
        {
            if (!SteamManager.Instance.IsSteamInitialized)
            {
                Debug.LogError("Steam is not initialized. Cannot initialize SteamAchievements.");
                return;
            }

            isInitialized = true;
            CacheAchievements();
            Debug.Log("SteamAchievements initialized successfully.");
        }

        private static void CacheAchievements()
        {
            achievementCache.Clear();

            try
            {
                uint achievementCount = SteamUserStats.GetNumAchievements();

                for (uint i = 0; i < achievementCount; i++)
                {
                    string achievementId = SteamUserStats.GetAchievementName(i);
                    if (!string.IsNullOrEmpty(achievementId))
                    {
                        bool achieved;
                        SteamUserStats.GetAchievement(achievementId, out achieved);
                        achievementCache[achievementId] = achieved;
                    }
                }

                Debug.Log($"Cached {achievementCache.Count} achievements");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to cache achievements: {e.Message}");
            }
        }

        /// <summary>
        /// 업적을 잠금 해제합니다.
        /// </summary>
        /// <param name="achievementId">업적 ID</param>
        /// <returns>업적 해제 성공 여부</returns>
        public static bool UnlockAchievement(string achievementId)
        {
            if (!isInitialized) return false;

            try
            {
                bool result = SteamUserStats.SetAchievement(achievementId);
                if (result)
                {
                    SteamUserStats.StoreStats();
                    Debug.Log($"Achievement unlocked: {achievementId}");

                    // 캐시 업데이트
                    achievementCache[achievementId] = true;
                }
                else
                {
                    Debug.LogError($"Failed to unlock achievement: {achievementId}");
                }

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while unlocking achievement: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 업적을 다시 잠급니다.
        /// </summary>
        /// <param name="achievementId">업적 ID</param>
        /// <returns>업적 잠금 성공 여부</returns>
        public static bool LockAchievement(string achievementId)
        {
            if (!isInitialized) return false;

            try
            {
                bool result = SteamUserStats.ClearAchievement(achievementId);
                if (result)
                {
                    SteamUserStats.StoreStats();
                    Debug.Log($"Achievement locked: {achievementId}");

                    // 캐시 업데이트
                    achievementCache[achievementId] = true;
                }
                else
                {
                    Debug.LogError($"Failed to lock achievement: {achievementId}");
                }

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while locking achievement: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 업적이 잠금 해제되었는지 확인합니다.
        /// </summary>
        /// <param name="achievementId">업적 ID</param>
        /// <returns>업적 잠금 해제 상태</returns>
        public static bool IsAchievementUnlocked(string achievementId)
        {
            if (!isInitialized) return false;

            try
            {
                if (achievementCache.TryGetValue(achievementId, out bool achieved))
                {
                    return achieved;
                }

                // 캐시에 없는 경우 직접 조회
                bool newAchieved;
                SteamUserStats.GetAchievement(achievementId, out newAchieved);
                achievementCache[achievementId] = newAchieved;
                return newAchieved;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while checking achievement: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 업적의 표시 이름을 가져옵니다.
        /// </summary>
        /// <param name="achievementId">업적 ID</param>
        /// <returns>업적 표시 이름</returns>
        public static string GetAchievementDisplayName(string achievementId)
        {
            if (!isInitialized) return string.Empty;

            try
            {
                return SteamUserStats.GetAchievementDisplayAttribute(achievementId, "name");
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while getting achievement name: {e.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 업적의 설명을 가져옵니다.
        /// </summary>
        /// <param name="achievementId">업적 ID</param>
        /// <returns>업적 설명</returns>
        public static string GetAchievementDescription(string achievementId)
        {
            if (!isInitialized) return string.Empty;

            try
            {
                return SteamUserStats.GetAchievementDisplayAttribute(achievementId, "desc");
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while getting achievement description: {e.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 모든 업적 목록을 가져옵니다.
        /// </summary>
        /// <returns>업적 ID 목록</returns>
        public static List<string> GetAllAchievements()
        {
            List<string> achievements = new List<string>();

            if (!isInitialized) return achievements;

            try
            {
                achievements.AddRange(achievementCache.Keys);
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while getting all achievements: {e.Message}");
            }

            return achievements;
        }

        /// <summary>
        /// 잠금 해제된 업적 목록을 가져옵니다.
        /// </summary>
        /// <returns>잠금 해제된 업적 ID 목록</returns>
        public static List<string> GetUnlockedAchievements()
        {
            List<string> unlockedAchievements = new List<string>();

            if (!isInitialized) return unlockedAchievements;

            try
            {
                foreach (var kvp in achievementCache)
                {
                    if (kvp.Value)
                    {
                        unlockedAchievements.Add(kvp.Key);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while getting unlocked achievements: {e.Message}");
            }

            return unlockedAchievements;
        }

        /// <summary>
        /// 잠금된 업적 목록을 가져옵니다.
        /// </summary>
        /// <returns>잠금된 업적 ID 목록</returns>
        public static List<string> GetLockedAchievements()
        {
            List<string> lockedAchievements = new List<string>();

            if (!isInitialized) return lockedAchievements;

            try
            {
                foreach (var kvp in achievementCache)
                {
                    if (!kvp.Value)
                    {
                        lockedAchievements.Add(kvp.Key);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while getting locked achievements: {e.Message}");
            }

            return lockedAchievements;
        }

        /// <summary>
        /// 업적 진행도를 설정합니다 (진행형 업적용).
        /// </summary>
        /// <param name="statName">통계 이름</param>
        /// <param name="value">진행도 값</param>
        /// <returns>설정 성공 여부</returns>
        public static bool SetAchievementProgress(string statName, int value)
        {
            if (!isInitialized) return false;

            try
            {
                bool result = SteamUserStats.SetStat(statName, value);
                if (result)
                {
                    SteamUserStats.StoreStats();
                    Debug.Log($"Achievement progress set: {statName} = {value}");
                }
                else
                {
                    Debug.LogError($"Failed to set achievement progress: {statName}");
                }

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while setting achievement progress: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 업적 통계 값을 가져옵니다.
        /// </summary>
        /// <param name="statName">통계 이름</param>
        /// <returns>통계 값</returns>
        public static int GetAchievementStat(string statName)
        {
            if (!isInitialized) return 0;

            try
            {
                int value;
                SteamUserStats.GetStat(statName, out value);
                return value;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while getting achievement stat: {e.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 업적 데이터를 강제로 Steam 서버에 동기화합니다.
        /// </summary>
        /// <returns>동기화 성공 여부</returns>
        public static bool ForceSyncAchievements()
        {
            if (!isInitialized) return false;

            try
            {
                bool result = SteamUserStats.StoreStats();
                if (result)
                {
                    CacheAchievements(); // 캐시 새로고침
                    Debug.Log("Achievements synced successfully");
                }
                else
                {
                    Debug.LogError("Failed to sync achievements");
                }

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while syncing achievements: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 업적 진행도를 증가시킵니다.
        /// </summary>
        /// <param name="statName">통계 이름</param>
        /// <param name="increment">증가량</param>
        /// <returns>새로운 값</returns>
        public static int IncrementAchievementProgress(string statName, int increment)
        {
            if (!isInitialized) return 0;

            try
            {
                int currentValue = GetAchievementStat(statName);
                int newValue = currentValue + increment;

                if (SetAchievementProgress(statName, newValue))
                {
                    return newValue;
                }

                return currentValue;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while incrementing achievement progress: {e.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 모든 업적 통계를 리셋합니다 (주의: 실제 게임에서는 사용하지 말 것).
        /// </summary>
        /// <returns>리셋 성공 여부</returns>
        public static bool ResetAllAchievements()
        {
            if (!isInitialized) return false;

            try
            {
                SteamUserStats.ResetAllStats(true);
                SteamUserStats.StoreStats();
                CacheAchievements();
                Debug.Log("All achievements reset");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while resetting achievements: {e.Message}");
                return false;
            }
        }
    }
}
