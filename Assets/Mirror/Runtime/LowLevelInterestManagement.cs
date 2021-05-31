// interest management component for custom solutions like
// distance based, spatial hashing, raycast based, etc.
using System.Collections.Generic;
using UnityEngine;

namespace Mirror
{
    [DisallowMultipleComponent]
    public abstract class LowLevelInterestManagement : MonoBehaviour
    {
        // Awake configures LowLevelInterestManagement in NetworkServer/Client
        protected virtual void Awake()
        {
            if (NetworkServer.aoi == null)
            {
                NetworkServer.aoi = this;
            }
            else Debug.LogError($"Only one InterestManagement component allowed. {NetworkServer.aoi.GetType()} has been set up already.");

            if (NetworkClient.aoi == null)
            {
                NetworkClient.aoi = this;
            }
            else Debug.LogError($"Only one InterestManagement component allowed. {NetworkClient.aoi.GetType()} has been set up already.");
        }

        /// <summary>
        /// This is called on the server when a new networked object is spawned
        /// </summary>
        public virtual void OnSpawned(NetworkIdentity identity) {}

        /// <summary>
        /// This is called on the server when a networked object is destroyed
        /// </summary>
        public virtual void OnDestroyed(NetworkIdentity identity) {}

        public abstract void OnRequestRebuild(NetworkIdentity identity, bool initialize);

        // Callback used by the visibility system to determine if an observer
        // (player) can see the NetworkIdentity. If this function returns true,
        // the network connection will be added as an observer.
        //   conn: Network connection of a player.
        //   returns True if the player can see this object.
        public abstract bool OnCheckObserver(NetworkIdentity identity, NetworkConnection newObserver);

        /// <summary>
        /// Adds the specified connection to the observers of identity
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="connection"></param>
        protected void AddObserver(NetworkConnection connection, NetworkIdentity identity)
        {
            connection.AddToObserving(identity);
            identity.observers.Add(connection.connectionId, connection);
        }

        /// <summary>
        /// Removes the specified connection from the observers of identity
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="connection"></param>
        protected void RemoveObserver(NetworkConnection connection, NetworkIdentity identity)
        {
            connection.RemoveFromObserving(identity, false);
            identity.observers.Remove(connection.connectionId);
        }

        // Callback used by the visibility system for objects on a host.
        // Objects on a host (with a local client) cannot be disabled or
        // destroyed when they are not visible to the local client. So this
        // function is called to allow custom code to hide these objects. A
        // typical implementation will disable renderer components on the
        // object. This is only called on local clients on a host.
        // => need the function in here and virtual so people can overwrite!
        // => not everyone wants to hide renderers!
        public virtual void SetHostVisibility(NetworkIdentity identity, bool visible)
        {
            foreach (Renderer rend in identity.GetComponentsInChildren<Renderer>())
                rend.enabled = visible;
        }
    }
}
