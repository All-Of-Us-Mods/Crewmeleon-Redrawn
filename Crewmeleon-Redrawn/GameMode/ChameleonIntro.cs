using System.Collections;
using CrewmeleonRedrawn.Utilities;
using MiraAPI.GameModes;
using MiraAPI.Utilities;
using PowerTools;
using Reactor.Utilities;
using UnityEngine;

namespace CrewmeleonRedrawn.GameMode;

/// <summary>
/// Replaces the vanilla intro cutscene with the hide and seek seeker/hider sequence.
/// </summary>
public static class ChameleonIntro
{
    public static IEnumerator Play(IntroCutscene intro)
    {
        SoundUtilities.Play(intro.IntroStinger);
        Logger.GlobalInstance.Info("IntroCutscene :: CoBegin() :: Game Mode: Hide and Seek (MiraAPI)", null);

        intro.LogPlayerRoleData();
        intro.HideAndSeekPanels.SetActive(true);

        intro.CrewmateRules.SetActive(!ChameleonGameMode.AmImpostor);
        intro.ImpostorRules.SetActive(ChameleonGameMode.AmImpostor);

        intro.ImpostorName.gameObject.SetActive(true);
        intro.ImpostorTitle.gameObject.SetActive(true);
        intro.TeamTitle.gameObject.SetActive(false);
        intro.BackgroundBar.enabled = false;

        var impostor = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.Data.Role.IsImpostor);

        if (impostor == null)
            Logger.GlobalInstance.Error("IntroCutscene :: CoBegin() :: impostor is NULL", null);

        GameManager.Instance.SetSpecialCosmetics(impostor);
        intro.ImpostorName.text = impostor != null ? impostor.Data.PlayerName : "???";

        yield return new WaitForSecondsRealtime(0.1f);

        PoolablePlayer? playerSlot = null;

        if (impostor != null)
        {
            intro.ImpostorTitle.text = impostor.Data.Role.GetRoleName();

            playerSlot = intro.CreatePlayer(1, 1, impostor.Data, false);
            playerSlot.SetBodyType(PlayerBodyTypes.Normal);
            playerSlot.SetFlipX(false);
            playerSlot.transform.localPosition = intro.impostorPos;
            playerSlot.transform.localScale = Vector3.one * intro.impostorScale;
        }

        yield return ShipStatus.Instance.CosmeticsCache.PopulateFromPlayers();
        yield return new WaitForSecondsRealtime(6f);

        if (playerSlot != null)
            playerSlot.gameObject.SetActive(false);

        intro.HideAndSeekPanels.SetActive(false);
        intro.CrewmateRules.SetActive(false);
        intro.ImpostorRules.SetActive(false);

        var hideTimer = ChameleonOptions.Gameplay.HideTime.Value;
        
        if (ChameleonGameMode.AmImpostor)
            yield return PlaySeekerIntro(intro, hideTimer);
        else
        {
            PlayHiderIntro(intro, impostor, hideTimer);
            PlayerControl.LocalPlayer.moveable = true;
        }
        ShipStatus.Instance.StartSFX();
        UnityEngine.Object.Destroy(intro.gameObject); // warning: this causes intro end events to never fire on mobile
        ChameleonGameModeManager.Create();
        CustomButtonUtilities.RefreshVisibilityDeferred();
    }

    private static IEnumerator PlaySeekerIntro(IntroCutscene intro, float hideTimer)
    {
        intro.HideAndSeekTimerText.gameObject.SetActive(true);
        
        var (poolablePlayer, anim) = GetSeekerVisual(intro);

        poolablePlayer.SetBodyCosmeticsVisible(false);
        poolablePlayer.UpdateFromPlayerData(
            PlayerControl.LocalPlayer.Data,
            PlayerControl.LocalPlayer.CurrentOutfitType,
            PlayerMaterial.MaskType.None,
            false,
            null,
            false);

        poolablePlayer.gameObject.SetActive(true);
        poolablePlayer.ToggleName(false);
        var spriteAnim = poolablePlayer.GetComponent<SpriteAnim>();
        spriteAnim.Play(anim, 1f);
        spriteAnim.SetTime(5);
        spriteAnim.Pause();

        while (hideTimer > 0f)
        {
            intro.HideAndSeekTimerText.text = Mathf.RoundToInt(hideTimer).ToString();
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 5f && spriteAnim.Paused)
            {
                spriteAnim.Resume();
            }
            ChameleonGameModeManager.Instance?.HudUpdate(HudManager.Instance);
            yield return null;
        }
        if (CustomGameModeManager.ActiveMode is not ChameleonGameMode c) yield break;
    }

    private static (PoolablePlayer Visual, AnimationClip Anim) GetSeekerVisual(IntroCutscene intro)
    {
        if (AprilFoolsMode.ShouldHorseAround())
        {
            var suit = intro.HorseWrangleVisualSuit;
            suit.gameObject.SetActive(true);
            suit.SetBodyType(PlayerBodyTypes.Seeker);

            intro.HorseWrangleVisualPlayer.SetBodyType(PlayerBodyTypes.Normal);
            intro.HorseWrangleVisualPlayer.UpdateFromPlayerData(
                PlayerControl.LocalPlayer.Data,
                PlayerControl.LocalPlayer.CurrentOutfitType,
                PlayerMaterial.MaskType.None,
                false,
                null,
                false);

            return (suit, intro.HnSSeekerSpawnHorseAnim);
        }

        var visual = intro.HideAndSeekPlayerVisual;
        visual.gameObject.SetActive(true);

        if (AprilFoolsMode.ShouldLongAround())
        {
            visual.SetBodyType(PlayerBodyTypes.LongSeeker);
            return (visual, intro.HnSSeekerSpawnLongAnim);
        }

        visual.SetBodyType(PlayerBodyTypes.Seeker);
        return (visual, intro.HnSSeekerSpawnAnim);
    }

    private static void PlayHiderIntro(IntroCutscene intro, PlayerControl? impostor, float hideTimer)
    {
        ShipStatus.Instance.HideCountdown = hideTimer;

        if (impostor == null)
            return;

        if (AprilFoolsMode.ShouldHorseAround())
        {
            impostor.AnimateCustom(intro.HnSSeekerSpawnHorseInGameAnim);
        }
        else if (AprilFoolsMode.ShouldLongAround())
        {
            impostor.AnimateCustom(intro.HnSSeekerSpawnLongInGameAnim);
        }
        else
        {
            impostor.AnimateCustom(intro.HnSSeekerSpawnAnim);
            impostor.cosmetics.SetBodyCosmeticsVisible(false);
        }

        Coroutines.Start(CoPauseSeekerAnim(impostor.MyPhysics.Animations.Animator));
    }

    private static IEnumerator CoPauseSeekerAnim(SpriteAnim animator)
    {
        var timer = ChameleonGameModeManager.Instance?.Timer;

        animator.SetTime(5);
        animator.Pause();
        
        while (timer?.GetTimeLeft() >= 5f)
        {
            yield return null;
        }
        animator.Resume();
    }
}
