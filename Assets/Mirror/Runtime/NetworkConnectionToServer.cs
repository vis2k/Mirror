using System;
using System.Collections.Generic;

namespace Mirror
{
    public class NetworkConnectionToServer : NetworkConnection
    {
        public override string address => "";

        // batching from client to server.
        // fewer transport calls give us significantly better performance/scale.
        //
        // for a 64KB max message transport and 64 bytes/message on average, we
        // reduce transport calls by a factor of 1000.
        //
        // depending on the transport, this can give 10x performance.
        //
        // Dictionary<channelId, batch> because we have multiple channels.
        Dictionary<int, Batcher> batches = new Dictionary<int, Batcher>();

        // batch messages and send them out in LateUpdate (or after batchInterval)
        bool batching;

        public NetworkConnectionToServer(bool batching)
        {
            this.batching = batching;
        }

        // TODO if we only have Reliable/Unreliable, then we could initialize
        // two batches and avoid this code
        Batcher GetBatchForChannelId(int channelId)
        {
            // get existing or create new writer for the channelId
            Batcher batch;
            if (!batches.TryGetValue(channelId, out batch))
            {
                // get max batch size for this channel
                int MaxBatchSize = Transport.activeTransport.GetMaxBatchSize(channelId);

                // create batcher
                batch = new Batcher(MaxBatchSize);
                batches[channelId] = batch;
            }
            return batch;
        }

        internal override void Send(ArraySegment<byte> segment, int channelId = Channels.Reliable)
        {
            // Debug.Log("ConnectionSend " + this + " bytes:" + BitConverter.ToString(segment.Array, segment.Offset, segment.Count));

            // validate packet size first.
            if (ValidatePacketSize(segment, channelId))
            {
                // batching enabled?
                if (batching)
                {
                    // try to batch, or send directly if too big.
                    // (user might try to send a max message sized message,
                    //  where max message size is larger than max batch size.
                    //  for example, kcp2k max message size is 144 KB but we
                    //  only want to batch MTU each time)
                    if (!GetBatchForChannelId(channelId).AddMessage(segment))
                        Transport.activeTransport.ClientSend(segment, channelId);
                }
                // otherwise send directly to minimize latency
                else Transport.activeTransport.ClientSend(segment, channelId);
            }
        }

        // flush batched messages at the end of every Update.
        internal virtual void Update()
        {
            // batching?
            if (batching)
            {
                // go through batches for all channels
                foreach (KeyValuePair<int, Batcher> kvp in batches)
                {
                    // make and send as many batches as necessary from the stored
                    // messages.
                    Batcher batcher = kvp.Value;
                    using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
                    {
                        while (batcher.MakeNextBatch(writer))
                        {
                            // send
                            Transport.activeTransport.ClientSend(writer.ToArraySegment(), kvp.Key);
                            //UnityEngine.Debug.Log($"sending batch of {writer.Position} bytes for channel={kvp.Key}");

                            // reset writer for each new batch
                            writer.Position = 0;
                        }
                    }
                }
            }
        }

        /// <summary>Disconnects this connection.</summary>
        public override void Disconnect()
        {
            // set not ready and handle clientscene disconnect in any case
            // (might be client or host mode here)
            // TODO remove redundant state. have one source of truth for .ready!
            isReady = false;
            NetworkClient.ready = false;
            Transport.activeTransport.ClientDisconnect();
        }
    }
}
