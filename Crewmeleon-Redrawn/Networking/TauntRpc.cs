using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace CrewmeleonRedrawn.Networking;

public static class TauntRpc
{
    [MethodRpc((uint)CrewmeleonRpc.Taunt)]
    public static void RpcTaunt(this PlayerControl source)
    {
        AudioSource.PlayClipAtPoint(CrewmeleonAssets.TauntSounds.Random()?.LoadAsset(), source.GetTruePosition(), 0.4f);
    }
}