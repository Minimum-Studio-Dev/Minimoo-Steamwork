using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Minimoo.SteamWork
{
    public static class SteamLogin
    {
        private static bool isInitialized = false;

        public static void Initialize()
        {
            if (!SteamManager.Instance.IsSteamInitialized)
            {
                Debug.LogError("Steam is not initialized. Cannot initialize SteamLogin.");
                return;
            }

            isInitialized = true;
            Debug.Log("SteamLogin initialized successfully.");
        }

        /// <summary>
        /// 현재 사용자가 Steam에 로그인되어 있는지 확인합니다.
        /// </summary>
        public static bool IsLoggedIn()
        {
            if (!isInitialized) return false;
            return SteamUser.BLoggedOn();
        }

        /// <summary>
        /// 현재 사용자의 Steam ID를 가져옵니다.
        /// </summary>
        public static CSteamID GetUserSteamId()
        {
            if (!isInitialized) return new CSteamID();
            return SteamManager.Instance.UserSteamId;
        }

        /// <summary>
        /// 현재 사용자의 이름을 가져옵니다.
        /// </summary>
        public static string GetUserName()
        {
            if (!isInitialized) return string.Empty;
            return SteamManager.Instance.UserPersonaName;
        }

        /// <summary>
        /// 현재 사용자의 아바타를 가져옵니다.
        /// </summary>
        public static Texture2D GetUserAvatar()
        {
            if (!isInitialized) return null;

            try
            {
                int avatarHandle = SteamFriends.GetLargeFriendAvatar(SteamManager.Instance.UserSteamId);
                if (avatarHandle > 0)
                {
                    return CreateTextureFromSteamImage(avatarHandle);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get user avatar: {e.Message}");
            }

            return null;
        }

        /// <summary>
        /// 현재 사용자의 프로필 URL을 가져옵니다.
        /// </summary>
        public static string GetUserProfileUrl()
        {
            if (!isInitialized) return string.Empty;
            return $"https://steamcommunity.com/profiles/{SteamManager.Instance.UserSteamId}";
        }

        /// <summary>
        /// Steam 오버레이를 엽니다.
        /// </summary>
        public static void OpenSteamOverlay()
        {
            if (!isInitialized) return;
            SteamFriends.ActivateGameOverlay("friends");
        }

        /// <summary>
        /// Steam 친구 목록 오버레이를 엽니다.
        /// </summary>
        public static void OpenFriendsOverlay()
        {
            if (!isInitialized) return;
            SteamFriends.ActivateGameOverlay("friends");
        }

        /// <summary>
        /// Steam 프로필 오버레이를 엽니다.
        /// </summary>
        public static void OpenProfileOverlay()
        {
            if (!isInitialized) return;
            SteamFriends.ActivateGameOverlayToUser("steamid", SteamManager.Instance.UserSteamId);
        }

        /// <summary>
        /// Steam 스토어 오버레이를 엽니다.
        /// </summary>
        public static void OpenStoreOverlay()
        {
            if (!isInitialized) return;
            SteamFriends.ActivateGameOverlayToStore(new AppId_t(0), EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
        }

        private static Texture2D CreateTextureFromSteamImage(int avatarHandle)
        {
            uint width, height;
            SteamUtils.GetImageSize(avatarHandle, out width, out height);

            if (width == 0 || height == 0)
                return null;

            byte[] imageData = new byte[width * height * 4];
            if (SteamUtils.GetImageRGBA(avatarHandle, imageData, (int)(width * height * 4)))
            {
                var texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                texture.LoadRawTextureData(imageData);
                texture.Apply();
                return texture;
            }

            return null;
        }

        /// <summary>
        /// 사용자의 게임 소유권을 확인합니다.
        /// </summary>
        public static bool CheckGameOwnership()
        {
            if (!isInitialized) return false;

            try
            {
                return SteamApps.BIsSubscribed();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to check game ownership: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 현재 게임의 빌드 ID를 가져옵니다.
        /// </summary>
        public static int GetCurrentBuildId()
        {
            if (!isInitialized) return 0;

            try
            {
                return SteamApps.GetAppBuildId();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get build ID: {e.Message}");
                return 0;
            }
        }
    }
}
