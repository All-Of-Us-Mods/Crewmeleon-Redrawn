using TMPro;
using UnityEngine;
using Reactor.Utilities.Extensions;

namespace CrewmeleonRedrawn.Utilities;

public static class HudUtilities
{
    private const float LabelOffsetX = 1.5f;

    public static HideAndSeekTimerBar CreateTimerBar(HudManager hud, Color color, float distanceFromTop, string text, out TextMeshPro label)
    {
        var bar = GameObject.Instantiate(GameManagerCreator.Instance.HideAndSeekManagerPrefab.TimerBarPrefab, hud.transform.parent);
        bar.timerBarRenderer.material.SetColor("_Color", color);

        var aspectPosition = bar.gameObject.GetComponent<AspectPosition>();
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.Top;
        aspectPosition.DistanceFromEdge = new Vector3(0, distanceFromTop, 0);
        aspectPosition.AdjustPosition();

        label = GameObject.Instantiate(bar.timeText, bar.transform);
        label.GetComponent<TextTranslatorTMP>().Destroy();
        label.transform.position += new Vector3(LabelOffsetX, 0, 0);
        label.alignment = TextAlignmentOptions.Right;
        label.text = text;

        return bar;
    }
}
