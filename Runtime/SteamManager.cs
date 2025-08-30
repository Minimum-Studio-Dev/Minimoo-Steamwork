using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using Minimoo;

namespace Minimoo.SteamWork
{
    public class SteamManager : MonoBehaviour
    {
        private static SteamManager instance;
        public static SteamManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("SteamManager");
                    instance = go.AddComponent<SteamManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        public bool IsSteamInitialized { get; private set; }
        public CSteamID UserSteamId { get; private set; }
        public string UserName { get; private set; }
        public string UserPersonaName { get; private set; }

        private Callback<GameOverlayActivated_t> gameOverlayActivatedCallback;
        private Callback<PersonaStateChange_t> personaStateChangeCallback;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSteam();
        }

        private void InitializeSteam()
        {
            try
            {
                // Steamworks.NET 초기화
                if (!SteamAPI.Init())
                {
                    D.Error("Failed to initialize Steam API");
                    IsSteamInitialized = false;
                    return;
                }

                IsSteamInitialized = true;
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
            }
            catch (Exception e)
            {
                D.Error($"Failed to initialize Steam: {e.Message}");
                IsSteamInitialized = false;
            }
        }

        private void SetupCallbacks()
        {
            gameOverlayActivatedCallback = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
            personaStateChangeCallback = Callback<PersonaStateChange_t>.Create(OnPersonaStateChange);
        }

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

        private void Update()
        {
            if (IsSteamInitialized)
            {
                SteamAPI.RunCallbacks();
            }
        }

        private void OnApplicationQuit()
        {
            if (IsSteamInitialized)
            {
                SteamAPI.Shutdown();
                IsSteamInitialized = false;
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
