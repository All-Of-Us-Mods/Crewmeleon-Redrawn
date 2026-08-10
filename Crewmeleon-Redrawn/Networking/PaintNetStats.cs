using Reactor.Utilities;

namespace Crewmeleon_Redrawn.Networking;

/// <summary>
/// Counters for the stroke RPC. Temporary instrumentation for diagnosing oversized packets and
/// strokes that never arrive.
/// </summary>
public static class PaintNetStats
{
    public static int SentChunks { get; private set; }
    public static int SentBytes { get; private set; }
    public static int LargestSent { get; private set; }
    public static int SentStrokes { get; private set; }

    public static int RecvChunks { get; private set; }
    public static int RecvBytes { get; private set; }
    public static int LargestRecv { get; private set; }
    public static int RecvStrokes { get; private set; }

    public static int LastStrokeChunks { get; private set; }
    public static int LastStrokeBytes { get; private set; }
    public static int LastStrokePoints { get; private set; }

    private static int strokeChunks;
    private static int strokeBytes;
    private static int strokePoints;

    public static void RecordSent(int bytes, int points, bool isFirst, bool isFinal)
    {
        SentChunks++;
        SentBytes += bytes;
        if (bytes > LargestSent) LargestSent = bytes;

        if (isFirst)
        {
            strokeChunks = 0;
            strokeBytes = 0;
            strokePoints = 0;
        }

        strokeChunks++;
        strokeBytes += bytes;
        strokePoints += points;

        if (!isFinal) return;

        SentStrokes++;
        LastStrokeChunks = strokeChunks;
        LastStrokeBytes = strokeBytes;
        LastStrokePoints = strokePoints;

        Logger<CrewmeleonRedrawnPlugin>.Info(
            $"[net] stroke sent: {strokeChunks} chunk(s), {strokePoints} pts, {strokeBytes} B total, largest {LargestSent} B");
    }

    public static void RecordReceived(int bytes, bool isFinal)
    {
        RecvChunks++;
        RecvBytes += bytes;
        if (bytes > LargestRecv) LargestRecv = bytes;

        if (!isFinal) return;

        RecvStrokes++;
        Logger<CrewmeleonRedrawnPlugin>.Info($"[net] stroke received (total {RecvStrokes}, {RecvBytes} B)");
    }

    public static void Reset()
    {
        SentChunks = SentBytes = LargestSent = SentStrokes = 0;
        RecvChunks = RecvBytes = LargestRecv = RecvStrokes = 0;
        LastStrokeChunks = LastStrokeBytes = LastStrokePoints = 0;
    }
}
