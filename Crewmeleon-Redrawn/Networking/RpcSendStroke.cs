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
    : PlayerCustomRpc<CrewmeleonRedrawnPlugin, PaintStroke?>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;

    public override void Write(MessageWriter writer, PaintStroke? data)
    {
        if (!data.HasValue)
        {
            writer.WritePacked(0U);
            return;
        }

        var stroke = data.Value;

        writer.WritePacked((uint)stroke.Points.Length);
        if (stroke.Points.Length == 0) return;

        writer.Write(stroke.Brush.Color);
        writer.Write(stroke.Brush.Radius);
        writer.Write(stroke.Brush.Opacity);
        writer.Write(stroke.Brush.Hardness);

        foreach (var point in stroke.Points)
        {
            writer.Write((short)point.x);
            writer.Write((short)point.y);
        }
    }

    public override PaintStroke? Read(MessageReader reader)
    {
        var count = reader.ReadPackedUInt32();
        if (count == 0U)
        {
            Logger<CrewmeleonRedrawnPlugin>.Error("Could not read stroke RPC data.");
            return null;
        }

        var brush = new BrushStamp(
            reader.ReadColor32(),
            reader.ReadByte(),
            reader.ReadByte(),
            reader.ReadByte());

        var points = new Vector2Int[count];
        for (var i = 0; i < count; i++)
        {
            points[i] = new Vector2Int(reader.ReadInt16(), reader.ReadInt16());
        }

        return new PaintStroke(brush, points);
    }

    public override void Handle(PlayerControl innerNetObject, PaintStroke? data)
    {
        if (!data.HasValue)
        {
            Logger<CrewmeleonRedrawnPlugin>.Error("Could not handle stroke RPC. Data is null.");
            return;
        }

        var canvas = innerNetObject.gameObject.GetComponentInChildren<PlayerCanvasComponent>();
        if (canvas == null)
        {
            Logger<CrewmeleonRedrawnPlugin>.Error("Could not handle stroke RPC. Canvas component is null.");
            return;
        }

        canvas.ApplyStroke(data.Value);
    }
}
