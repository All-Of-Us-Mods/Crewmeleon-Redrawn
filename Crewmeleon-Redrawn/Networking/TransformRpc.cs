using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using UnityEngine;

namespace CrewmeleonRedrawn.Networking;

public static class TransformRpc
{
    [MethodRpc((uint)CrewmeleonRpc.SyncFacing)]
    public static void RpcResyncTransform(this PlayerControl player, bool flipX)
    {
        if (!player)
            return;

        if (player.AmOwner)
            player.NetTransform.RpcSnapTo((Vector2)player.transform.position);

        player.cosmetics.SetFlipX(flipX);
    }
}