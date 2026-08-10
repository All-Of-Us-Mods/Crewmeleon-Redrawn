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

        writer.WritePacked((uint)data.Value.Pixels.Length);
        foreach (var pixel in data.Value.Pixels)
        {
            writer.Write((ushort)pixel.x);
            writer.Write((ushort)pixel.y);
        }

        writer.Write(data.Value.Color);
    }

    public override PaintStroke? Read(MessageReader reader)
    {
        var count = reader.ReadPackedUInt32();
        if (count == 0U)
        {
            Logger<CrewmeleonRedrawnPlugin>.Error("Could not read stroke RPC data.");
            return new PaintStroke([], Color.clear);
        }

        var pixels = new Vector2[count];

        for (var i = 0; i < count; i++)
        {
            pixels[i] = new Vector2(reader.ReadUInt16(), reader.ReadUInt16());
        }

        var color = (Color)reader.ReadColor32();

        return new PaintStroke(pixels, color);
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