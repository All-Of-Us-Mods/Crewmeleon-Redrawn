using Reactor.Networking.Attributes;
using UnityEngine;

namespace CrewmeleonRedrawn.Networking;

public static class TransformRpc
{
    private const float NetworkPositionMin = -50f;
    private const float NetworkPositionMax = 50f;
    private const float NetworkPositionRange = NetworkPositionMax - NetworkPositionMin;

    [MethodRpc((uint)CrewmeleonRpc.SyncFacing)]
    public static void RpcResyncTransform(this PlayerControl player, bool flipX)
    {
        if (!player)
            return;

        if (player.AmOwner)
        {
            var position = (Vector2)player.transform.position;
            player.NetTransform.RpcSnapTo(position);

            var networkPosition = PredictNetworkPosition(position);
            player.transform.position = new Vector3(
                networkPosition.x,
                networkPosition.y,
                player.transform.position.z);
        }

        player.cosmetics.SetFlipX(flipX);
    }

    private static Vector2 PredictNetworkPosition(Vector2 position) => new(
        PredictNetworkCoordinate(position.x),
        PredictNetworkCoordinate(position.y));

    private static float PredictNetworkCoordinate(float value)
    {
        var normalized = Mathf.Clamp01((value - NetworkPositionMin) / NetworkPositionRange);
        var encoded = (ushort)(normalized * ushort.MaxValue);
        return Mathf.Lerp(NetworkPositionMin, NetworkPositionMax, encoded / (float)ushort.MaxValue);
    }
}