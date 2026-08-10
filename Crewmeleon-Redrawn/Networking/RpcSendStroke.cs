using Crewmeleon_Redrawn;
using Crewmeleon_Redrawn.Components;
using Crewmeleon_Redrawn.Networking;
using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Extensions;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using UnityEngine;

[RegisterCustomRpc((uint)CrRpcs.SendStroke)]
public class RpcSendStroke(CrewmeleonRedrawnPlugin plugin, uint id)
    : PlayerCustomRpc<CrewmeleonRedrawnPlugin, StrokeChunk>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;

    public override void Write(MessageWriter writer, StrokeChunk data)
    {
        var startLength = writer.Length;

        writer.Write(data.IsFirst);
        writer.Write(data.IsFinal);

        // brush details ride only on the opening chunk; a custom mask alone can approach the
        // packet limit, so it must not repeat per chunk
        if (data.IsFirst)
        {
            writer.Write(data.Brush.Color);
            writer.Write(data.Brush.Radius);
            writer.Write(data.Brush.Opacity);
            writer.Write(data.Brush.Hardness);
            writer.Write((byte)data.Brush.Shape);

            if (data.Brush.Shape == BrushShape.Custom)
            {
                writer.Write(data.Brush.Mirrored);

                var encoded = data.Brush.Mask?.Encode() ?? [];
                writer.WritePacked((uint)encoded.Length);
                writer.Write(encoded);
            }
        }

        writer.WritePacked((uint)data.Points.Length);
        foreach (var point in data.Points)
        {
            writer.Write((short)point.x);
            writer.Write((short)point.y);
        }

        PaintNetStats.RecordSent(writer.Length - startLength, data.Points.Length, data.IsFirst, data.IsFinal);
    }

    public override StrokeChunk Read(MessageReader reader)
    {
        var startPosition = reader.Position;

        var isFirst = reader.ReadBoolean();
        var isFinal = reader.ReadBoolean();

        var brush = default(BrushStamp);
        if (isFirst)
        {
            brush = new BrushStamp(
                reader.ReadColor32(),
                reader.ReadByte(),
                reader.ReadByte(),
                reader.ReadByte(),
                (BrushShape)reader.ReadByte());

            if (brush.Shape == BrushShape.Custom)
            {
                var mirrored = reader.ReadBoolean();
                var length = (int)reader.ReadPackedUInt32();
                var encoded = reader.ReadBytes(length);
                brush = new BrushStamp(brush.Color, brush.Radius, brush.Opacity, brush.Hardness,
                    brush.Shape, BrushMask.Decode(encoded), mirrored);
            }
        }

        var count = reader.ReadPackedUInt32();
        var points = new Vector2Int[count];
        for (var i = 0; i < count; i++)
        {
            points[i] = new Vector2Int(reader.ReadInt16(), reader.ReadInt16());
        }

        PaintNetStats.RecordReceived(reader.Position - startPosition, isFinal);

        return new StrokeChunk(isFirst, isFinal, brush, points);
    }

    public override void Handle(PlayerControl innerNetObject, StrokeChunk data)
    {
        // includeInactive: the canvas sits on an inactive object until the player becomes a hider
        var canvas = innerNetObject.gameObject.GetComponentInChildren<PlayerCanvasComponent>(true);
        if (canvas == null)
        {
            Logger<CrewmeleonRedrawnPlugin>.Error("Could not handle stroke RPC. Canvas component is null.");
            return;
        }

        if (data.IsFirst) canvas.BeginRemoteStroke(data.Brush);
        if (data.Points.Length > 0) canvas.AppendRemoteStroke(data.Points);
        if (data.IsFinal) canvas.FinishRemoteStroke();
    }
}
