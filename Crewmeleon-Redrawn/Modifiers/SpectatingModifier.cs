using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = System.Object;

namespace Crewmeleon_Redrawn.Modifiers;

public class SpectatingModifier : BaseModifier
{
    public override string ModifierName => "Spectating";
    public override bool HideOnUi => true;
    private int spectatingIndex = 0;
    public override void OnActivate()
    {
        Coroutines.Start(CoBegin());
        HudManager.Instance.ShadowQuad.enabled = false;
        Player.moveable = false;
    }

    public override void OnDeactivate()
    {
        Coroutines.Start(CoEnd());
        HudManager.Instance.ShadowQuad.enabled = true;
        Player.moveable = true;
        HudManager.Instance.PlayerCam.Target = Player;
    }

    public GameObject SpectateControls;

    public static List<PlayerControl> GetSpectateTargets()
    {
        if (!OptionGroupSingleton<SpectatingOptions>.Instance.SpectateHiders)
        {
            return PlayerControl.AllPlayerControls.ToArray().Where(x => x.Data.Role.IsImpostor).ToList();
        }
        return PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Data.IsDead).ToList();
    }

    private IEnumerator CoBegin()
    {
        var buttonsParent = HudManager.Instance.transform.FindChild("Buttons");
        var bottomRight = buttonsParent.FindChild("BottomRight");
        HudManager.Instance.StartCoroutine(Effects.Slide2D(bottomRight, bottomRight.transform.localPosition, bottomRight.transform.localPosition + Vector3.down * 10, 0.75f));
        var bottomLeft = buttonsParent.FindChild("BottomLeft");
        HudManager.Instance.StartCoroutine(Effects.Slide2D(bottomLeft, bottomLeft.transform.localPosition, bottomLeft.transform.localPosition + Vector3.down * 10, 0.75f));
        yield return HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.clear, Color.black, 0.5f));
        HudManager.Instance.PlayerCam.Target = GetSpectateTargets()[0];
        HudManager.Instance.PlayerCam.transform.position = HudManager.Instance.PlayerCam.Target.transform.position;
        spectatingIndex = 0;
        CreateControls();
        yield return HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.black, Color.clear, 0.5f));
    }
    private IEnumerator CoEnd()
    {
        yield return HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.clear, Color.black, 0.3f));
        SpectateControls.gameObject.Destroy();
        yield return HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.black, Color.clear, 0.2f));
        var buttonsParent = HudManager.Instance.transform.FindChild("Buttons");
        var bottomRight = buttonsParent.FindChild("BottomRight");
        HudManager.Instance.StartCoroutine(Effects.Slide2D(bottomRight, bottomRight.transform.localPosition, bottomRight.transform.localPosition - Vector3.down * 10, 0.75f));
        var bottomLeft = buttonsParent.FindChild("BottomLeft");
        HudManager.Instance.StartCoroutine(Effects.Slide2D(bottomLeft, bottomLeft.transform.localPosition, bottomLeft.transform.localPosition - Vector3.down * 10, 0.75f));
    }

    private void CreateControls()
    {
        SpectateControls = new GameObject("SpectateControls");
        SpectateControls.transform.SetParent(HudManager.Instance.transform);
        var aspectPos = SpectateControls.AddComponent<AspectPosition>();
        aspectPos.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
        aspectPos.DistanceFromEdge = new Vector3(2.5f, 0.75f, 0);
        aspectPos.AdjustPosition();
        
        var label = Helpers.CreateTextLabel("TargetName", SpectateControls.transform, AspectPosition.EdgeAlignments.Bottom, Vector3.down, 4);
        label.text = GetSpectateTargets()[spectatingIndex].Data.PlayerName;
        label.GetComponent<AspectPosition>().Destroy();
        label.transform.localPosition = Vector3.zero;
        
        var nextButton = new GameObject("NextButton").AddComponent<PassiveButton>();
        nextButton.transform.SetParent(SpectateControls.transform);
        nextButton.gameObject.layer = LayerMask.NameToLayer("UI");
        nextButton.OnClick = new Button.ButtonClickedEvent();
        nextButton.OnClick.AddListener((UnityAction) new System.Action(() =>
        {
            spectatingIndex++;
            var spectateTargets = GetSpectateTargets();
            if (spectatingIndex >= spectateTargets.Count) spectatingIndex = 0;
            label.text = spectateTargets[spectatingIndex].Data.PlayerName;
            HudManager.Instance.PlayerCam.Target = spectateTargets[spectatingIndex];
            HudManager.Instance.PlayerCam.SnapToTarget();
        }));
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
        
        var prevButton = UnityEngine.Object.Instantiate(nextButton, SpectateControls.transform, true);
        prevButton.OnClick = new Button.ButtonClickedEvent();
        prevButton.OnClick.AddListener((UnityAction) new System.Action(() =>
        {
            spectatingIndex++;
            var spectateTargets = GetSpectateTargets();
            if (spectatingIndex >= spectateTargets.Count) spectatingIndex = 0;
            label.text = spectateTargets[spectatingIndex].Data.PlayerName;
            HudManager.Instance.PlayerCam.Target = spectateTargets[spectatingIndex];
            HudManager.Instance.PlayerCam.transform.position = HudManager.Instance.PlayerCam.Target.transform.position;
        }));
        prevButton.transform.position = label.transform.position - new Vector3(2, 0, 0);
        
        SpectateControls.transform.localScale = Vector3.one * 0.7f;
        
        var stopButton = UnityEngine.Object.Instantiate(HudManager.Instance.GameMenu.Tabs[0].Content.transform.FindChild("LeaveGameButton"), SpectateControls.transform).GetComponent<PassiveButton>();
        stopButton.OnClick = new Button.ButtonClickedEvent();
        stopButton.OnClick.AddListener((UnityAction)new System.Action(() =>
        {
            Player.RemoveModifier(this);
        }));
        var aspectPosition = stopButton.gameObject.AddComponent<AspectPosition>();
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.Right;
        aspectPosition.DistanceFromEdge = new Vector3(-3.5f, 0, 0);
        aspectPosition.AdjustPosition();
        HudManager.Instance.StartCoroutine(Effects.ActionAfterDelay(0.05f, new System.Action(() =>
        {
            stopButton.transform.GetChild(1).GetComponent<TextMeshPro>().text = "Stop Spectating";
        })));
    }
}