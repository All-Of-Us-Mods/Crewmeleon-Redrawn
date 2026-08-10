using UnityEngine;

namespace Crewmeleon_Redrawn.Components;

/// <summary>
/// A hand-drawn brush tip: a square grid of alpha values, sampled and scaled to whatever radius
/// the brush is set to.
/// </summary>
public sealed class BrushMask
{
    public const int Size = 31;

    private const byte ModeRaw = 0;
    private const byte ModeRle = 1;

    public byte[] Cells { get; }

    public BrushMask() => Cells = new byte[Size * Size];

    private BrushMask(byte[] cells) => Cells = cells;

    public bool IsEmpty => Cells.All(c => c == 0);

    /// <summary>Alpha at an offset expressed as -1..1 across the tip.</summary>
    public float Sample(float nx, float ny)
    {
        var x = Mathf.RoundToInt((nx + 1f) * 0.5f * (Size - 1));
        var y = Mathf.RoundToInt((ny + 1f) * 0.5f * (Size - 1));

        if (x < 0 || x >= Size || y < 0 || y >= Size) return 0f;

        return Cells[y * Size + x] / 255f;
    }

    public BrushMask Clone() => new((byte[]) Cells.Clone());

    /// <summary>
    /// Runs beat raw for typical brushes (large solid and empty areas) but lose on noise, so the
    /// smaller of the two wins and a leading byte says which was used.
    /// </summary>
    public byte[] Encode()
    {
        var rle = new List<byte> { ModeRle };

        var index = 0;
        while (index < Cells.Length)
        {
            var value = Cells[index];
            var run = 1;
            while (index + run < Cells.Length && Cells[index + run] == value && run < 255) run++;

            rle.Add(value);
            rle.Add((byte) run);
            index += run;
        }

        if (rle.Count >= Cells.Length + 1)
        {
            var raw = new byte[Cells.Length + 1];
            raw[0] = ModeRaw;
            Cells.CopyTo(raw, 1);
            return raw;
        }

        return rle.ToArray();
    }

    public static BrushMask? Decode(byte[] data)
    {
        if (data.Length < 1) return null;

        var cells = new byte[Size * Size];

        if (data[0] == ModeRaw)
        {
            if (data.Length != cells.Length + 1) return null;
            Array.Copy(data, 1, cells, 0, cells.Length);
            return new BrushMask(cells);
        }

        var write = 0;
        for (var i = 1; i + 1 < data.Length; i += 2)
        {
            var value = data[i];
            var run = data[i + 1];

            for (var r = 0; r < run && write < cells.Length; r++) cells[write++] = value;
        }

        return write == cells.Length ? new BrushMask(cells) : null;
    }

    public string ToBase64() => Convert.ToBase64String(Encode());

    public static BrushMask? FromBase64(string value)
    {
        try
        {
            return Decode(Convert.FromBase64String(value));
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
