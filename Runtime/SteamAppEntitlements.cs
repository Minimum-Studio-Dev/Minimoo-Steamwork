using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Minimoo;

#if UNITY_STANDALONE
using Steamworks;
#endif

namespace Minimoo.SteamWork
{
    public static class SteamAppEntitlements
    {
#if UNITY_STANDALONE
        private static bool isInitialized = false;
        private static Dictionary<uint, bool> entitlementCache = new Dictionary<uint, bool>();
#endif

        public static void Initialize()
        {
#if UNITY_STANDALONE
            if (!SteamManager.Instance.IsSteamInitialized)
            {
                D.Error("Steam이 초기화되지 않았습니다. SteamAppEntitlements를 초기화할 수 없습니다.");
                return;
            }

            isInitialized = true;
            D.Log("SteamAppEntitlements가 성공적으로 초기화되었습니다.");
#endif
        }

        /// <summary>
        /// 특정 앱 ID가 구독되었는지 확인합니다.
        /// </summary>
        /// <param name="appId">앱 ID</param>
        /// <returns>구독 상태</returns>
        public static bool IsSubscribedApp(uint appId)
        {
#if UNITY_STANDALONE
            if (!isInitialized) return false;

            try
            {
                if (entitlementCache.TryGetValue(appId, out bool isSubscribed))
                {
                    return isSubscribed;
                }

                // 캐시에 없는 경우 Steam API로 직접 확인
                var steamAppId = new AppId_t(appId);
                bool subscribed = SteamApps.BIsSubscribedApp(steamAppId);
                entitlementCache[appId] = subscribed;

                D.Log($"앱 구독 상태 확인: {appId} = {subscribed}");
                return subscribed;
            }
            catch (Exception e)
            {
                D.Error($"앱 구독 상태 확인 중 예외 발생: {e.Message}");
                return false;
            }
#else
            return false;
#endif
        }

        /// <summary>
        /// 특정 DLC가 설치되었는지 확인합니다.
        /// </summary>
        /// <param name="appId">DLC 앱 ID</param>
        /// <returns>DLC 설치 상태</returns>
        public static bool IsDlcInstalled(uint appId)
        {
#if UNITY_STANDALONE
            if (!isInitialized) return false;

            try
            {
                var steamAppId = new AppId_t(appId);
                bool isInstalled = SteamApps.BIsDlcInstalled(steamAppId);

                D.Log($"DLC 설치 상태 확인: {appId} = {isInstalled}");
                return isInstalled;
            }
            catch (Exception e)
            {
                D.Error($"DLC 설치 상태 확인 중 예외 발생: {e.Message}");
                return false;
            }
#else
            return false;
#endif
        }

        /// <summary>
        /// DLC를 설치합니다.
        /// </summary>
        /// <param name="appId">DLC 앱 ID</param>
        /// <returns>설치 성공 여부</returns>
        public static bool InstallDlc(uint appId)
        {
#if UNITY_STANDALONE
            if (!isInitialized) return false;

            try
            {
                var steamAppId = new AppId_t(appId);
                bool result = SteamApps.InstallDLC(steamAppId);

                if (result)
                {
                    D.Log($"DLC 설치 요청 성공: {appId}");
                }
                else
                {
                    D.Error($"DLC 설치 요청 실패: {appId}");
                }

                return result;
            }
            catch (Exception e)
            {
                D.Error($"DLC 설치 중 예외 발생: {e.Message}");
                return false;
            }
#else
            return false;
#endif
        }

        /// <summary>
        /// 앱 구독 상태 캐시를 클리어합니다.
        /// </summary>
        public static void ClearEntitlementCache()
        {
#if UNITY_STANDALONE
            entitlementCache.Clear();
            D.Log("앱 구독 상태 캐시가 클리어되었습니다.");
#endif
        }

        /// <summary>
        /// 현재 앱의 구독 상태를 확인합니다.
        /// </summary>
        /// <returns>현재 앱 구독 상태</returns>
        public static bool IsCurrentAppSubscribed()
        {
#if UNITY_STANDALONE
            if (!isInitialized) return false;

            try
            {
                bool isSubscribed = SteamApps.BIsSubscribed();
                D.Log($"현재 앱 구독 상태: {isSubscribed}");
                return isSubscribed;
            }
            catch (Exception e)
            {
                D.Error($"현재 앱 구독 상태 확인 중 예외 발생: {e.Message}");
                return false;
            }
#else
            return false;
#endif
        }

        /// <summary>
        /// 앱이 무료인지 확인합니다.
        /// </summary>
        /// <returns>무료 앱 여부</returns>
        public static bool IsAppFree()
        {
#if UNITY_STANDALONE
            if (!isInitialized) return false;

            try
            {
                bool isFree = SteamApps.BIsSubscribedApp(SteamUtils.GetAppID());
                D.Log($"앱 무료 여부: {isFree}");
                return isFree;
            }
            catch (Exception e)
            {
                D.Error($"앱 무료 여부 확인 중 예외 발생: {e.Message}");
                return false;
            }
#else
            return false;
#endif
        }

        /// <summary>
        /// 특정 앱의 소유권을 확인합니다.
        /// </summary>
        /// <param name="appId">앱 ID</param>
        /// <returns>소유권 상태</returns>
        public static bool IsAppOwned(uint appId)
        {
#if UNITY_STANDALONE
            if (!isInitialized) return false;

            try
            {
                // BIsSubscribedApp은 소유권을 포함한 구독 상태를 확인
                return IsSubscribedApp(appId);
            }
            catch (Exception e)
            {
                D.Error($"앱 소유권 확인 중 예외 발생: {e.Message}");
                return false;
            }
#else
            return false;
#endif
        }
    }
}
