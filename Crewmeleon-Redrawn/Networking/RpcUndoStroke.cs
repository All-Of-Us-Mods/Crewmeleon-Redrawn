using Crewmeleon_Redrawn;
using Crewmeleon_Redrawn.Components;
using Crewmeleon_Redrawn.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;

[RegisterCustomRpc((uint)CrRpcs.UndoStroke)]
public class RpcUndoStroke(CrewmeleonRedrawnPlugin plugin, uint id)
    : PlayerCustomRpc<CrewmeleonRedrawnPlugin>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;

    public override void Handle(PlayerControl innerNetObject)
    {
        var canvas = innerNetObject.gameObject.GetComponentInChildren<PlayerCanvasComponent>(true);
        if (canvas == null)
        {
            Logger<CrewmeleonRedrawnPlugin>.Error("Could not handle undo RPC. Canvas component is null.");
            return;
        }

        canvas.UndoStroke();
    }
}
