using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Minimoo.SteamWork
{
    public static class SteamActivity
    {
        private static bool isInitialized = false;

        // Rich Presence 키들
        private const string RP_STATUS = "status";
        private const string RP_CONNECT = "connect";
        private const string RP_STEAM_DISPLAY = "steam_display";

        public static void Initialize()
        {
            if (!SteamManager.Instance.IsSteamInitialized)
            {
                Debug.LogError("Steam is not initialized. Cannot initialize SteamActivity.");
                return;
            }

            isInitialized = true;
            Debug.Log("SteamActivity initialized successfully.");
        }

        /// <summary>
        /// 현재 게임 상태를 설정합니다.
        /// </summary>
        /// <param name="status">상태 텍스트 (예: "메뉴", "게임 플레이 중", "레벨 5 클리어")</param>
        public static void SetStatus(string status)
        {
            if (!isInitialized) return;

            try
            {
                SteamFriends.SetRichPresence(RP_STATUS, status);
                Debug.Log($"Steam activity status set: {status}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to set status: {e.Message}");
            }
        }

        /// <summary>
        /// 게임에 참여할 수 있는 연결 정보를 설정합니다.
        /// </summary>
        /// <param name="connectString">연결 문자열 (예: "+connect 127.0.0.1:27015")</param>
        public static void SetConnectInfo(string connectString)
        {
            if (!isInitialized) return;

            try
            {
                SteamFriends.SetRichPresence(RP_CONNECT, connectString);
                Debug.Log($"Steam connect info set: {connectString}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to set connect info: {e.Message}");
            }
        }

        /// <summary>
        /// Steam 표시 텍스트를 설정합니다 (친구 목록에 표시되는 텍스트).
        /// </summary>
        /// <param name="displayText">표시 텍스트</param>
        public static void SetDisplayText(string displayText)
        {
            if (!isInitialized) return;

            try
            {
                SteamFriends.SetRichPresence(RP_STEAM_DISPLAY, displayText);
                Debug.Log($"Steam display text set: {displayText}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to set display text: {e.Message}");
            }
        }

        /// <summary>
        /// 사용자 정의 Rich Presence 키-값 쌍을 설정합니다.
        /// </summary>
        /// <param name="key">키</param>
        /// <param name="value">값</param>
        public static void SetRichPresence(string key, string value)
        {
            if (!isInitialized) return;

            try
            {
                SteamFriends.SetRichPresence(key, value);
                Debug.Log($"Steam rich presence set: {key} = {value}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to set rich presence: {e.Message}");
            }
        }

        /// <summary>
        /// Rich Presence를 초기화합니다 (모든 값 제거).
        /// </summary>
        public static void ClearRichPresence()
        {
            if (!isInitialized) return;

            try
            {
                SteamFriends.ClearRichPresence();
                Debug.Log("Steam rich presence cleared");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to clear rich presence: {e.Message}");
            }
        }

        /// <summary>
        /// 게임 초대장을 보냅니다.
        /// </summary>
        /// <param name="friendCSteamID">친구의 Steam ID</param>
        /// <param name="connectString">연결 문자열</param>
        /// <returns>초대장 전송 성공 여부</returns>
        public static bool InviteFriend(CSteamID friendCSteamID, string connectString = "")
        {
            if (!isInitialized) return false;

            try
            {
                bool result = SteamFriends.InviteUserToGame(friendCSteamID, connectString);
                if (result)
                {
                    Debug.Log($"Game invitation sent to friend: {friendCSteamID}");
                }
                else
                {
                    Debug.LogError($"Failed to send game invitation to friend: {friendCSteamID}");
                }

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while inviting friend: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 친구 목록을 가져옵니다.
        /// </summary>
        /// <returns>친구 Steam ID 목록</returns>
        public static List<CSteamID> GetFriendList()
        {
            List<CSteamID> friends = new List<CSteamID>();

            if (!isInitialized) return friends;

            try
            {
                int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);

                for (int i = 0; i < friendCount; i++)
                {
                    CSteamID friendId = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                    if (friendId.IsValid())
                    {
                        friends.Add(friendId);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while getting friend list: {e.Message}");
            }

            return friends;
        }

        /// <summary>
        /// 친구의 이름을 가져옵니다.
        /// </summary>
        /// <param name="friendCSteamID">친구의 Steam ID</param>
        /// <returns>친구 이름</returns>
        public static string GetFriendName(CSteamID friendCSteamID)
        {
            if (!isInitialized) return string.Empty;

            try
            {
                return SteamFriends.GetFriendPersonaName(friendCSteamID);
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while getting friend name: {e.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 친구의 현재 게임 상태를 가져옵니다.
        /// </summary>
        /// <param name="friendCSteamID">친구의 Steam ID</param>
        /// <returns>게임 상태 텍스트</returns>
        public static string GetFriendGameStatus(CSteamID friendCSteamID)
        {
            if (!isInitialized) return string.Empty;

            try
            {
                FriendGameInfo_t gameInfo;
                SteamFriends.GetFriendGamePlayed(friendCSteamID, out gameInfo);
                if (gameInfo.m_gameID.IsValid())
                {
                    return $"{gameInfo.m_gameID} ({gameInfo.m_unGameIP}:{gameInfo.m_usGamePort})";
                }
                else
                {
                    return "Not in game";
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while getting friend game status: {e.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 친구의 Rich Presence 값을 가져옵니다.
        /// </summary>
        /// <param name="friendCSteamID">친구의 Steam ID</param>
        /// <param name="key">Rich Presence 키</param>
        /// <returns>Rich Presence 값</returns>
        public static string GetFriendRichPresence(CSteamID friendCSteamID, string key)
        {
            if (!isInitialized) return string.Empty;

            try
            {
                return SteamFriends.GetFriendRichPresence(friendCSteamID, key);
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while getting friend rich presence: {e.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 친구의 온라인 상태를 가져옵니다.
        /// </summary>
        /// <param name="friendCSteamID">친구의 Steam ID</param>
        /// <returns>온라인 상태</returns>
        public static EPersonaState GetFriendPersonaState(CSteamID friendCSteamID)
        {
            if (!isInitialized) return EPersonaState.k_EPersonaStateOffline;

            try
            {
                return SteamFriends.GetFriendPersonaState(friendCSteamID);
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while getting friend persona state: {e.Message}");
                return EPersonaState.k_EPersonaStateOffline;
            }
        }

        /// <summary>
        /// 현재 게임의 Activity 정보를 설정합니다.
        /// </summary>
        /// <param name="activityInfo">Activity 정보</param>
        public static void SetGameActivity(ActivityInfo activityInfo)
        {
            if (!isInitialized) return;

            try
            {
                SetStatus(activityInfo.Status);
                SetDisplayText(activityInfo.DisplayText);

                if (!string.IsNullOrEmpty(activityInfo.ConnectString))
                {
                    SetConnectInfo(activityInfo.ConnectString);
                }

                // 추가 Rich Presence 설정
                if (activityInfo.CustomRichPresence != null)
                {
                    foreach (var kvp in activityInfo.CustomRichPresence)
                    {
                        SetRichPresence(kvp.Key, kvp.Value);
                    }
                }

                Debug.Log($"Game activity set: {activityInfo.Status}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to set game activity: {e.Message}");
            }
        }

        /// <summary>
        /// 메뉴 화면 Activity를 설정합니다.
        /// </summary>
        public static void SetMenuActivity()
        {
            SetGameActivity(new ActivityInfo
            {
                Status = "메뉴",
                DisplayText = "메뉴 화면"
            });
        }

        /// <summary>
        /// 게임 플레이 Activity를 설정합니다.
        /// </summary>
        /// <param name="level">현재 레벨</param>
        public static void SetPlayingActivity(int level = 0)
        {
            string status = level > 0 ? $"레벨 {level} 플레이 중" : "게임 플레이 중";
            SetGameActivity(new ActivityInfo
            {
                Status = status,
                DisplayText = status
            });
        }

        /// <summary>
        /// 로비 Activity를 설정합니다.
        /// </summary>
        /// <param name="playerCount">현재 플레이어 수</param>
        /// <param name="maxPlayers">최대 플레이어 수</param>
        public static void SetLobbyActivity(int playerCount, int maxPlayers)
        {
            string status = $"로비 ({playerCount}/{maxPlayers})";
            SetGameActivity(new ActivityInfo
            {
                Status = status,
                DisplayText = status
            });
        }
    }

    /// <summary>
    /// Activity 정보를 담는 구조체
    /// </summary>
    public struct ActivityInfo
    {
        public string Status;
        public string DisplayText;
        public string ConnectString;
        public Dictionary<string, string> CustomRichPresence;
    }
}
