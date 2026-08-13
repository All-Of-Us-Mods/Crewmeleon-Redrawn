using System.Collections;
using CrewmeleonRedrawn.Utilities;
using CrewmeleonRedrawn.Modifiers;
using CrewmeleonRedrawn.Roles;
using MiraAPI.GameModes;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using Rewired.Utils;
using TMPro;
using UnityEngine;

namespace CrewmeleonRedrawn.GameMode;

/// <summary>
/// Controls the player tracker as seen on the HUD
/// </summary>
public class PlayerTracker
{
    private static int maxPlayers = 25;
    private static float trackerLength = 1.5f;
    private CrewmatesKilledTracker? _tracker;
    private AspectPosition? _aspectPosition;
    private GridArrange? _gridArrange;
    private TextMeshPro? _hidersLabel;
    private TextMeshPro? _seekersLabel;
    private static List<PlayerControl> allTrackedPlayers = new();

    public void Begin(HudManager hud)
    {
        _tracker = hud.CrewmatesKilled;
        _aspectPosition = _tracker?.gameObject.GetComponent<AspectPosition>();
        _gridArrange = _tracker?.gameObject.AddComponent<GridArrange>();
        _gridArrange!.Alignment = GridArrange.StartAlign.Right;
        _hidersLabel = Helpers.CreateTextLabel("HidersLabel", hud.transform, AspectPosition.EdgeAlignments.Left,
            new Vector3(0.5f, 1.23f, 0), textAlignment: TextAlignmentOptions.Left);
        _hidersLabel.color = Palette.CrewmateBlue;
        _seekersLabel = Helpers.CreateTextLabel("SeekersLabel", hud.transform, AspectPosition.EdgeAlignments.Top,
            new Vector3(1.5f, 1.23f, 0), textAlignment: TextAlignmentOptions.Right);
        _seekersLabel.color = Palette.ImpostorRoleRed;
        allTrackedPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Data && x.Data.Role && !x.Data.Role.IsImpostor).ToList();
    }

    public void Update()
    {
        if (_tracker == null || _aspectPosition == null || _gridArrange == null || _hidersLabel == null ||
            _seekersLabel == null) return;
        _tracker.gameObject.SetActive(true);
        //Count logic
        var alivePlayers = Helpers.GetAlivePlayers();
        var aliveHiders = alivePlayers.Where(x => x.Data && x.Data.Role && x.Data.Role is HiderRole);
        _hidersLabel.text = $"Hiders: {aliveHiders.Count()}";
        _seekersLabel.text = $"Seekers: {alivePlayers.Count(x => x.Data && x.Data.Role && x.Data.Role.IsImpostor)}";
        
        //Positioning logic
        _aspectPosition.DistanceFromEdge = new Vector3(0.23f, 0.23f, 0f);
        _aspectPosition.AdjustPosition();
        
        //Grid logic
        _gridArrange.MaxColumns = maxPlayers / 2;
        _gridArrange.CellSize = new Vector2(trackerLength / _gridArrange.cells.Count, -0.75f);
        _gridArrange.ArrangeChilds();
        
        //Percentage logic
        var deadTrackedCount = allTrackedPlayers.Count(x => !x.IsNullOrDestroyed() && x.Data.IsDead);
        var index = deadTrackedCount - 1;
        if (index >= 0 && index < _tracker.crewmateSprites.Count && _tracker.crewmateSprites[index] != null && !_tracker.crewmateSprites[index].IsKilled)
        {
            _tracker.crewmateSprites[index].SetKilled(_tracker.slashAnimations.ToArray().Random());
        }
    }
}
