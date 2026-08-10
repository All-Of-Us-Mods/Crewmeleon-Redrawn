using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;

namespace Crewmeleon_Redrawn.GameMode;

public static class TimerBarFactory
{
    private const float LabelOffsetX = 1.5f;

    public static HideAndSeekTimerBar Create(
        HudManager hud,
        Color color,
        float distanceFromTop,
        string label,
        out TextMeshPro text)
    {
        var bar = GameObject.Instantiate(GameManagerCreator.Instance.HideAndSeekManagerPrefab.TimerBarPrefab, hud.transform.parent);
        bar.timerBarRenderer.material.SetColor("_Color", color);

        var aspectPosition = bar.gameObject.GetComponent<AspectPosition>();
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.Top;
        aspectPosition.DistanceFromEdge = new Vector3(0, distanceFromTop, 0);
        aspectPosition.AdjustPosition();

        text = GameObject.Instantiate(bar.timeText, bar.transform);
        text.GetComponent<TextTranslatorTMP>().Destroy();
        text.transform.position += new Vector3(LabelOffsetX, 0, 0);
        text.alignment = TextAlignmentOptions.Right;
        text.text = label;

        return bar;
    }
}
