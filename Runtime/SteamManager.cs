using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Minimoo;

#if UNITY_STANDALONE
using Steamworks;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Minimoo.SteamWork
{
    public class SteamManager : Singleton<SteamManager>
    {
        public bool IsSteamInitialized
        {
            get
            {
#if UNITY_STANDALONE
                return _isSteamInitialized;
#else
                return false;
#endif
            }
            private set
            {
#if UNITY_STANDALONE
                _isSteamInitialized = value;
#endif
            }
        }
#if UNITY_STANDALONE
        public CSteamID UserSteamId
        {
            get
            {
                return _userSteamId;
                return default(CSteamID);
            }
            private set
            {
                _userSteamId = value;
            }
        }
#endif
        public string UserName
        {
            get
            {
#if UNITY_STANDALONE
                return _userName;
#else
                return string.Empty;
#endif
            }
            private set
            {
#if UNITY_STANDALONE
                _userName = value;
#endif
            }
        }

        public string UserPersonaName
        {
            get
            {
#if UNITY_STANDALONE
                return _userPersonaName;
#else
                return string.Empty;
#endif
            }
            private set
            {
#if UNITY_STANDALONE
                _userPersonaName = value;
#endif
            }
        }

#if UNITY_STANDALONE
        private bool _isSteamInitialized = false;
        private CSteamID _userSteamId;
        private string _userName = string.Empty;
        private string _userPersonaName = string.Empty;
        private Callback<GameOverlayActivated_t> gameOverlayActivatedCallback;
        private Callback<PersonaStateChange_t> personaStateChangeCallback;
#endif

        protected override void Awake()
        {
            base.Awake();
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif

#if UNITY_STANDALONE
#if UNITY_EDITOR
            if (Application.isPlaying && SteamManagerTestMode.IsEnabled)
#endif
            {
                InitializeSteam();
            }
#endif
        }

        private void OnDisable()
        {
#if UNITY_STANDALONE
#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
            {
                Shutdown();
            }
#endif
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif

#if UNITY_STANDALONE
#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
            {
                Shutdown();
            }
#endif
        }

#if UNITY_STANDALONE
         private void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying || !SteamManagerTestMode.IsEnabled) return;
#endif
            if (_isSteamInitialized)
            {
                SteamAPI.RunCallbacks();
            }
        }
#endif

#if UNITY_STANDALONE
        private void InitializeSteam()
        {
            try
            {
                // Steamworks.NET 초기화
                if (!SteamAPI.Init())
                {
                    D.Error("Failed to initialize Steam API");
                    _isSteamInitialized = false;
                    return;
                }

                _isSteamInitialized = true;
                UserSteamId = SteamUser.GetSteamID();
                UserName = SteamFriends.GetPersonaName();
                UserPersonaName = SteamFriends.GetPersonaName();

                D.Log($"Steam initialized successfully. User: {UserPersonaName} ({UserSteamId})");

                // Setup callbacks
                SetupCallbacks();

                // Initialize subsystems
                SteamLogin.Initialize();
                SteamCloudSave.Initialize();
                SteamAchievements.Initialize();
                SteamActivity.Initialize();
                SteamLeaderboards.Initialize();
                SteamAppEntitlements.Initialize();
            }
            catch (Exception e)
            {
                D.Error($"Failed to initialize Steam: {e.Message}");
                IsSteamInitialized = false;
            }
        }
#endif

#if UNITY_STANDALONE
        private void SetupCallbacks()
        {
            gameOverlayActivatedCallback = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
            personaStateChangeCallback = Callback<PersonaStateChange_t>.Create(OnPersonaStateChange);
        }
#endif

#if UNITY_STANDALONE
        private void OnGameOverlayActivated(GameOverlayActivated_t callback)
        {
            D.Log($"Steam overlay activated: {callback.m_bActive}");
        }

        private void OnPersonaStateChange(PersonaStateChange_t callback)
        {
            if (callback.m_ulSteamID == UserSteamId.m_SteamID)
            {
                UserPersonaName = SteamFriends.GetPersonaName();
                D.Log($"Persona state changed: {UserPersonaName}");
            }
        }
#endif
     
        public void Shutdown()
        {
#if UNITY_STANDALONE
            if (_isSteamInitialized)
            {
                SteamAPI.Shutdown();
                _isSteamInitialized = false;
            }
#endif
        }

#if UNITY_EDITOR
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                D.Log("Exiting play mode, shutting down Steam");
                Shutdown();
            }
        }
#endif

    }
}
