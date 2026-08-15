using CrewmeleonRedrawn;
using CrewmeleonRedrawn.Components;
using CrewmeleonRedrawn.Networking;
using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Extensions;
using Reactor.Networking.Rpc;
using Reactor.Utilities;

[RegisterCustomRpc((uint)CrewmeleonRpc.UndoStroke)]
public class RpcUndoStroke(CrewmeleonRedrawnPlugin plugin, uint id)
    : PlayerCustomRpc<CrewmeleonRedrawnPlugin, StrokeUndoRequest>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;

    public override void Write(MessageWriter writer, StrokeUndoRequest data)
    {
        writer.WritePacked(data.SequenceId);
        writer.WritePacked(data.StrokeId);
    }

    public override StrokeUndoRequest Read(MessageReader reader) => new(reader.ReadPackedUInt32(), reader.ReadPackedUInt32());

    public override void Handle(PlayerControl innerNetObject, StrokeUndoRequest data)
    {
        var canvas = innerNetObject.gameObject.GetComponentInChildren<PlayerCanvasComponent>(true);
        if (canvas == null)
        {
            Logger<CrewmeleonRedrawnPlugin>.Error("Could not handle undo RPC. Canvas component is null.");
            return;
        }

        canvas.ReceiveRemoteUndo(data);
    }
}
