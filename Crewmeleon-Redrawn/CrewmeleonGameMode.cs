using System.Collections;
using AmongUs.GameOptions;
using Crewmeleon_Redrawn.Modifiers;
using Crewmeleon_Redrawn.Roles;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using PowerTools;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;
using TMPro;

namespace Crewmeleon_Redrawn;

public class ChameleonGameMode : AbstractGameMode
{
    public override string Name => "Crewmeleon";
    public override string Description => "You can run, but you can't hide!";
    public override Color Color { get; } = new Color32(150, 255, 90, 255);

    public override bool ShowGameModeIntroCutscene => false;
    public override bool GameModeBodyTypeOverride => true;
    public override bool ShowNormalGameSettings => false;

    public TimerStage CurrentStage { get; private set; }

    private HideAndSeekTimerBar? timerBar;
    private HideAndSeekTimerBar? tauntBar;
    private TextMeshPro? stageText;

    private float stageTimeLeft;
    private float stageMaxTime;

    private float tauntTimeLeft;
    private float tauntMaxTime;

    private float defaultSeekerSpeed;

    private int deadPlayerCount;

    private static bool AmImpostor => PlayerControl.LocalPlayer.Data.Role.IsImpostor;
    private static bool CanUseChat => ChatOpts.ChatEnabled && (!AmImpostor || ChatOpts.SeekerCanSeeChat.Value);

    private static GameplayOptions GameplayOpts => OptionGroupSingleton<GameplayOptions>.Instance;
    private static TauntingOptions TauntingOpts => OptionGroupSingleton<TauntingOptions>.Instance;
    private static ChatOptions ChatOpts => OptionGroupSingleton<ChatOptions>.Instance;

    public enum TimerStage
    {
        Revelation,
        Seeking,
        Hiding,
    }

    public override void AssignRoles(out bool runOriginal, LogicRoleSelectionNormal instance)
    {
        runOriginal = false;

        var players = PlayerControl.AllPlayerControls.ToArray().ToList();
        if (players.Count == 1)
        {
            players[0].RpcSetRole((RoleTypes) RoleId.Get<SeekerRole>(), false);
            return;
        }
        
        var setSeekers = new List<NetworkedPlayerInfo?> 
        { 
            GameplayOpts.Seeker1.GetPlayerValue(), 
            GameplayOpts.Seeker2.GetPlayerValue(), 
            GameplayOpts.Seeker3.GetPlayerValue()
        }
        .OfType<NetworkedPlayerInfo>().ToList();

        players = players.Randomize();
        foreach (var sser in setSeekers)
        {
            var p = sser.Object;
            players.Remove(p);
        }
        
        var seekers = new List<PlayerControl>();

        var seekerCount = OptionGroupSingleton<GameplayOptions>.Instance.SeekersCount - setSeekers.Count;
        for (int i = 0; i < Math.Clamp(seekerCount, 0, players.Count - 1); i++)
        {
            seekers.Add(players[i]);
            Logger<CrewmeleonRedrawnPlugin>.Instance.LogWarning($"Randomly assigned seeker to {players[i].Data.PlayerName}.");
        }

        foreach (var sser in setSeekers)
        {
            if(seekers.Contains(sser.Object))
            {
                Logger<CrewmeleonRedrawnPlugin>.Instance.LogError($"Failed to assign seeker to {sser.PlayerName}, they are already assigned as a seeker.");
                continue;
            }

            seekers.Add(sser.Object);
            Logger<CrewmeleonRedrawnPlugin>.Instance.LogWarning($"Manually assigned seeker to {sser.PlayerName}.");
        }
        
        var hiders = players.Where(x => !seekers.Contains(x)).ToList();
        AssignTeamRoles(hiders, (RoleTypes) RoleId.Get<HiderRole>());
        AssignTeamRoles(seekers, (RoleTypes) RoleId.Get<SeekerRole>());
    }

    private static void AssignTeamRoles(List<PlayerControl> players, RoleTypes role)
    {
        foreach (var p in players)
        {
            p.RpcSetRole(role, false);
            Logger<CrewmeleonRedrawnPlugin>.Instance.LogMessage($"Assigned {role.ToDisplayString()} role to {p.Data.PlayerName}.");
        }
    }

