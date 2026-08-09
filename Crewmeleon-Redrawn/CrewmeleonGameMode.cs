using System.Collections;
using AmongUs.GameOptions;
using Crewmeleon_Redrawn;
using Crewmeleon_Redrawn.Roles;
using HarmonyLib;
using InnerNet;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.HnsReimplemented.Options;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using UnityEngine;
using Object = System.Object;

public class ChameleonGameMode : AbstractGameMode
{
    public override string Name => "Crewmeleon";
    public override string Description => "You can run, but you can't hide!";
    public override Color Color { get; } = new Color32(150, 255, 90, 255);
    public override bool CanReport(DeadBody body) => false;
    public override bool ShowGameModeIntroCutscene => false;
    public override bool GameModeBodyTypeOverride => true;
    public override bool ShowNormalGameSettings => false;
    public override void AssignRoles(out bool runOriginal, LogicRoleSelectionNormal instance)
    {
        runOriginal = false;
        var players = PlayerControl.AllPlayerControls.ToArray().ToList();
        if (players.Count == 1)
        {
            players[0].RpcSetRole((RoleTypes) RoleId.Get<SeekerRole>(), false);
            return;
        }
        players = players.Randomize();
        var seekers = new List<PlayerControl>();
        for (int i = 0;
             i < Math.Clamp(OptionGroupSingleton<GameplayOptions>.Instance.SeekersCount, 1, players.Count - 1);
             i++)
        {
            seekers.Add(players[i]);
        }
        var hiders = players.Where(x => !seekers.Contains(x)).ToList();
        AssignRolesForTeam(hiders, (RoleTypes) RoleId.Get<HiderRole>());
        AssignRolesForTeam(seekers, (RoleTypes) RoleId.Get<SeekerRole>());
    }
    public static void AssignRolesForTeam(
        List<PlayerControl> players,
        RoleTypes role)
    {
        foreach (var p in players)
        {
            p.RpcSetRole(role, false);
            PluginSingleton<CrewmeleonRedrawnPlugin>.Instance.Log.LogMessage($"Set {p.Data.PlayerName}'s role to be: {role.ToDisplayString()}");
        }
    }
    public override PlayerBodyTypes GetBodyType(PlayerControl player)
    {
        if (player == null || player.Data == null || player.Data.Role == null)
        {
            if (AprilFoolsMode.ShouldHorseAround())
            {
                return PlayerBodyTypes.Horse;
            }

            if (AprilFoolsMode.ShouldLongAround())
            {
                return PlayerBodyTypes.Long;
            }

            return PlayerBodyTypes.Normal;
        }

        if (AprilFoolsMode.ShouldHorseAround())
        {
            if (player.Data.Role.IsImpostor)
            {
                return PlayerBodyTypes.Normal;
            }
            return PlayerBodyTypes.Horse;
        }

        if (AprilFoolsMode.ShouldLongAround())
        {
            if (player.Data.Role.IsImpostor)
            {
                return PlayerBodyTypes.LongSeeker;
            }
            return PlayerBodyTypes.Long;
        }

        if (player.Data.Role.IsImpostor)
        {
            return PlayerBodyTypes.Seeker;
        }
        return PlayerBodyTypes.Normal;
    }
    private int deadPlayerCount;
    public override void OnPlayerDeath(PlayerControl player, bool assignGhostRole)
    {
        base.OnPlayerDeath(player, assignGhostRole);
        HudManager.Instance.NotifyOfDeath();
        var popup = GameManagerCreator.Instance.HideAndSeekManagerPrefab.DeathPopupPrefab;
        deadPlayerCount++;
        var item = UnityEngine.Object.Instantiate(popup, HudManager.Instance.transform.parent);
        item.Show(player, deadPlayerCount);
    }
    public override bool CanUseMapConsole(MapConsole console) => false;
    public override bool CanUseTasks(Console console) => false;
    public override bool ShouldShowSabotageMap(MapBehaviour map) => false;
    public override bool CanVent(Vent vent, NetworkedPlayerInfo playerInfo) => false;
    public HideAndSeekTimerBar TimerBar;
    public float TimeLeft;
    public float MaxTime;
    public TimerStage currentStage;
    public float defaultSpeed;

