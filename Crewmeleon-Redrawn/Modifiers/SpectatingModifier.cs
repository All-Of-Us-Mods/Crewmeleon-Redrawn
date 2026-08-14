using System.Collections;
using CrewmeleonRedrawn.GameMode;
using CrewmeleonRedrawn.Utilities;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
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
    private GameObject? _spectateControls;
    private bool _wasMoveable;

    public override void OnActivate()
    {
        if (!Player.AmOwner) return;
        Coroutines.Start(CoBegin());
        HudManager.Instance.ShadowQuad.enabled = false;

        _wasMoveable = Player.moveable;
        Player.DisableMovement();
    }

    public override void OnDeactivate()
    {
        if (!Player.AmOwner) return;
        Coroutines.Start(CoEnd());
        HudManager.Instance.ShadowQuad.enabled = true;
        Player.moveable = _wasMoveable;

        HudManager.Instance.PlayerCam.Target = Player;
    }

    private IEnumerator CoBegin()
    {
        var buttonsParent = HudManager.Instance.transform.FindChild("Buttons");
        var bottomRight = buttonsParent.FindChild("BottomRight");
        var bottomLeft = buttonsParent.FindChild("BottomLeft");

        HudManager.Instance.StartCoroutine(Effects.Slide2D(bottomRight, bottomRight.transform.localPosition, bottomRight.transform.localPosition + Vector3.down * 10, 0.75f));
        HudManager.Instance.StartCoroutine(Effects.Slide2D(bottomLeft, bottomLeft.transform.localPosition, bottomLeft.transform.localPosition + Vector3.down * 10, 0.75f));
        
        yield return HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.clear, Color.black, 0.5f));
        
        _targetIndex = 0;
        CreateControls();

        var target = GetSpectateTargets()[_targetIndex];
        SnapCamToTarget(target);

        yield return HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.black, Color.clear, 0.5f));
    }

    private IEnumerator CoEnd()
    {
        yield return HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.clear, Color.black, 0.3f));
        
        if(_spectateControls is not null && _spectateControls)
            _spectateControls.gameObject.Destroy();

        yield return HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.black, Color.clear, 0.2f));
        
        var buttonsParent = HudManager.Instance.transform.FindChild("Buttons");
        var bottomRight = buttonsParent.FindChild("BottomRight");
        var bottomLeft = buttonsParent.FindChild("BottomLeft");

        HudManager.Instance.StartCoroutine(Effects.Slide2D(bottomRight, bottomRight.transform.localPosition, bottomRight.transform.localPosition - Vector3.down * 10, 0.75f));
        HudManager.Instance.StartCoroutine(Effects.Slide2D(bottomLeft, bottomLeft.transform.localPosition, bottomLeft.transform.localPosition - Vector3.down * 10, 0.75f));
    }

    private void CreateControls()
    {
        // create the spectate controls container
        _spectateControls = new GameObject("SpectateControls");
        _spectateControls.transform.SetParent(HudManager.Instance.transform);

        var controlsAspectPos = _spectateControls.AddComponent<AspectPosition>();
        controlsAspectPos.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
        controlsAspectPos.DistanceFromEdge = new Vector3(2.5f, 0.75f, 0);
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

        _spectateControls.transform.localScale = Vector3.one * 0.7f;
        
        // create the stop spectating button
        var stopButton = GameObject.Instantiate(HudManager.Instance.GameMenu.Tabs[0].Content.transform.FindChild("LeaveGameButton"), _spectateControls.transform).GetComponent<PassiveButton>();
        stopButton.OnClick = new Button.ButtonClickedEvent();
        stopButton.OnClick.AddListener((UnityAction)(() => Player.RpcRemoveModifier<SpectatingModifier>()));

        var stopBtnAspectPos = stopButton.gameObject.AddComponent<AspectPosition>();
        stopBtnAspectPos.Alignment = AspectPosition.EdgeAlignments.Right;
        stopBtnAspectPos.DistanceFromEdge = new Vector3(-3.5f, 0, 0);
        stopBtnAspectPos.AdjustPosition();

        HudManager.Instance.StartCoroutine(Effects.ActionAfterDelay(0.05f, new System.Action(() =>
        {
            stopButton.transform.GetChild(1).GetComponent<TextMeshPro>().text = "Stop Spectating";
        })));
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

        if (ChameleonOptions.Spectating.SpectateHiders)
            return players;

        return players.Where(p => p.Data.Role.IsImpostor).ToList();
    }
}