using CrewmeleonRedrawn.GameMode;
using CrewmeleonRedrawn.Utilities;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CrewmeleonRedrawn.Modifiers;

public class SpectatingModifier : BaseModifier
{
    public override string ModifierName => "Spectating";
    public override bool HideOnUi => true;

    private int _targetIndex;
    private bool _wasMoveable;
    private GameObject? _spectateControls;

    public override void OnActivate()
    {
        if (!Player.AmOwner)
            return;

        _targetIndex = 0;
        CreateControls();

        var target = GetSpectateTargets()[_targetIndex];
        SnapCamToTarget(target);

        HudManager.Instance.ShadowQuad.enabled = false;

        _wasMoveable = Player.moveable;
        Player.DisableMovement();

        CustomButtonUtilities.RefreshActionButtonsDeferred(Player);
    }

    public override void OnDeactivate()
    {
        if (!Player.AmOwner)
            return;

        if (_spectateControls is not null && _spectateControls)
            _spectateControls.gameObject.Destroy();

        HudManager.Instance.ShadowQuad.enabled = true;
        HudManager.Instance.PlayerCam.Target = Player;

        Player.moveable = _wasMoveable;

        CustomButtonUtilities.RefreshActionButtonsDeferred(Player);
    }

    private void CreateControls()
    {
        // create the spectate controls container
        _spectateControls = new GameObject("SpectateControls");
        _spectateControls.transform.SetParent(HudManager.Instance.transform);

        var controlsAspectPos = _spectateControls.AddComponent<AspectPosition>();
        controlsAspectPos.Alignment = AspectPosition.EdgeAlignments.Bottom;
        controlsAspectPos.DistanceFromEdge = new Vector3(0, 0.75f, 0);
        controlsAspectPos.AdjustPosition();
        
        // create the target player name label
        var label = Helpers.CreateTextLabel("TargetName", _spectateControls.transform, AspectPosition.EdgeAlignments.Bottom, Vector3.down, 4);
        label.text = GetSpectateTargets()[_targetIndex].Data.PlayerName;
        label.GetComponent<AspectPosition>().Destroy();
        label.transform.localPosition = Vector3.zero;
        
        // create the spectate next player button
        var nextButton = new GameObject("NextButton").AddComponent<PassiveButton>();
        nextButton.transform.SetParent(_spectateControls.transform);
        nextButton.gameObject.layer = LayerMask.NameToLayer("UI");

        nextButton.OnClick = new Button.ButtonClickedEvent();
        nextButton.OnClick.AddListener((UnityAction) (() => SpectateNextTarget(label)));

        var boxCollider2D = nextButton.gameObject.AddComponent<BoxCollider2D>();
        nextButton.ClickMask = boxCollider2D;
        nextButton.Colliders = new[] { boxCollider2D };
        boxCollider2D.size = new Vector2(1f, 1f);
        nextButton.OnMouseOver = new();
        nextButton.OnMouseOut = new();
        nextButton.activeSprites = new GameObject("Active");
        nextButton.activeSprites.transform.SetParent(nextButton.transform);
        nextButton.activeSprites.transform.localEulerAngles = Vector3.zero;

        var spriteRenderer = nextButton.activeSprites.AddComponent<SpriteRenderer>();
        spriteRenderer.color = MiraAssets.AcceptedTeal;
        spriteRenderer.sprite = MiraAssets.NextButton.LoadAsset();
        nextButton.activeSprites.layer = LayerMask.NameToLayer("UI");
        nextButton.inactiveSprites = new GameObject("Inactive");
        nextButton.inactiveSprites.transform.SetParent(nextButton.transform);
        nextButton.inactiveSprites.transform.localEulerAngles = Vector3.zero;
        nextButton.inactiveSprites.AddComponent<SpriteRenderer>().sprite = MiraAssets.NextButton.LoadAsset();
        nextButton.inactiveSprites.layer = LayerMask.NameToLayer("UI");
        nextButton.transform.position = label.transform.position + new Vector3(2, 0, 0);
        
        // copy the next player button to create the previous player button
        var prevButton = GameObject.Instantiate(nextButton, _spectateControls.transform, true);
        prevButton.OnClick = new Button.ButtonClickedEvent();
        prevButton.OnClick.AddListener((UnityAction)(() => SpectatePreviousTarget(label)));
        prevButton.transform.position = label.transform.position - new Vector3(2, 0, 0);
        prevButton.activeSprites.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        prevButton.inactiveSprites.transform.localEulerAngles = new Vector3(0f, 180f, 0f);

        _spectateControls.transform.localScale = Vector3.one * 0.7f;
    }

    private void SpectateNextTarget(TextMeshPro targetLabel)
    {
        var targets = GetSpectateTargets();

        if (++_targetIndex >= targets.Count)
            _targetIndex = 0;

        targetLabel.text = targets[_targetIndex].Data.PlayerName;
        SnapCamToTarget(targets[_targetIndex]);
    }

    private void SpectatePreviousTarget(TextMeshPro targetLabel)
    {
        var targets = GetSpectateTargets();

        if (--_targetIndex < 0)
            _targetIndex = targets.Count - 1;

        targetLabel.text = targets[_targetIndex].Data.PlayerName;
        SnapCamToTarget(targets[_targetIndex]);
    }

    private static void SnapCamToTarget(PlayerControl target)
    {
        HudManager.Instance.PlayerCam.Target = target;
        HudManager.Instance.PlayerCam.SnapToTarget();
    }

    public static List<PlayerControl> GetSpectateTargets()
    {
        var players = Helpers.GetAlivePlayers();

        if (ChameleonOptions.Spectating.SpectateHiders || CustomButtonUtilities.IsInPractice())
            return players;

        return players.Where(p => p.Data.Role.IsImpostor).ToList();
    }

    public override void OnDeath(DeathReason reason)
    {
        _wasMoveable = true;
        ModifierComponent?.RemoveModifier(this);
    }
}