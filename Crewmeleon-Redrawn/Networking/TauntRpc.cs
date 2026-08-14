using CrewmeleonRedrawn.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;

namespace CrewmeleonRedrawn.Networking;

public static class TauntRpc
{
    [MethodRpc((uint)CrewmeleonRpc.Taunt)]
    public static void RpcTaunt(this PlayerControl source)
    {
        SoundUtilities.PlayAtPosition(CrewmeleonAssets.TauntSounds.Random()?.LoadAsset(), source.GetTruePosition(), 0.4f);
    }
}