    //Using PostAssignRoles because Initialize doesn't get called, inlining maybe?
    public override void PostAssignRoles(LogicRoleSelectionNormal logic)
    {
        ShipStatus.Instance.BreakEmergencyButton();

        var hud = HudManager.Instance;
        hud.CrewmatesKilled.gameObject.SetActive(true);
        hud.TaskStuff.gameObject.SetActive(false);

        timerBar = GameObject.Instantiate(GameManagerCreator.Instance.HideAndSeekManagerPrefab.TimerBarPrefab, hud.transform.parent);
        timerBar.timerBarRenderer.material.SetColor("_Color", Palette.CrewmateBlue);

        var timerBarAspectPos = timerBar.gameObject.GetComponent<AspectPosition>();
        timerBarAspectPos.Alignment = AspectPosition.EdgeAlignments.Top;
        timerBarAspectPos.DistanceFromEdge = new Vector3(0, 0.35f, 0);
        timerBarAspectPos.AdjustPosition();

        stageMaxTime = GameplayOpts.HideTime.Value;
        stageTimeLeft = stageMaxTime;

        CurrentStage = TimerStage.Hiding;

        stageText = GameObject.Instantiate(timerBar.timeText, timerBar.transform);
        stageText.GetComponent<TextTranslatorTMP>().Destroy();
        stageText.transform.position += new Vector3(1.5f, 0, 0);
        stageText.alignment = TextAlignmentOptions.Right;
        stageText.text = $"{CurrentStage.ToString().ToUpperInvariant()} TIME";
        
        // create the taunt timer bar and label if taunting is enabled
        if (TauntingOpts.TauntingEnabled)
        {
            tauntBar = GameObject.Instantiate(GameManagerCreator.Instance.HideAndSeekManagerPrefab.TimerBarPrefab, hud.transform.parent);
            tauntBar.timerBarRenderer.material.SetColor("_Color", Color.yellow);

            var tauntBarAspectPos = tauntBar.gameObject.GetComponent<AspectPosition>();
            tauntBarAspectPos.Alignment = AspectPosition.EdgeAlignments.Top;
            tauntBarAspectPos.DistanceFromEdge = new Vector3(0, 0.75f, 0);
            tauntBarAspectPos.AdjustPosition();

            tauntMaxTime = TauntingOpts.TauntCooldown.Value;
            tauntTimeLeft = tauntMaxTime;

            var text = GameObject.Instantiate(tauntBar.timeText, tauntBar.transform);
            text.GetComponent<TextTranslatorTMP>().Destroy();
            text.transform.position += new Vector3(1.5f, 0, 0);
            text.alignment = TextAlignmentOptions.Right;
            tauntBar.transform.localScale *= 0.7f;
            text.text = "NEXT TAUNT";
        }
    }

    public override PlayerBodyTypes GetBodyType(PlayerControl player)
    {
        bool isImpostor = player && player.Data && player.Data.Role && player.Data.Role.IsImpostor;

        if (AprilFoolsMode.ShouldHorseAround())
            return isImpostor ? PlayerBodyTypes.Normal : PlayerBodyTypes.Horse;

        if (AprilFoolsMode.ShouldLongAround())
            return isImpostor ? PlayerBodyTypes.LongSeeker : PlayerBodyTypes.Long;

        return isImpostor ? PlayerBodyTypes.Seeker : PlayerBodyTypes.Normal;
    }

    public override void OnPlayerDeath(PlayerControl player, bool assignGhostRole)
    {
        base.OnPlayerDeath(player, assignGhostRole);
        NotifyOfDeath(player);
    }

    public void NotifyOfDeath(PlayerControl player, bool notDead = false)
    {
        deadPlayerCount++;

        HudManager.Instance.NotifyOfDeath();

        var popupPrefab = GameManagerCreator.Instance.HideAndSeekManagerPrefab.DeathPopupPrefab;
        var popup = GameObject.Instantiate(popupPrefab, HudManager.Instance.transform.parent);

        popup.text.text = notDead ? "HAS BEEN INFECTED" : "HAS BEEN KILLED";
        popup.Show(player, deadPlayerCount);
    }

    public override bool CanReport(DeadBody body) => false;
    public override bool CanUseMapConsole(MapConsole console) => false;
    public override bool CanUseTasks(Console console) => false;
    public override bool ShouldShowSabotageMap(MapBehaviour map) => false;
    public override bool CanVent(Vent vent, NetworkedPlayerInfo playerInfo) => false;

