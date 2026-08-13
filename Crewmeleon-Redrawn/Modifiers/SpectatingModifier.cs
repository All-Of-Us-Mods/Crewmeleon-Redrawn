using System.Collections;
using CrewmeleonRedrawn.GameMode;
using MiraAPI.GameModes;
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

    private int targetIndex = 0;

    private GameObject? spectateControls;

    public override void OnActivate()
    {
        if (CustomGameModeManager.ActiveMode is ChameleonGameMode chameleon)
        {
            chameleon.OnBeginSpectate(Player);
        }
        if (!Player.AmOwner) return;
        Coroutines.Start(CoBegin());
        HudManager.Instance.ShadowQuad.enabled = false;
        this.Player.moveable = false;
    }

    public override void OnDeactivate()
    {
        if (CustomGameModeManager.ActiveMode is ChameleonGameMode chameleon)
        {
            chameleon.OnStopSpectate(Player);
        }
        if (!Player.AmOwner) return;
        Coroutines.Start(CoEnd());
        HudManager.Instance.ShadowQuad.enabled = true;
        this.Player.moveable = true;

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
        
        targetIndex = 0;
        CreateControls();

        var target = GetSpectateTargets()[targetIndex];
        SnapCamToTarget(target);

        yield return HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.black, Color.clear, 0.5f));
    }

    private IEnumerator CoEnd()
    {
        yield return HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.clear, Color.black, 0.3f));
        
        if(spectateControls is not null && spectateControls)
            spectateControls.gameObject.Destroy();

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
        spectateControls = new GameObject("SpectateControls");
        spectateControls.transform.SetParent(HudManager.Instance.transform);

        var controlsAspectPos = spectateControls.AddComponent<AspectPosition>();
        controlsAspectPos.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
        controlsAspectPos.DistanceFromEdge = new Vector3(2.5f, 0.75f, 0);
        controlsAspectPos.AdjustPosition();
        
        // create the target player name label
        var label = Helpers.CreateTextLabel("TargetName", spectateControls.transform, AspectPosition.EdgeAlignments.Bottom, Vector3.down, 4);
        label.text = GetSpectateTargets()[targetIndex].Data.PlayerName;
        label.GetComponent<AspectPosition>().Destroy();
        label.transform.localPosition = Vector3.zero;
        
        // create the spectate next player button
        var nextButton = new GameObject("NextButton").AddComponent<PassiveButton>();
        nextButton.transform.SetParent(spectateControls.transform);
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
        var prevButton = GameObject.Instantiate(nextButton, spectateControls.transform, true);
        prevButton.OnClick = new Button.ButtonClickedEvent();
        prevButton.OnClick.AddListener((UnityAction)(() => SpectatePreviousTarget(label)));
        prevButton.transform.position = label.transform.position - new Vector3(2, 0, 0);

        spectateControls.transform.localScale = Vector3.one * 0.7f;
        
        // create the stop spectating button
        var stopButton = GameObject.Instantiate(HudManager.Instance.GameMenu.Tabs[0].Content.transform.FindChild("LeaveGameButton"), spectateControls.transform).GetComponent<PassiveButton>();
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

        if (++targetIndex >= targets.Count)
            targetIndex = 0;

        targetLabel.text = targets[targetIndex].Data.PlayerName;
        SnapCamToTarget(targets[targetIndex]);
    }

    private void SpectatePreviousTarget(TextMeshPro targetLabel)
    {
        var targets = GetSpectateTargets();

        if (--targetIndex < 0)
            targetIndex = targets.Count - 1;

        targetLabel.text = targets[targetIndex].Data.PlayerName;
        SnapCamToTarget(targets[targetIndex]);
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