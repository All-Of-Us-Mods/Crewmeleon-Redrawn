using UnityEngine;

namespace Crewmeleon_Redrawn.GameMode;

public class ChameleonPlayerTracker
{
    private GridArrange? _gridArrange;
    private AspectPosition? _aspectPosition;
    private CrewmatesKilledTracker _tracker;
    public void Begin(HudManager hud)
    {
        _tracker = hud.CrewmatesKilled;
        _tracker.gameObject.SetActive(true);
        _gridArrange = _tracker.gameObject.AddComponent<GridArrange>();
        _gridArrange.Alignment = GridArrange.StartAlign.Right;
        _gridArrange.CellSize = new Vector3(0.5f, -0.5f, 0);
        _gridArrange.MaxColumns = 5;
        _gridArrange.ArrangeChilds();
        _aspectPosition = 
            _tracker.gameObject.GetComponent<AspectPosition>();
        _aspectPosition.DistanceFromEdge = new Vector3(0.23f, 0.35f, 0);
        _aspectPosition.AdjustPosition();
    }
    public void Update()
    {
        if (_aspectPosition == null || _gridArrange == null || _tracker == null) return;
        _tracker.gameObject.SetActive(true);
        _gridArrange.Alignment = GridArrange.StartAlign.Right;
        _gridArrange.CellSize = new Vector3(0.5f, -0.5f, 0);
        _gridArrange.MaxColumns = 5;
        _gridArrange.ArrangeChilds();
        _aspectPosition.DistanceFromEdge = new Vector3(0.23f, 0.35f, 0);
        _aspectPosition.AdjustPosition();
    }
}