using CrewmeleonRedrawn.GameMode;
using CrewmeleonRedrawn.Modifiers;
using CrewmeleonRedrawn.Roles;
using CrewmeleonRedrawn.Utilities;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using System.Collections;
using UnityEngine;

namespace CrewmeleonRedrawn.Buttons.Hider;

public class SpectateButton : CustomActionButton
{
    private const string SpectateText = "Spectate";
    private const string StopSpectatingText = "Close";

    public override string Name => SpectateText;
    public override float Cooldown => 1;
    public override LoadableAsset<Sprite> Sprite => CrewmeleonAssets.SpectateButton;

    public override ButtonLocation Location =>
        CrewmeleonRedrawnPlugin.IsMobile ? ButtonLocation.BottomRight : ButtonLocation.BottomLeft;

    private bool _canSpectate = false;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        _canSpectate = SpectatingModifier.GetSpectateTargets().Count > 0;
    }

    public override bool CanUse() => _canSpectate;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is HiderRole
               && !PlayerControl.LocalPlayer.HasModifier<PaintingModifier>()
               && (ChameleonGameModeManager.Instance is { CurrentStage: not TimerStage.Revelation } ||
                   CustomButtonUtilities.IsInPractice());
    }

    protected override void OnClick()
    {
        if(PlayerControl.LocalPlayer.HasModifier<SpectatingModifier>())
        {
            Coroutines.Start(CoStopSpectating());
        }
        else
        {
            Coroutines.Start(CoBeginSpectating());
        }
    }

    private IEnumerator CoBeginSpectating()
    {
        yield return HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.clear, Color.black, 0.2f));
        yield return new WaitForSeconds(0.05f);

        PlayerControl.LocalPlayer.RpcAddModifier<SpectatingModifier>();
        OverrideName(StopSpectatingText);

        yield return new WaitForSeconds(0.05f);
        yield return HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.black, Color.clear, 0.2f));

        _canSpectate = true;
    }

    private IEnumerator CoStopSpectating()
    {
        yield return HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.clear, Color.black, 0.2f));
        yield return new WaitForSeconds(0.05f);

        PlayerControl.LocalPlayer.RpcRemoveModifier<SpectatingModifier>();
        OverrideName(SpectateText);

        yield return new WaitForSeconds(0.05f);
        yield return HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.black, Color.clear, 0.2f));

        _canSpectate = SpectatingModifier.GetSpectateTargets().Count > 0;
    }
}