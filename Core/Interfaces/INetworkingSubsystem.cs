using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core
{
    public interface INetworkingSubsystem
    {
        DateTime LatestReceiveTimestamp { get; }
        DateTime LatestSendTimestamp { get; }

        int PacketsReceived { get; }
        int PacketsSent { get; }

        int TotalBytesSent { get; }
        int TotalBytesReceived { get; }

        int BytesSentPerSecond { get; }
        int BytesReceivedPerSecond { get; }

        void Update(GameTime gt);
    }
}
