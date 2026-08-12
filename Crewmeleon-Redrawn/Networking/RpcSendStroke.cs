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
        writer.Write(data.IsFirst);
        writer.Write(data.IsFinal);

        // brush only goes in the first chunk, no point repeating it
        if (data.IsFirst)
        {
            writer.Write(data.Brush.Color);
            writer.Write(data.Brush.Radius);
            writer.Write(data.Brush.Opacity);
            writer.Write(data.Brush.Hardness);
        }

        writer.WritePacked((uint)data.Points.Length);
        foreach (var point in data.Points)
        {
            WriteInt16(writer, point.x);
            WriteInt16(writer, point.y);
        }
    }

    public override StrokeChunk Read(MessageReader reader)
    {
        var isFirst = reader.ReadBoolean();
        var isFinal = reader.ReadBoolean();

        var brush = default(BrushStamp);
        if (isFirst)
        {
            brush = new BrushStamp(
                reader.ReadColor32(),
                reader.ReadByte(),
                reader.ReadByte(),
                reader.ReadByte());
        }

        var count = reader.ReadPackedUInt32();
        var points = new Vector2Int[count];
        for (var i = 0; i < count; i++)
        {
            points[i] = new Vector2Int(ReadInt16(reader), ReadInt16(reader));
        }

        return new StrokeChunk(isFirst, isFinal, brush, points);
    }

    // byte by byte on purpose, writer.Write((short)v) picked the int overload and wrote 4 bytes
    // while the reader took 2 so every point after the first came out garbage
    private static void WriteInt16(MessageWriter writer, int value)
    {
        writer.Write((byte)(value & 0xFF));
        writer.Write((byte)((value >> 8) & 0xFF));
    }

    private static int ReadInt16(MessageReader reader)
    {
        int low = reader.ReadByte();
        int high = reader.ReadByte();
        return (short)(low | (high << 8));
    }

    public override void Handle(PlayerControl innerNetObject, StrokeChunk data)
    {
        // includeInactive, the canvas object is inactive until youre a hider
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
