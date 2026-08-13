using System.Collections;
using CrewmeleonRedrawn.Networking;
using CrewmeleonRedrawn.Roles;
using MiraAPI.GameModes;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace CrewmeleonRedrawn.GameMode;

public class ChameleonGameMode : AbstractGameMode
{
    public static ChameleonGameMode? Instance => CustomGameModeManager.ActiveMode is ChameleonGameMode mode ? mode : null;

    public override string Name => "Crewmeleon";
    public override string Description => "You can run, but you can't hide!";
    public override Color Color { get; } = new Color32(150, 255, 90, 255);

    public override bool ShowGameModeIntroCutscene => true;
    public override bool GameModeBodyTypeOverride => true;
    public override bool ShowNormalGameSettings => false;
    public override bool ShowNormalRoleSettings => false;

    public TimerStage CurrentStage => Timer.CurrentStage;

    internal static bool AmImpostor => PlayerControl.LocalPlayer.Data.Role.IsImpostor;

    private static bool CanUseChat => ChameleonOptions.Chat.ChatEnabled
                                      && (!AmImpostor || ChameleonOptions.Chat.SeekerCanSeeChat.Value);

    public readonly ChameleonTimer Timer = new();
    public readonly TauntTimer TauntTimer = new();
    public readonly PlayerTracker PlayerTracker = new();

    private int deadPlayerCount;

    public override void AssignRoles(out bool runOriginal, LogicRoleSelectionNormal instance)
    {
        runOriginal = false;
        ChameleonRoleAssigner.AssignRoles();
    }

    public override void Initialize()
    {
        ShipStatus.Instance.BreakEmergencyButton();

        foreach (var player in Helpers.GetAlivePlayers())
            player.cosmetics.TogglePet(false);

        var hud = HudManager.Instance;
        hud.CrewmatesKilled.gameObject.SetActive(true);
        hud.TaskStuff.gameObject.SetActive(false);

        Timer.CreateTimer(hud);
        PlayerTracker.Begin(hud);

        if (AmongUsClient.Instance.AmHost)
        {
            PlayerControl.LocalPlayer.RpcUpdateTimerState(TimerStage.Hiding);
        }
    }

    public override void HudUpdate(HudManager instance)
    {
        if (!Timer.IsActive)
            return;

        instance.TaskStuff.gameObject.SetActive(false);
        instance.ReportButton.gameObject.SetActive(false);
        instance.SabotageButton.gameObject.SetActive(false);
        instance.ImpostorVentButton.gameObject.SetActive(false);
        instance.KillButton.gameObject.SetActive(false);
        instance.Chat.gameObject.SetActive(CanUseChat);

        Timer.Update();
        PlayerTracker.Update();
        if (CurrentStage == TimerStage.Seeking) TauntTimer.Update();
    }

    public override IEnumerator IntroCutscene(IntroCutscene intro)
    {
        deadPlayerCount = 0;
        return ChameleonIntro.Play(intro);
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

    public void NotifyOfDeath(PlayerControl player, bool infected = false)
    {
        deadPlayerCount++;
        
        HudManager.Instance.NotifyOfDeath();
        
        var popupPrefab = GameManagerCreator.Instance.HideAndSeekManagerPrefab.DeathPopupPrefab;
        var popup = GameObject.Instantiate(popupPrefab, HudManager.Instance.transform.parent);

        popup.text.GetComponent<TextTranslatorTMP>().DestroyImmediate();
        popup.text.text = infected ? "HAS BEEN INFECTED" : "HAS BEEN KILLED";
        popup.Show(player, deadPlayerCount);
    }

    public void OnBeginSpectate(PlayerControl player)
    {
    }
    
    public void OnStopSpectate(PlayerControl player)
    {
    }

    public override void CheckGameEnd(out bool runOriginal, LogicGameFlowNormal instance)
    {
        runOriginal = false;
        if (!PlayerControl.LocalPlayer.IsHost()) return;
        if (Helpers.GetAlivePlayers().Where(x => x.Data.Role is HiderRole && !x.Data.IsDead).ToArray().Length >
            0) return;
        GameManager.Instance.RpcEndGame(GameOverReason.ImpostorsByKill, false);
    }

    public override bool CanReport(DeadBody body) => false;
    public override bool CanUseMapConsole(MapConsole console) => false;
    public override bool CanUseTasks(Console console) => false;
    public override bool ShouldShowSabotageMap(MapBehaviour map) => false;
    public override bool CanVent(Vent vent, NetworkedPlayerInfo playerInfo) => false;
}