    public override void HudUpdate(HudManager instance)
    {
        if (timerBar is null || !timerBar)
            return;

        instance.TaskStuff.gameObject.SetActive(false);
        instance.Chat.gameObject.SetActive(CanUseChat);
        
        stageTimeLeft -= Time.deltaTime;
        timerBar.UpdateTimer(stageTimeLeft, stageMaxTime);

        // prevent the seekers from moving in the hiding stage
        if (CurrentStage == TimerStage.Hiding)
        {
            if (AmImpostor && PlayerControl.LocalPlayer.MyPhysics.Speed != 0)
            {
                defaultSeekerSpeed = PlayerControl.LocalPlayer.MyPhysics.Speed;

                // surely there's a better way?
                PlayerControl.LocalPlayer.moveable = false;
                PlayerControl.LocalPlayer.NetTransform.Halt();
                PlayerControl.LocalPlayer.MyPhysics.Speed = 0;
            }
        }

        // end the current stage if the timer has reached 0
        if(stageTimeLeft <= 0)
        {
            if(CurrentStage == TimerStage.Revelation)
                OnRevalationStageEnd();
            else
            {
                if (CurrentStage == TimerStage.Hiding)
                    OnHidingStageEnd();
                else if (CurrentStage == TimerStage.Seeking)
                    OnSeekingStageEnd();

                if (stageText is not null && stageText)
                    stageText.text = $"{CurrentStage.ToString().ToUpperInvariant()} TIME";
            }
        }

        // update the taunt timer bar and perform automatic taunt
        if(TauntingOpts.TauntingEnabled)
        {
            tauntTimeLeft -= Time.deltaTime;

            if(tauntBar is not null && tauntBar)
                tauntBar.UpdateTimer(tauntTimeLeft, tauntMaxTime);

            if (tauntTimeLeft <= 0)
            {
                tauntTimeLeft = tauntMaxTime;

                foreach (var playerControl in Helpers.GetAlivePlayers().Where(x => !x.AmOwner))
                    AudioSource.PlayClipAtPoint(GameManagerCreator.Instance.HideAndSeekManagerPrefab.FinalHideCountdownSFX, playerControl.GetTruePosition(), 0.1f);
            }
        }     
    }

    /// <summary>
    /// End the hiding stage and start the seeking stage.
    /// </summary>
    private void OnHidingStageEnd()
    {
        CurrentStage = TimerStage.Seeking;

        stageMaxTime = GameplayOpts.SeekTime.Value;
        stageTimeLeft = stageMaxTime;

        if(timerBar is not null && timerBar)
            timerBar.timerBarRenderer.material.SetColor("_Color", Palette.ImpostorRed);

        SoundManager.Instance.PlaySound(GameManagerCreator.Instance.HideAndSeekManagerPrefab.FinalHideAlertSFX, false, 1);
        
        // allow the seeker to move when the seeking stage starts
        if (AmImpostor && PlayerControl.LocalPlayer.MyPhysics.Speed <= 0)
        {
            PlayerControl.LocalPlayer.moveable = true;
            PlayerControl.LocalPlayer.MyPhysics.Speed = defaultSeekerSpeed;
        }
    }

    /// <summary>
    /// End the seeking stage and start the revalation stage.
    /// </summary>
    private void OnSeekingStageEnd()
    {
        CurrentStage = TimerStage.Revelation;

        var hiders = Helpers.GetAlivePlayers().Where(p => !p.Data.Role.IsImpostor).ToList();
        int timePerPlayer = 5; // TODO: add game option to manually adjust this

        // give each hider a revalation period
        if (hiders.Count > 0 )
        {
            stageMaxTime = hiders.Count * timePerPlayer; 
            stageTimeLeft = stageMaxTime;

            if (timerBar is not null && timerBar)
                timerBar.timerBarRenderer.material.SetColor("_Color", Color.yellow);

            Coroutines.Start(CoReveal(hiders, timePerPlayer));
            return;
        }

        stageTimeLeft = 0;
        return;
    }

    /// <summary>
    /// End the revalation stage and end the game.
    /// </summary>
    private void OnRevalationStageEnd()
    {
        if (!PlayerControl.LocalPlayer.IsHost())
            return;
            
        GameManager.Instance.RpcEndGame(GameOverReason.HideAndSeek_CrewmatesByTimer, false);
    }

