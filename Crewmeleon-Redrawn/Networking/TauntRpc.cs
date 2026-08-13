using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Crewmeleon_Redrawn.Networking;

public static class TauntRpc
{
    [MethodRpc((uint)CrewmeleonRpc.Taunt)]
    public static void RpcTaunt(this PlayerControl source)
    {
        AudioSource.PlayClipAtPoint(TauntSounds.Random()?.LoadAsset(), source.GetTruePosition(), 0.4f);
    }

    public static List<LoadableAudioResourceAsset> TauntSounds =
    [
        new("Crewmeleon-Redrawn.Resources.Sounds.taunt_1.wav"),
        new("Crewmeleon-Redrawn.Resources.Sounds.taunt_2.wav"),
        new("Crewmeleon-Redrawn.Resources.Sounds.taunt_3.wav"),
        new("Crewmeleon-Redrawn.Resources.Sounds.taunt_4.wav"),
        new("Crewmeleon-Redrawn.Resources.Sounds.taunt_5.wav"),
        new("Crewmeleon-Redrawn.Resources.Sounds.taunt_6.wav"),
        new("Crewmeleon-Redrawn.Resources.Sounds.taunt_7.wav"),
        new("Crewmeleon-Redrawn.Resources.Sounds.taunt_8.wav")
    ];
}