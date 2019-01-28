using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirror.Components.NetworkLobby
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkLobbyPlayer")]
    public class NetworkLobbyPlayer : NetworkBehaviour
    {

        [SerializeField] public bool ShowLobbyGUI = true;

        [SyncVar]
        public bool ReadyToBegin = false;

        [SyncVar]
        public int Index;

        void Start()
        {
            DontDestroyOnLoad(gameObject);
            if (isClient)
                SceneManager.sceneLoaded += ClientLoadedScene;
        }

        private void ClientLoadedScene(Scene arg0, LoadSceneMode arg1)
        {
            var lobby = NetworkManager.singleton as NetworkLobbyManager;
            if (lobby)
            {
                // dont even try this in the startup scene
                string loadedSceneName = SceneManager.GetSceneAt(0).name;
                if (loadedSceneName == lobby.LobbyScene)
                    return;
            }

            if (isLocalPlayer)
                CmdSendLevelLoaded();
        }

        public override void OnStartClient()
        {
            var lobby = NetworkManager.singleton as NetworkLobbyManager;
            if (lobby)
            {
                ReadyToBegin = false;
                OnClientEnterLobby();
            }
            else
                Debug.LogError("LobbyPlayer could not find a NetworkLobbyManager. The LobbyPlayer requires a NetworkLobbyManager object to function. Make sure that there is one in the scene.");
        }

        [Command]
        public void CmdChangeReadyState(bool ReadyState)
        {
            ReadyToBegin = ReadyState;
            var lobby = NetworkManager.singleton as NetworkLobbyManager;
            if (lobby)
                lobby.ReadyStatusChanged();
        }

        [Command]
        public void CmdSendLevelLoaded()
        {
            var lobby = NetworkManager.singleton as NetworkLobbyManager;
            if (lobby)
                lobby.PlayerLoadedScene(GetComponent<NetworkIdentity>().connectionToClient);
        }

        public void RemovePlayer()
        {
            if (isLocalPlayer && !ReadyToBegin)
            {
                if (LogFilter.Debug) { Debug.Log("NetworkLobbyPlayer RemovePlayer"); }

                ClientScene.RemovePlayer();
            }
        }

        // ------------------------ callbacks ------------------------

        public virtual void OnClientEnterLobby() { }

        public virtual void OnClientExitLobby() { }

        public virtual void OnClientReady(bool readyState) { }

        // ------------------------ optional UI ------------------------

        void OnGUI()
        {
            if (!ShowLobbyGUI)
                return;

            var lobby = NetworkManager.singleton as NetworkLobbyManager;
            if (lobby)
            {
                if (!lobby.m_ShowLobbyGUI)
                    return;

                string loadedSceneName = SceneManager.GetSceneAt(0).name;
                if (loadedSceneName != lobby.LobbyScene)
                    return;
            }

            Rect rec = new Rect(20 + Index * 100, 200, 90, 20);

            GUI.Label(rec, String.Format("Player [{0}]", netId));

            rec.y += 25;
            if (ReadyToBegin)
                GUI.Label(rec, "  Ready");
            else
                GUI.Label(rec, "Not Ready");

            rec.y += 25;
            if (isServer && GUI.Button(rec, "REMOVE"))
            {
                Debug.Log("Need code for Host to kick player correctly");
            }

            if (NetworkClient.active && isLocalPlayer)
            {
                Rect readyCancelRect = new Rect(20, 300, 120, 20);

                if (ReadyToBegin)
                {
                    if (GUI.Button(readyCancelRect, "Cancel"))
                        CmdChangeReadyState(false);
                }
                else
                {
                    if (GUI.Button(readyCancelRect, "Ready"))
                        CmdChangeReadyState(true);
                }
            }
        }
    }
}