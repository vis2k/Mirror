﻿using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

namespace Mirror
{
    // UnityEvent definitions
    [Serializable] public class UnityEventConnection : UnityEvent<NetworkConnection> { }

    public abstract class Authenticator : MonoBehaviour
    {
        /// <summary>
        /// The current Authenticator used by Mirror. 
        /// </summary>
        public static Authenticator activeAuthenticator;

        /// <summary>
        /// Determines if client has been authenticated
        /// </summary>
        /// <returns>True if a client has been authenticated successfully</returns>
        public abstract bool isAuthenticated();

        /// <summary>
        /// Notify subscribers on the server when a client is authenticated
        /// </summary>
        [HideInInspector] public UnityEventConnection OnServerAuthenticated = new UnityEventConnection();

        /// <summary>
        /// Notify subscribers on the client when a client is authenticated
        /// </summary>
        [HideInInspector] public UnityEventConnection OnClientAuthenticated = new UnityEventConnection();

        /// <summary>
        /// Called on server when a client needs to authenticate
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public abstract void ServerAuthenticate(NetworkConnection conn);

        /// <summary>
        /// Called on client when a client needs to authenticate
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public abstract void ClientAuthenticate(NetworkConnection conn);
    }
}
