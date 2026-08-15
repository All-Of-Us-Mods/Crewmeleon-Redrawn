using CrewmeleonRedrawn.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using Rewired.Utils;
using TMPro;
using UnityEngine;
using Action = Il2CppSystem.Action;
using Object = System.Object;

namespace CrewmeleonRedrawn.GameMode;

/// <summary>
/// Controls the player tracker as seen on the HUD
/// </summary>
public class PlayerTracker
{
    private const int MaxPlayers = 25;
    private const float TrackerLength = 1.5f;
    private CrewmatesKilledTracker _tracker = null!;
    private AspectPosition _aspectPosition = null!;
    private GridArrange _gridArrange = null!;
    private TextMeshPro _hidersLabel = null!;
    private TextMeshPro _seekersLabel = null!;
    private List<PlayerControl> _allTrackedPlayers = [];

    public void Begin(HudManager hud)
    {
        _tracker = hud.CrewmatesKilled;
        _aspectPosition = _tracker.gameObject.GetComponent<AspectPosition>();
        _gridArrange = _tracker.gameObject.AddComponent<GridArrange>();
        _gridArrange!.Alignment = GridArrange.StartAlign.Right;
        _hidersLabel = UnityEngine.Object.Instantiate(hud.KillButton.buttonLabelText, hud.transform);
        _hidersLabel.GetComponent<TextTranslatorTMP>()?.Destroy();
        _hidersLabel.color = Color.white;
        _hidersLabel.SetOutlineColor(Palette.CrewmateBlue);
        _seekersLabel = UnityEngine.Object.Instantiate(hud.KillButton.buttonLabelText, hud.transform);
        _seekersLabel.GetComponent<TextTranslatorTMP>()?.Destroy();
        _seekersLabel.color = Color.white;
        _seekersLabel.SetOutlineColor(Palette.ImpostorRoleRed);
        _allTrackedPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Data && x.Data.Role && !x.Data.Role.IsImpostor).ToList();
        _tracker.StartCoroutine(Effects.ActionAfterDelay(0.05f, new System.Action(() =>
        {
            float z = 0;
            foreach (var sprite in _gridArrange.cells)
            {
                sprite.transform.localPosition += new Vector3(0, 0, z);
                z -= 0.1f;
            }
        })));
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
        _hidersLabel.transform.position = new Vector3(_tracker.gameObject.transform.position.x + .75f, _tracker.crewmateSprites.ToArray().Last().transform.position.y - 1.5f, -50);
        _seekersLabel.transform.position = new Vector3(_tracker.gameObject.transform.position.x + .75f, _tracker.crewmateSprites.ToArray().Last().transform.position.y - 1, -50);
        
        //Positioning logic
        _aspectPosition.DistanceFromEdge = new Vector3(0.25f, 0.35f, 0f);
        _aspectPosition.AdjustPosition();
        
        //Grid logic
        _gridArrange.MaxColumns = Math.Clamp(MaxPlayers / 2, 0, 8);
        _gridArrange.CellSize = new Vector2(Math.Clamp(TrackerLength / _gridArrange.cells.Count, 0.25f, 0.4f), Math.Clamp(TrackerLength / _gridArrange.cells.Count, 0.25f, 0.75f) / -2f);
        _gridArrange.ArrangeChilds();
        
        //Percentage logic
        var deadTrackedCount = _allTrackedPlayers.Count(x => !x.IsNullOrDestroyed() && x.Data.IsDead);
        var index = deadTrackedCount - 1;
        if (index >= 0 && index < _tracker.crewmateSprites.Count && _tracker.crewmateSprites[index] != null && !_tracker.crewmateSprites[index].IsKilled)
        {
            _tracker.crewmateSprites[index].SetKilled(_tracker.slashAnimations.ToArray().Random());
        }
    }

    public void OnInfected()
    {
        var firstEntry = _tracker.crewmateSprites.ToArray().First();
        _tracker.crewmateSprites.Remove(firstEntry);
        firstEntry.gameObject.Destroy();
    }
}
