using Crewmeleon_Redrawn.Modifiers;
using MiraAPI.Modifiers;
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
        _aspectPosition = 
            _tracker.gameObject.GetComponent<AspectPosition>();
        _aspectPosition.DistanceFromEdge = new Vector3(0.23f, 0.35f, 0);
        _aspectPosition.AdjustPosition();
        _tracker.gameObject.SetActive(true);
        _gridArrange = _tracker.gameObject.AddComponent<GridArrange>();
        _gridArrange.Alignment = GridArrange.StartAlign.Right;
        _gridArrange.CellSize = new Vector3(0.5f, -0.5f, 0);
        _gridArrange.MaxColumns = 5;
        _gridArrange.ArrangeChilds();
    }
    public void Update()
    {
        if (_aspectPosition == null || _gridArrange == null || _tracker == null) return;
        _tracker.gameObject.SetActive(true);
        _gridArrange.Alignment = GridArrange.StartAlign.Right;
        _gridArrange.MaxColumns = _gridArrange.cells.Count;
        _gridArrange.CellSize = new Vector3(2f / _gridArrange.cells.Count, 1, 0);
        _gridArrange.ArrangeChilds();
        _aspectPosition.DistanceFromEdge = new Vector3(0.23f, 0.35f, 0);
        _aspectPosition.AdjustPosition();
    }

    public void OnCrewmateKilled()
    {
        _tracker.OnCrewmateKilled();
    }

    public void OnSpectate(PlayerControl player)
    {
        int numSpectators = PlayerControl.AllPlayerControls.ToArray().Count(pc => pc.Data?.Role != null && !pc.Data.Role.IsImpostor && pc.HasModifier<SpectatingModifier>());
        int numDead = PlayerControl.AllPlayerControls.ToArray().Count(pc => pc.Data?.Role != null && !pc.Data.Role.IsImpostor && pc.Data.IsDead);
        for (int i = 0; i < numSpectators; ++i)
        {
            int index = i + numDead;
            if (_tracker.crewmateSprites[index] == null || _tracker.crewmateSprites[index].IsKilled) continue;
            _tracker.crewmateSprites[index].transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.cyan;
        }
    }
}