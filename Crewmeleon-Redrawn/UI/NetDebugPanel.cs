using Crewmeleon_Redrawn.Networking;
using ReactUI.Core;
using ReactUI.Hooks;
using ReactUI.Input;
using UnityEngine;
using S = ReactUI.Style;
using static ReactUI.UI;

namespace Crewmeleon_Redrawn.UI;

/// <summary>
/// Live stroke-networking counters, toggled with F3. Temporary instrumentation — the shared
/// BepInEx log interleaves every client, so per-client numbers are easier to read on screen.
/// </summary>
public static class NetDebugPanel
{
    private static readonly Func<VNode> Root = Component(RenderRoot);

    public static VNode Render() => Root();

    private static VNode RenderRoot()
    {
        Scheduler.ScheduleRender(HooksRuntime.Current.ComponentId);

        if (!KeyToggle.Get(KeyCode.F3)) return Div();

        return Div(ClassName("net-panel"),
            Text("STROKE NET  (F3)", ClassName("net-title")),

            Row("sent", $"{PaintNetStats.SentStrokes} strokes / {PaintNetStats.SentChunks} chunks"),
            Row("sent bytes", $"{PaintNetStats.SentBytes} B"),
            Row("largest sent", $"{PaintNetStats.LargestSent} B", PaintNetStats.LargestSent > 900),

            Row("last stroke",
                $"{PaintNetStats.LastStrokePoints} pts / {PaintNetStats.LastStrokeChunks} chunks / {PaintNetStats.LastStrokeBytes} B"),

            Div(ClassName("net-rule")),

            Row("recv", $"{PaintNetStats.RecvStrokes} strokes / {PaintNetStats.RecvChunks} chunks"),
            Row("recv bytes", $"{PaintNetStats.RecvBytes} B"),
            Row("largest recv", $"{PaintNetStats.LargestRecv} B", PaintNetStats.LargestRecv > 900),

            Button("Reset", PaintNetStats.Reset, ClassName("btn btn-small btn-busy"))
        );
    }

    private static VNode Row(string label, string value, bool warn = false)
    {
        return Div(ClassName("net-row"),
            Text(label, ClassName("net-label")),
            Text(value, ClassName(warn ? "net-value net-warn" : "net-value"))
        );
    }
}
