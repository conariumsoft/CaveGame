using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CaveGame.Core.Network
{
    public struct OutgoingPayload
    {
        public Packet Payload { get; set; }
        public IPEndPoint TargetAddress { get; set; } 
    }

   public class SharedNetworkSubsystem
   {
        public const int NAP_TIME_MILLISECONDS = 1;
        public const int SIO_UDP_CONNRESET = -1744830452;
        public const int PROTOCOL_VERSION = Globals.ProtocolVersion;

        protected ConcurrentQueue<NetworkMessage> IncomingMessages { get; private set; }
        protected ConcurrentQueue<OutgoingPayload> OutgoingMessages { get; private set; }
       

        public virtual DateTime LatestReceiveTimestamp { get; protected set; }
        public virtual DateTime LatestSendTimestamp { get; protected set; }
        public virtual int PacketsReceived { get; protected set; }
        public virtual int PacketsSent { get; protected set; }
        public virtual int TotalBytesSent { get; protected set; }
        public virtual int TotalBytesReceived { get; protected set; }
        public virtual int BytesSentPerSecond { get; protected set; }
        public virtual int BytesReceivedPerSecond { get; protected set; }

        public virtual IMessageOutlet Output { get; set; }

        public virtual int Port { get; protected set; }

        protected int InternalReceiveCount { get; set; }
        protected int InternalSendCount { get; set; }

        private float counter = 0;

        public UdpClient UdpSocket { get; protected set; }

        protected void IOControlFixICMPBullshit()
        {
            UdpSocket.Client.IOControl(
                (IOControlCode)SIO_UDP_CONNRESET,
                new byte[] { 0, 0, 0, 0 },
                null
            );
        }

        public SharedNetworkSubsystem()
        {
            IncomingMessages = new ConcurrentQueue<NetworkMessage>();
            OutgoingMessages = new ConcurrentQueue<OutgoingPayload>();
            IOControlFixICMPBullshit();
        }

        private void ResetByteCounters()
        {
            BytesSentPerSecond = InternalSendCount;
            BytesReceivedPerSecond = InternalReceiveCount;
            InternalSendCount = 0;
            InternalReceiveCount = 0;
            counter = 0;
        }


        public virtual void Update(GameTime gt)
        {
            counter += gt.GetDelta();
            if (counter > (1.0f))
                ResetByteCounters();
        }

        public virtual void Start()
        {

        }

        public virtual void Close()
        {

        }


    }
}
