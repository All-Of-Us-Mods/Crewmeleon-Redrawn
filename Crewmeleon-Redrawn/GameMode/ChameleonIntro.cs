using System.Collections;
using CrewmeleonRedrawn.Utilities;
using MiraAPI.GameModes;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using PowerTools;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
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

        var impostors = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Data.Role.IsImpostor).ToList();

        if (impostors.Count == 0)
            Logger.GlobalInstance.Error("IntroCutscene :: CoBegin() :: no impostors found", null);

        intro.ImpostorName.text = "SEEKERS";

        yield return new WaitForSecondsRealtime(0.1f);
        
        intro.HideAndSeekPlayerVisual.gameObject.SetActive(false);
        
        int maxDepth = Mathf.CeilToInt(7.5f);
        List<PoolablePlayer> poolablePlayers = [];
        for (int i = 0; i < impostors.Count; i++)
        {
            var impostor = impostors[i];
            var player = intro.CreatePlayer(i, maxDepth, impostor.Data, true);
            poolablePlayers.Add(player);
            player.SetBodyType(PlayerBodyTypes.Seeker);
            player.transform.localPosition -= new Vector3(1.23f, 0f, 27);
        }

        if (ChameleonGameMode.AmImpostor)
        {
        }
        else
        {
            string[] descStrings = ["During hiding, find a good spot and try drawing to blend in!", "It's SEEKING TIME! Spectate the seeker's POV while they search for hiders", "Revelation Time! Check out everyone's hiding spots!" ];
            string[] stageStrings = ["HIDING", "SEEKING", "REVELATION" ];
            for (int i = 0; i < 3; i++)
            {
                var sprite = intro.CrewmateRules.transform.GetChild(i).GetComponent<SpriteRenderer>();
                sprite.sprite = CrewmeleonAssets.CrewmateRules[i].LoadAsset();
                var descText = sprite.transform.FindChild($"P{i + 1}Text");
                descText.GetComponent<TextMeshPro>().text = descStrings[i];
                var headerText = sprite.transform.FindChild($"Rule {i + 1}");
                headerText.GetComponent<TextMeshPro>().text = stageStrings[i];
                //Using findchild because innersloth decided to move the gameobjects around so GetChild(index) can't be accurate, ffs.
            }
            
        }
        yield return ShipStatus.Instance.CosmeticsCache.PopulateFromPlayers();
        yield return new WaitForSeconds(6f);

        intro.HideAndSeekPanels.SetActive(false);
        intro.CrewmateRules.SetActive(false);
        intro.ImpostorRules.SetActive(false);

        var hideTimer = ChameleonOptions.Gameplay.HideTime.Value;
        
        if (ChameleonGameMode.AmImpostor)
        {
            foreach (var poolablePlayer in poolablePlayers)
            {
                poolablePlayer.gameObject.DeepDestroy();
            }
            yield return PlaySeekerIntro(intro, hideTimer);
        }
        else
        {
            PlayHiderIntro(intro, impostors, hideTimer);
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

    private static void PlayHiderIntro(IntroCutscene intro, List<PlayerControl> impostors, float hideTimer)
    {
        ShipStatus.Instance.HideCountdown = hideTimer;

        foreach (var impostor in impostors)
        {
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
