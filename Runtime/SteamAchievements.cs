using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Minimoo;

#if UNITY_STANDALONE
using Steamworks;
#endif

namespace Minimoo.SteamWork
{
    public static class SteamAchievements
    {
#if UNITY_STANDALONE
        private static bool isInitialized = false;
        private static Dictionary<string, bool> achievementCache = new Dictionary<string, bool>();
#endif

        public static void Initialize()
        {
#if UNITY_STANDALONE
            if (!SteamManager.Instance.IsSteamInitialized)
            {
                D.Error("Steam is not initialized. Cannot initialize SteamAchievements.");
                return;
            }

            isInitialized = true;
            CacheAchievements();
            D.Log("SteamAchievements initialized successfully.");
#endif
        }

#if UNITY_STANDALONE
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

                D.Log($"Cached {achievementCache.Count} achievements");
            }
            catch (Exception e)
            {
                D.Error($"Failed to cache achievements: {e.Message}");
            }
        }
#endif

        /// <summary>
        /// 업적을 잠금 해제합니다.
        /// </summary>
        /// <param name="achievementId">업적 ID</param>
        /// <returns>업적 해제 성공 여부</returns>
        public static bool UnlockAchievement(string achievementId)
        {
#if UNITY_STANDALONE
            if (!isInitialized) return false;

            try
            {
                bool result = SteamUserStats.SetAchievement(achievementId);
                if (result)
                {
                    SteamUserStats.StoreStats();
                    D.Log($"Achievement unlocked: {achievementId}");

                    // 캐시 업데이트
                    achievementCache[achievementId] = true;
                }
                else
                {
                    D.Error($"Failed to unlock achievement: {achievementId}");
                }

                return result;
            }
            catch (Exception e)
            {
                D.Error($"Exception while unlocking achievement: {e.Message}");
                return false;
            }
#else
            return false;
#endif
        }

        /// <summary>
        /// 업적을 다시 잠급니다.
        /// </summary>
        /// <param name="achievementId">업적 ID</param>
        /// <returns>업적 잠금 성공 여부</returns>
        public static bool LockAchievement(string achievementId)
        {
#if UNITY_STANDALONE
            if (!isInitialized) return false;

            try
            {
                bool result = SteamUserStats.ClearAchievement(achievementId);
                if (result)
                {
                    SteamUserStats.StoreStats();
                    D.Log($"Achievement locked: {achievementId}");

                    // 캐시 업데이트
                    achievementCache[achievementId] = false;
                }
                else
                {
                    D.Error($"Failed to lock achievement: {achievementId}");
                }

                return result;
            }
            catch (Exception e)
            {
                D.Error($"Exception while locking achievement: {e.Message}");
                return false;
            }
#else
            return false;
#endif
        }

        /// <summary>
        /// 업적이 잠금 해제되었는지 확인합니다.
        /// </summary>
        /// <param name="achievementId">업적 ID</param>
        /// <returns>업적 잠금 해제 상태</returns>
        public static bool IsAchievementUnlocked(string achievementId)
        {
#if UNITY_STANDALONE
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
                D.Error($"Exception while checking achievement: {e.Message}");
                return false;
            }
#else
            return false;
#endif
        }

        /// <summary>
        /// 업적의 표시 이름을 가져옵니다.
        /// </summary>
        /// <param name="achievementId">업적 ID</param>
        /// <returns>업적 표시 이름</returns>
        public static string GetAchievementDisplayName(string achievementId)
        {
#if UNITY_STANDALONE
            if (!isInitialized) return string.Empty;

            try
            {
                return SteamUserStats.GetAchievementDisplayAttribute(achievementId, "name");
            }
            catch (Exception e)
            {
                D.Error($"Exception while getting achievement name: {e.Message}");
                return string.Empty;
            }
#else
            return string.Empty;
#endif
        }

        /// <summary>
        /// 업적의 설명을 가져옵니다.
        /// </summary>
        /// <param name="achievementId">업적 ID</param>
        /// <returns>업적 설명</returns>
        public static string GetAchievementDescription(string achievementId)
        {
#if UNITY_STANDALONE
            if (!isInitialized) return string.Empty;

            try
            {
                return SteamUserStats.GetAchievementDisplayAttribute(achievementId, "desc");
            }
            catch (Exception e)
            {
                D.Error($"Exception while getting achievement description: {e.Message}");
                return string.Empty;
            }
#else
            return string.Empty;
#endif
        }

        /// <summary>
        /// 모든 업적 목록을 가져옵니다.
        /// </summary>
        /// <returns>업적 ID 목록</returns>
        public static List<string> GetAllAchievements()
        {
#if UNITY_STANDALONE
            List<string> achievements = new List<string>();

            if (!isInitialized) return achievements;

            try
            {
                achievements.AddRange(achievementCache.Keys);
            }
            catch (Exception e)
            {
                D.Error($"Exception while getting all achievements: {e.Message}");
            }

            return achievements;
#else
            return new List<string>();
#endif
        }

        /// <summary>
        /// 잠금 해제된 업적 목록을 가져옵니다.
        /// </summary>
        /// <returns>잠금 해제된 업적 ID 목록</returns>
        public static List<string> GetUnlockedAchievements()
        {
#if UNITY_STANDALONE
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
                D.Error($"Exception while getting unlocked achievements: {e.Message}");
            }

            return unlockedAchievements;