    public override void HudUpdate(HudManager instance)
    {
        if (TimerBar == null) return;
        instance.TaskStuff.gameObject.SetActive(false);
        instance.Chat.gameObject.SetActive(CanUseChat());
        TimeLeft -= Time.deltaTime;
        TimerBar.UpdateTimer(TimeLeft, MaxTime);
        if (currentStage == TimerStage.Hiding)
        {
            if (PlayerControl.LocalPlayer.Data.Role is SeekerRole && PlayerControl.LocalPlayer.MyPhysics.Speed != 0)
            {
                defaultSpeed = PlayerControl.LocalPlayer.MyPhysics.Speed;
                // surely there's a better way?
                PlayerControl.LocalPlayer.moveable = false;
                PlayerControl.LocalPlayer.NetTransform.Halt();
                PlayerControl.LocalPlayer.MyPhysics.Speed = 0;
            }
        }
        if (TimeLeft <= 0 && currentStage != TimerStage.Revelation)
        {
            currentStage = (TimerStage)((uint)currentStage - 1);
            switch (currentStage)
            {
                case TimerStage.Seeking:
                    TimeLeft = OptionGroupSingleton<GameplayOptions>.Instance.SeekTime.Value;
                    TimerBar.timerBarRenderer.material.SetColor("_Color", Palette.ImpostorRed);
                    SoundManager.Instance.PlaySound(
                        GameManagerCreator.Instance.HideAndSeekManagerPrefab.FinalHideAlertSFX, false, 1);
                    MaxTime = TimeLeft;
                    // unsure how it'd end up negative but it's just a precaution
                    if (PlayerControl.LocalPlayer.Data.Role is SeekerRole && PlayerControl.LocalPlayer.MyPhysics.Speed <= 0)
                    {
                        PlayerControl.LocalPlayer.moveable = true;
                        PlayerControl.LocalPlayer.MyPhysics.Speed = defaultSpeed;
                    }
                    break;
                case TimerStage.Revelation:
                    var players = Helpers.GetAlivePlayers().Where(x => !x.Data.Role.IsImpostor).ToList();
                    if (players.Count == 0)
                    {
                        TimeLeft = 0;
                        return;
                    }
                    TimeLeft = players.Count * 5;
                    MaxTime = TimeLeft;
                    TimerBar.timerBarRenderer.material.SetColor("_Color", Color.yellow);
                    Coroutines.Start(CoReveal(players));
                    break;
            }
        }
        else if (TimeLeft <= 0 && currentStage == TimerStage.Revelation)
        {
            if (PlayerControl.LocalPlayer.IsHost()) GameManager.Instance.RpcEndGame(GameOverReason.HideAndSeek_CrewmatesByTimer, false);
        }
    }
    //Using PostAssignRoles because Initialize doesn't get called, inlining maybe?
    public override void PostAssignRoles(LogicRoleSelectionNormal logic)
    {
        ShipStatus.Instance.BreakEmergencyButton();
        var opts = OptionGroupSingleton<GameplayOptions>.Instance;
        var instance = HudManager.Instance;
        instance.CrewmatesKilled.gameObject.SetActive(true);
        instance.TaskStuff.gameObject.SetActive(false);
        TimerBar = UnityEngine.Object.Instantiate<HideAndSeekTimerBar>(GameManagerCreator.Instance.HideAndSeekManagerPrefab.TimerBarPrefab, instance.transform.parent);
        TimerBar.timerBarRenderer.material.SetColor("_Color", Palette.CrewmateBlue);
        var aspectPosition = TimerBar.gameObject.GetComponent<AspectPosition>();
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.Top;
        aspectPosition.DistanceFromEdge = new Vector3(0, 0.5f, 0);
        aspectPosition.AdjustPosition();
        TimeLeft = opts.HideTime.Value;
        MaxTime = TimeLeft;
        currentStage = TimerStage.Hiding;
    }

    private bool CanUseChat()
    {
        var opts = OptionGroupSingleton<ChatOptions>.Instance;
        if (opts.ChatEnabled)
        {
            if (PlayerControl.LocalPlayer.Data.Role.IsImpostor) return opts.SeekerCanSeeChat.Value;
            return true;
        }

        return false;
    }

    private IEnumerator CoReveal(List<PlayerControl> players)
    {
        float timePerPlayer = TimeLeft / players.Count;
        foreach (var player in players)
        {
            HudManager.Instance.PlayerCam.Target = player;
            yield return new WaitForSeconds(timePerPlayer);
        }
    }

    public enum TimerStage
    {
        Revelation = 0,
        Seeking = 1,
        Hiding = 2,
    }
}
