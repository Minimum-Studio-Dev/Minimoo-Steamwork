using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

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
        public SteamId UserSteamId { get; private set; }
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
                SteamClient.Init(0); // 0 means use the appid from steam_appid.txt

                IsSteamInitialized = true;
                UserSteamId = SteamClient.SteamId;
                UserName = SteamClient.Name;
                UserPersonaName = SteamFriends.GetPersonaName();

                Debug.Log($"Steam initialized successfully. User: {UserPersonaName} ({UserSteamId})");

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
                Debug.LogError($"Failed to initialize Steam: {e.Message}");
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
            Debug.Log($"Steam overlay activated: {callback.m_bActive}");
        }

        private void OnPersonaStateChange(PersonaStateChange_t callback)
        {
            if (callback.m_ulSteamID == UserSteamId.Value)
            {
                UserPersonaName = SteamFriends.GetPersonaName();
                Debug.Log($"Persona state changed: {UserPersonaName}");
            }
        }

        private void Update()
        {
            if (IsSteamInitialized)
            {
                SteamClient.RunCallbacks();
            }
        }

        private void OnApplicationQuit()
        {
            if (IsSteamInitialized)
            {
                SteamClient.Shutdown();
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
