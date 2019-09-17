using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Mirror
{
    /// <summary>
    /// Unity Event for the NetworkConnection
    /// </summary>
    [Serializable] public class UnityEventNetworkConnection : UnityEvent<NetworkConnection> { }

    /// <summary>
    /// Base class for implementing component-based authentication during the Connect phase
    /// </summary>
    [HelpURL("https://mirror-networking.com/xmldocs/articles/Concepts/Authentication.html")]
    public abstract class Authenticator : MonoBehaviour
    {
        [Header("Event Listeners (optional)")]

        /// <summary>
        /// Notify subscribers on the server when a client is authenticated
        /// </summary>
        [Tooltip("Mirror has an internal subscriber to this event. You can add your own here.")]
        public UnityEventNetworkConnection OnServerAuthenticated = new UnityEventNetworkConnection();

        /// <summary>
        /// Notify subscribers on the client when the client is authenticated
        /// </summary>
        [Tooltip("Mirror has an internal subscriber to this event. You can add your own here.")]
        public UnityEventNetworkConnection OnClientAuthenticated = new UnityEventNetworkConnection();

        #region server

        /// <summary>
        /// Called on server from StartServer to initialize the Authenticator
        /// <para>Server message handlers should be registered in this method.</para>
        /// </summary>
        public abstract void OnStartServer();

        /// <summary>
        /// Called on client from StartClient to initialize the Authenticator
        /// <para>Client message handlers should be registered in this method.</para>
        /// </summary>
        public abstract void OnStartClient();

        // This will get more code in the near future
        internal void OnServerAuthenticateInternal(NetworkConnection conn)
        {
            OnServerAuthenticate(conn);
        }

        /// <summary>
        /// Called on server from OnServerAuthenticateInternal when a client needs to authenticate
        /// </summary>
        /// <param name="conn">Connection to client.</param>
        public virtual void OnServerAuthenticate(NetworkConnection conn)
        {
            conn.isAuthenticated = true;
        }

        #endregion

        #region client

        // This will get more code in the near future
        internal void OnClientAuthenticateInternal(NetworkConnection conn)
        {
            OnClientAuthenticate(conn);
        }

        /// <summary>
        /// Called on client from OnClientAuthenticateInternal when a client needs to authenticate
        /// </summary>
        /// <param name="conn">Connection of the client.</param>
        public virtual void OnClientAuthenticate(NetworkConnection conn)
        {
            conn.isAuthenticated = true;
        }

        #endregion

        void OnValidate()
        {
#if UNITY_EDITOR
            // automatically assign NetworkManager field if we add this to NetworkManager
            NetworkManager manager = GetComponent<NetworkManager>();
            if (manager != null && manager.authenticator == null)
            {
                manager.authenticator = this;
                UnityEditor.Undo.RecordObject(gameObject, "Assigned NetworkManager authenticator");
            }
#endif
        }
    }
}