#else
            return new List<string>();
#endif
        }

        /// <summary>
        /// 잠금된 업적 목록을 가져옵니다.
        /// </summary>
        /// <returns>잠금된 업적 ID 목록</returns>
        public static List<string> GetLockedAchievements()
        {
#if UNITY_STANDALONE
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
                D.Error($"Exception while getting locked achievements: {e.Message}");
            }

            return lockedAchievements;
#else
            return new List<string>();
#endif
        }

        /// <summary>
        /// 업적 진행도를 설정합니다 (진행형 업적용).
        /// </summary>
        /// <param name="statName">통계 이름</param>
        /// <param name="value">진행도 값</param>
        /// <returns>설정 성공 여부</returns>
        public static bool SetAchievementProgress(string statName, int value)
        {
#if UNITY_STANDALONE
            if (!isInitialized) return false;

            try
            {
                bool result = SteamUserStats.SetStat(statName, value);
                if (result)
                {
                    SteamUserStats.StoreStats();
                    D.Log($"Achievement progress set: {statName} = {value}");
                }
                else
                {
                    D.Error($"Failed to set achievement progress: {statName}");
                }

                return result;
            }
            catch (Exception e)
            {
                D.Error($"Exception while setting achievement progress: {e.Message}");
                return false;
            }
#else
            return false;
#endif
        }

        /// <summary>
        /// 업적 통계 값을 가져옵니다.
        /// </summary>
        /// <param name="statName">통계 이름</param>
        /// <returns>통계 값</returns>
        public static int GetAchievementStat(string statName)
        {
#if UNITY_STANDALONE
            if (!isInitialized) return 0;

            try
            {
                int value;
                SteamUserStats.GetStat(statName, out value);
                return value;
            }
            catch (Exception e)
            {
                D.Error($"Exception while getting achievement stat: {e.Message}");
                return 0;
            }
#else
            return 0;
#endif
        }

        /// <summary>
        /// 업적 데이터를 강제로 Steam 서버에 동기화합니다.
        /// </summary>
        /// <returns>동기화 성공 여부</returns>
        public static bool ForceSyncAchievements()
        {
#if UNITY_STANDALONE
            if (!isInitialized) return false;

            try
            {
                bool result = SteamUserStats.StoreStats();
                if (result)
                {
                    CacheAchievements(); // 캐시 새로고침
                    D.Log("Achievements synced successfully");
                }
                else
                {
                    D.Error("Failed to sync achievements");
                }

                return result;
            }
            catch (Exception e)
            {
                D.Error($"Exception while syncing achievements: {e.Message}");
                return false;
            }
#else
            return false;
#endif
        }

        /// <summary>
        /// 업적 진행도를 증가시킵니다.
        /// </summary>
        /// <param name="statName">통계 이름</param>
        /// <param name="increment">증가량</param>
        /// <returns>새로운 값</returns>
        public static int IncrementAchievementProgress(string statName, int increment)
        {
#if UNITY_STANDALONE
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
                D.Error($"Exception while incrementing achievement progress: {e.Message}");
                return 0;
            }
#else
            return 0;
#endif
        }

        /// <summary>
        /// 모든 업적 통계를 리셋합니다 (주의: 실제 게임에서는 사용하지 말 것).
        /// </summary>
        /// <returns>리셋 성공 여부</returns>
        public static bool ResetAllAchievements()
        {
#if UNITY_STANDALONE
            if (!isInitialized) return false;

            try
            {
                SteamUserStats.ResetAllStats(true);
                SteamUserStats.StoreStats();
                CacheAchievements();
                D.Log("All achievements reset");
                return true;
            }
            catch (Exception e)
            {
                D.Error($"Exception while resetting achievements: {e.Message}");
                return false;
            }
#else
            return false;
#endif
        }
    }
}
