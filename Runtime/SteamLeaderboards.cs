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
    public static class SteamLeaderboards
    {
#if UNITY_STANDALONE
        private static bool isInitialized = false;
#endif

        public static void Initialize()
        {
#if UNITY_STANDALONE
            if (!SteamManager.Instance.IsSteamInitialized)
            {
                D.Error("Steam is not initialized. Cannot initialize SteamLeaderboards.");
                return;
            }

            isInitialized = true;
            D.Log("SteamLeaderboards initialized successfully.");
#endif
        }

#if UNITY_STANDALONE
        /// <summary>
        /// 리더보드에 점수를 업로드합니다.
        /// </summary>
        /// <param name="leaderboardName">리더보드 이름</param>
        /// <param name="score">점수</param>
        /// <param name="method">업로드 방식</param>
        /// <returns>업로드 성공 여부</returns>
        public static async UniTask<bool> UploadScore(string leaderboardName, int score, ELeaderboardUploadScoreMethod method = ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest)
        {
            if (!isInitialized)
            {
                D.Error("SteamLeaderboards is not initialized.");
                return false;
            }

            try
            {
                // 리더보드 찾기/생성
                var findLeaderboardCall = SteamUserStats.FindOrCreateLeaderboard(leaderboardName, ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending, ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric);
                var findResult = await WaitForCallResult<LeaderboardFindResult_t>(findLeaderboardCall);

                if (findResult.m_bLeaderboardFound == 0)
                {
                    D.Error($"Failed to find or create leaderboard: {leaderboardName}");
                    return false;
                }

                SteamLeaderboard_t leaderboard = findResult.m_hSteamLeaderboard;

                // 점수 업로드
                var uploadCall = SteamUserStats.UploadLeaderboardScore(leaderboard, method, score, null, 0);
                var uploadResult = await WaitForCallResult<LeaderboardScoreUploaded_t>(uploadCall);

                if (uploadResult.m_bSuccess == 1)
                {
                    D.Log($"Score uploaded to leaderboard '{leaderboardName}': {score} (Rank: {uploadResult.m_nGlobalRankNew})");
                    return true;
                }
                else
                {
                    D.Error($"Failed to upload score to leaderboard '{leaderboardName}'");
                    return false;
                }
            }
            catch (Exception e)
            {
                D.Error($"Exception while uploading score: {e.Message}");
                return false;
            }
        }
#endif

#if UNITY_STANDALONE
        /// <summary>
        /// 리더보드에서 특정 범위의 엔트리를 가져옵니다.
        /// </summary>
        /// <param name="leaderboardName">리더보드 이름</param>
        /// <param name="start">시작 순위 (1부터 시작)</param>
        /// <param name="end">끝 순위</param>
        /// <returns>리더보드 엔트리 목록</returns>
        public static async UniTask<List<LeaderboardEntry>> GetLeaderboardEntries(string leaderboardName, int start = 1, int end = 10)
        {
            var entries = new List<LeaderboardEntry>();

            if (!isInitialized)
            {
                D.Error("SteamLeaderboards is not initialized.");
                return entries;
            }

            try
            {
                // 리더보드 찾기
                var findLeaderboardCall = SteamUserStats.FindLeaderboard(leaderboardName);
                var findResult = await WaitForCallResult<LeaderboardFindResult_t>(findLeaderboardCall);

                if (findResult.m_bLeaderboardFound == 0)
                {
                    D.Error($"Leaderboard not found: {leaderboardName}");
                    return entries;
                }

                SteamLeaderboard_t leaderboard = findResult.m_hSteamLeaderboard;

                // 엔트리 다운로드
                var downloadCall = SteamUserStats.DownloadLeaderboardEntries(leaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, start - 1, end - 1);
                var downloadResult = await WaitForCallResult<LeaderboardScoresDownloaded_t>(downloadCall);

                if (downloadResult.m_cEntryCount > 0)
                {
                    for (int i = 0; i < downloadResult.m_cEntryCount; i++)
                    {
                        LeaderboardEntry_t entry;
                        if (SteamUserStats.GetDownloadedLeaderboardEntry(downloadResult.m_hSteamLeaderboardEntries, i, out entry, null, 0))
                        {
                            var leaderboardEntry = new LeaderboardEntry
                            {
                                Rank = entry.m_nGlobalRank,
                                Score = entry.m_nScore,
                                SteamId = entry.m_steamIDUser,
                                UserName = SteamFriends.GetFriendPersonaName(entry.m_steamIDUser)
                            };
                            entries.Add(leaderboardEntry);
                        }
                    }

                    D.Log($"Downloaded {entries.Count} entries from leaderboard '{leaderboardName}'");
                }
                else
                {
                    D.Log($"No entries found in leaderboard '{leaderboardName}'");
                }
            }
            catch (Exception e)
            {
                D.Error($"Exception while getting leaderboard entries: {e.Message}");
            }

            return entries;
        }
#endif

#if UNITY_STANDALONE
        /// <summary>
        /// 친구들의 리더보드 엔트리를 가져옵니다.
        /// </summary>
        /// <param name="leaderboardName">리더보드 이름</param>
        /// <returns>친구들의 리더보드 엔트리 목록</returns>
        public static async UniTask<List<LeaderboardEntry>> GetFriendsLeaderboardEntries(string leaderboardName)
        {
            var entries = new List<LeaderboardEntry>();

            if (!isInitialized)
            {
                D.Error("SteamLeaderboards is not initialized.");
                return entries;
            }

            try
            {
                // 리더보드 찾기
                var findLeaderboardCall = SteamUserStats.FindLeaderboard(leaderboardName);
                var findResult = await WaitForCallResult<LeaderboardFindResult_t>(findLeaderboardCall);

                if (findResult.m_bLeaderboardFound == 0)
                {
                    D.Error($"Leaderboard not found: {leaderboardName}");
                    return entries;
                }

                SteamLeaderboard_t leaderboard = findResult.m_hSteamLeaderboard;

                // 친구들 엔트리 다운로드
                var downloadCall = SteamUserStats.DownloadLeaderboardEntries(leaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends, 0, 0);
                var downloadResult = await WaitForCallResult<LeaderboardScoresDownloaded_t>(downloadCall);

                if (downloadResult.m_cEntryCount > 0)
                {
                    for (int i = 0; i < downloadResult.m_cEntryCount; i++)
                    {
                        LeaderboardEntry_t entry;
                        if (SteamUserStats.GetDownloadedLeaderboardEntry(downloadResult.m_hSteamLeaderboardEntries, i, out entry, null, 0))
                        {
                            var leaderboardEntry = new LeaderboardEntry
                            {
                                Rank = entry.m_nGlobalRank,
                                Score = entry.m_nScore,
                                SteamId = entry.m_steamIDUser,
                                UserName = SteamFriends.GetFriendPersonaName(entry.m_steamIDUser)
                            };
                            entries.Add(leaderboardEntry);
                        }
                    }

                    D.Log($"Downloaded {entries.Count} friend entries from leaderboard '{leaderboardName}'");
                }
                else
                {
                    D.Log($"No friend entries found in leaderboard '{leaderboardName}'");
                }
            }
            catch (Exception e)
            {
                D.Error($"Exception while getting friends leaderboard entries: {e.Message}");
            }

            return entries;
        }
#endif

#if UNITY_STANDALONE
        /// <summary>
        /// 현재 사용자의 리더보드 엔트리를 가져옵니다.
        /// </summary>
        /// <param name="leaderboardName">리더보드 이름</param>
        /// <returns>사용자의 리더보드 엔트리</returns>
        public static async UniTask<LeaderboardEntry?> GetUserLeaderboardEntry(string leaderboardName)
        {
            if (!isInitialized)
            {
                D.Error("SteamLeaderboards is not initialized.");
                return null;
            }

            try
            {
                // 리더보드 찾기
                var findLeaderboardCall = SteamUserStats.FindLeaderboard(leaderboardName);
                var findResult = await WaitForCallResult<LeaderboardFindResult_t>(findLeaderboardCall);

                if (findResult.m_bLeaderboardFound == 0)
                {
                    D.Error($"Leaderboard not found: {leaderboardName}");
                    return null;
                }

                SteamLeaderboard_t leaderboard = findResult.m_hSteamLeaderboard;

                // 사용자 엔트리 다운로드 (현재 사용자 주변 범위로 다운로드)
                var downloadCall = SteamUserStats.DownloadLeaderboardEntries(leaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser, 0, 0);
                var downloadResult = await WaitForCallResult<LeaderboardScoresDownloaded_t>(downloadCall);

                if (downloadResult.m_cEntryCount > 0)
                {
                    LeaderboardEntry_t entry;
                    if (SteamUserStats.GetDownloadedLeaderboardEntry(downloadResult.m_hSteamLeaderboardEntries, 0, out entry, null, 0))
                    {
                        var leaderboardEntry = new LeaderboardEntry
                        {
                            Rank = entry.m_nGlobalRank,
                            Score = entry.m_nScore,
                            SteamId = entry.m_steamIDUser,
                            UserName = SteamFriends.GetFriendPersonaName(entry.m_steamIDUser)
                        };

                        D.Log($"Retrieved user entry from leaderboard '{leaderboardName}': Rank {leaderboardEntry.Rank}, Score {leaderboardEntry.Score}");
                        return leaderboardEntry;
                    }
                }

                D.Log($"User entry not found in leaderboard '{leaderboardName}'");
                return null;
            }
            catch (Exception e)
            {
                D.Error($"Exception while getting user leaderboard entry: {e.Message}");
                return null;
            }
        }
#endif

#if UNITY_STANDALONE
        /// <summary>
        /// 리더보드의 총 엔트리 수를 가져옵니다.
        /// </summary>
        /// <param name="leaderboardName">리더보드 이름</param>
        /// <returns>총 엔트리 수</returns>
        public static async UniTask<int> GetLeaderboardEntryCount(string leaderboardName)
        {
            if (!isInitialized)
            {
                D.Error("SteamLeaderboards is not initialized.");
                return 0;
            }

            try
            {
                // 리더보드 찾기
                var findLeaderboardCall = SteamUserStats.FindLeaderboard(leaderboardName);
                var findResult = await WaitForCallResult<LeaderboardFindResult_t>(findLeaderboardCall);

                if (findResult.m_bLeaderboardFound == 0)
                {
                    D.Error($"Leaderboard not found: {leaderboardName}");
                    return 0;
                }

                SteamLeaderboard_t leaderboard = findResult.m_hSteamLeaderboard;
                return SteamUserStats.GetLeaderboardEntryCount(leaderboard);
            }
            catch (Exception e)
            {
                D.Error($"Exception while getting leaderboard entry count: {e.Message}");
                return 0;
            }
        }
#endif

#if UNITY_STANDALONE
        /// <summary>
        /// 리더보드 정보를 가져옵니다.
        /// </summary>
        /// <param name="leaderboardName">리더보드 이름</param>
        /// <returns>리더보드 정보</returns>
        public static async UniTask<LeaderboardInfo?> GetLeaderboardInfo(string leaderboardName)
        {
            if (!isInitialized)
            {
                D.Error("SteamLeaderboards is not initialized.");
                return null;
            }

            try
            {
                // 리더보드 찾기
                var findLeaderboardCall = SteamUserStats.FindLeaderboard(leaderboardName);
                var findResult = await WaitForCallResult<LeaderboardFindResult_t>(findLeaderboardCall);

                if (findResult.m_bLeaderboardFound == 0)
                {
                    D.Error($"Leaderboard not found: {leaderboardName}");
                    return null;
                }

                SteamLeaderboard_t leaderboard = findResult.m_hSteamLeaderboard;

                int entryCount = SteamUserStats.GetLeaderboardEntryCount(leaderboard);
                ELeaderboardSortMethod sortMethod;
                ELeaderboardDisplayType displayType;
                sortMethod = SteamUserStats.GetLeaderboardSortMethod(leaderboard);
                displayType = SteamUserStats.GetLeaderboardDisplayType(leaderboard);

                var info = new LeaderboardInfo
                {
                    Name = leaderboardName,
                    EntryCount = entryCount,
                    SortMethod = sortMethod,
                    DisplayType = displayType
                };

                D.Log($"Retrieved leaderboard info for '{leaderboardName}': {entryCount} entries");
                return info;
            }
            catch (Exception e)
            {
                D.Error($"Exception while getting leaderboard info: {e.Message}");
                return null;
            }
        }
#else
        /// <summary>
        /// 리더보드에 점수를 업로드합니다. (Steam 미사용 시)
        /// </summary>
        /// <param name="leaderboardName">리더보드 이름</param>
        /// <param name="score">점수</param>
        /// <param name="method">업로드 방식</param>
        /// <returns>항상 false 반환</returns>
        public static async UniTask<bool> UploadScore(string leaderboardName, int score, int method = 0)
        {
            D.Log($"Leaderboard upload not available on this platform: {leaderboardName}, score: {score}");
            return false;
        }

        /// <summary>
        /// 리더보드에서 특정 범위의 엔트리를 가져옵니다. (Steam 미사용 시)
        /// </summary>
        /// <param name="leaderboardName">리더보드 이름</param>
        /// <param name="start">시작 순위 (1부터 시작)</param>
        /// <param name="end">끝 순위</param>
        /// <returns>빈 리스트 반환</returns>
        public static async UniTask<List<LeaderboardEntry>> GetLeaderboardEntries(string leaderboardName, int start = 1, int end = 10)
        {
            D.Log($"Leaderboard entries not available on this platform: {leaderboardName}");
            return new List<LeaderboardEntry>();
        }

        /// <summary>
        /// 친구들의 리더보드 엔트리를 가져옵니다. (Steam 미사용 시)
        /// </summary>
        /// <param name="leaderboardName">리더보드 이름</param>
        /// <returns>빈 리스트 반환</returns>
        public static async UniTask<List<LeaderboardEntry>> GetFriendsLeaderboardEntries(string leaderboardName)
        {
            D.Log($"Friends leaderboard entries not available on this platform: {leaderboardName}");
            return new List<LeaderboardEntry>();
        }

        /// <summary>
        /// 현재 사용자의 리더보드 엔트리를 가져옵니다. (Steam 미사용 시)
        /// </summary>
        /// <param name="leaderboardName">리더보드 이름</param>
        /// <returns>null 반환</returns>
        public static async UniTask<LeaderboardEntry?> GetUserLeaderboardEntry(string leaderboardName)
        {
            D.Log($"User leaderboard entry not available on this platform: {leaderboardName}");
            return null;
        }

        /// <summary>
        /// 리더보드의 총 엔트리 수를 가져옵니다. (Steam 미사용 시)
        /// </summary>
        /// <param name="leaderboardName">리더보드 이름</param>
        /// <returns>0 반환</returns>
        public static async UniTask<int> GetLeaderboardEntryCount(string leaderboardName)
        {
            D.Log($"Leaderboard entry count not available on this platform: {leaderboardName}");
            return 0;
        }

        /// <summary>
        /// 리더보드 정보를 가져옵니다. (Steam 미사용 시)
        /// </summary>
        /// <param name="leaderboardName">리더보드 이름</param>
        /// <returns>null 반환</returns>
        public static async UniTask<LeaderboardInfo?> GetLeaderboardInfo(string leaderboardName)
        {
            D.Log($"Leaderboard info not available on this platform: {leaderboardName}");
            return null;
        }
#endif

#if UNITY_STANDALONE
        /// <summary>
        /// Steam API 호출 결과를 기다리는 헬퍼 메소드
        /// </summary>
        private static async UniTask<T> WaitForCallResult<T>(SteamAPICall_t call) where T : struct
        {
            var completionSource = new UniTaskCompletionSource<T>();

            CallResult<T> callResult = new CallResult<T>((result, failure) =>
            {
                if (failure)
                {
                    D.Error($"Steam API call failed for {typeof(T).Name}");
                    completionSource.TrySetException(new Exception("Steam API call failed"));
                }
                else
                {
                    completionSource.TrySetResult(result);
                }
            });

            callResult.Set(call);

            return await completionSource.Task;
        }
#endif
    }

    /// <summary>
    /// 리더보드 엔트리 정보를 담는 구조체
    /// </summary>
    public struct LeaderboardEntry
    {
#if UNITY_STANDALONE
        public int Rank;
        public int Score;
        public CSteamID SteamId;
        public string UserName;
#else
        public int Rank;
        public int Score;
        public ulong SteamId; // Fallback for non-Steam platforms
        public string UserName;
#endif
    }

    /// <summary>
    /// 리더보드 정보를 담는 구조체
    /// </summary>
    public struct LeaderboardInfo
    {
        public string Name;
        public int EntryCount;
#if UNITY_STANDALONE
        public ELeaderboardSortMethod SortMethod;
        public ELeaderboardDisplayType DisplayType;
#else
        public int SortMethod; // Fallback for non-Steam platforms
        public int DisplayType; // Fallback for non-Steam platforms
#endif
    }
}
