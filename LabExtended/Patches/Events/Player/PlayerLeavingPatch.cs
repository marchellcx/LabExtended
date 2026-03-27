using HarmonyLib;

using LabExtended.API;
using LabExtended.Events;

using LiteNetLib;

using Mirror.LiteNetLib4Mirror;

namespace LabExtended.Patches.Events.Player;

/// <summary>
/// Implements the <see cref="ExPlayerEvents.Leaving"/> event.
/// </summary>
public static class PlayerLeavingPatch
{
    [HarmonyPatch(typeof(LiteNetLib4MirrorServer), nameof(LiteNetLib4MirrorServer.OnPeerDisconnected))]
    private static bool Prefix(NetPeer peer, DisconnectInfo disconnectinfo)
    {
        LiteNetLib4MirrorTransport.Singleton.Events.Enqueue(delegate
        {
            LiteNetLib4MirrorCore.LastDisconnectError = disconnectinfo.SocketErrorCode;
            LiteNetLib4MirrorCore.LastDisconnectReason = disconnectinfo.Reason;
            
            LiteNetLib4MirrorTransport.Singleton.OnServerDisconnected(peer.Id + 1);

            if (LiteNetLib4MirrorServer.Peers.TryRemove(peer.Id + 1, out var netPeer))
            {
                ExPlayer? player = ExPlayer.Get(peer);
        
                if (player?.ReferenceHub != null)
                    ExPlayerEvents.OnLeaving((new(player, disconnectinfo is { Reason: DisconnectReason.Timeout }, disconnectinfo)));
            }
        });
        
        return false;
    }
}