    public override IEnumerator IntroCutscene(IntroCutscene intro)
    {
        deadPlayerCount = 0;

        SoundManager.Instance.PlaySound(intro.IntroStinger, false, 1f, null);
        ShipStatus.Instance.BreakEmergencyButton();

        Logger.GlobalInstance.Info("IntroCutscene :: CoBegin() :: Game Mode: Hide and Seek (MiraAPI)", null);

        intro.LogPlayerRoleData();
        intro.HideAndSeekPanels.SetActive(true);

        intro.CrewmateRules.SetActive(!AmImpostor);
        intro.ImpostorRules.SetActive(AmImpostor);

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

        if (impostor != null)
        {
            intro.ImpostorTitle.text = impostor.Data.Role.GetRoleName();
        }    

        PoolablePlayer? playerSlot = null;

        if (impostor != null)
        {
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

        var hideTimer = OptionGroupSingleton<GameplayOptions>.Instance.HideTime.Value;

        if (AmImpostor)
        {
            intro.HideAndSeekTimerText.gameObject.SetActive(true);

            PoolablePlayer poolablePlayer;
            AnimationClip anim;
            if (AprilFoolsMode.ShouldHorseAround())
            {
                poolablePlayer = intro.HorseWrangleVisualSuit;
                poolablePlayer.gameObject.SetActive(true);
                poolablePlayer.SetBodyType(PlayerBodyTypes.Seeker);
                anim = intro.HnSSeekerSpawnHorseAnim;
                intro.HorseWrangleVisualPlayer.SetBodyType(PlayerBodyTypes.Normal);
                intro.HorseWrangleVisualPlayer.UpdateFromPlayerData(
                    PlayerControl.LocalPlayer.Data,
                    PlayerControl.LocalPlayer.CurrentOutfitType,
                    PlayerMaterial.MaskType.None,
                    false,
                    null,
                    false);
            }
            else if (AprilFoolsMode.ShouldLongAround())
            {
                poolablePlayer = intro.HideAndSeekPlayerVisual;
                poolablePlayer.gameObject.SetActive(true);
                poolablePlayer.SetBodyType(PlayerBodyTypes.LongSeeker);
                anim = intro.HnSSeekerSpawnLongAnim;
            }
            else
            {
                // we can prob delay the getting up portion no until the last 5ish seconds?
                poolablePlayer = intro.HideAndSeekPlayerVisual;
                poolablePlayer.gameObject.SetActive(true);
                poolablePlayer.SetBodyType(PlayerBodyTypes.Seeker);
                anim = intro.HnSSeekerSpawnAnim;
            }

            poolablePlayer.SetBodyCosmeticsVisible(false);
            poolablePlayer.UpdateFromPlayerData(
                PlayerControl.LocalPlayer.Data,
                PlayerControl.LocalPlayer.CurrentOutfitType,
                PlayerMaterial.MaskType.None,
                false,
                null,
                false);
            SpriteAnim component = poolablePlayer.GetComponent<SpriteAnim>();
            poolablePlayer.gameObject.SetActive(true);
            poolablePlayer.ToggleName(false);
            component.Play(anim, 1f);
            while (hideTimer > 0f)
            {
                intro.HideAndSeekTimerText.text = Mathf.RoundToInt(hideTimer).ToString();
                hideTimer -= Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            ShipStatus.Instance.HideCountdown = hideTimer;
            if (AprilFoolsMode.ShouldHorseAround())
            {
                if (impostor != null)
                {
                    impostor.AnimateCustom(intro.HnSSeekerSpawnHorseInGameAnim);
                }
            }
            else if (AprilFoolsMode.ShouldLongAround())
            {
                if (impostor != null)
                {
                    impostor.AnimateCustom(intro.HnSSeekerSpawnLongInGameAnim);
                }
            }
            else if (impostor != null)
            {
                impostor.AnimateCustom(intro.HnSSeekerSpawnAnim);
                impostor.cosmetics.SetBodyCosmeticsVisible(false);
            }
        }

        ShipStatus.Instance.StartSFX();
        UnityEngine.Object.Destroy(intro.gameObject);
    }

    private IEnumerator CoReveal(List<PlayerControl> players, int timePerPlayer)
    {
        if (PlayerControl.LocalPlayer.HasModifier<SpectatingModifier>())
            PlayerControl.LocalPlayer.RemoveModifier<SpectatingModifier>();
        
        HudManager.Instance.ShadowQuad.enabled = false;

        foreach (var player in players)
        {
            HudManager.Instance.PlayerCam.Target = player;
            yield return new WaitForSeconds(timePerPlayer);
        }
    }
}
