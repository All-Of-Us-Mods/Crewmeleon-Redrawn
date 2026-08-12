using System.Collections;
using MiraAPI.GameModes;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Crewmeleon_Redrawn.GameMode;

public class ChameleonGameMode : AbstractGameMode
{
    public static ChameleonGameMode? Instance => CustomGameModeManager.ActiveMode is ChameleonGameMode mode ? mode : null;

    public override string Name => "Crewmeleon";
    public override string Description => "You can run, but you can't hide!";
    public override Color Color { get; } = new Color32(150, 255, 90, 255);

    public override bool ShowGameModeIntroCutscene => false;
    public override bool GameModeBodyTypeOverride => true;
    public override bool ShowNormalGameSettings => false;

    public TimerStage CurrentStage => _timer.CurrentStage;

    internal static bool AmImpostor => PlayerControl.LocalPlayer.Data.Role.IsImpostor;

    private static bool CanUseChat => ChameleonOptions.Chat.ChatEnabled
                                      && (!AmImpostor || ChameleonOptions.Chat.SeekerCanSeeChat.Value);

    private readonly ChameleonTimer _timer = new();
    private readonly TauntTimer _tauntTimer = new();

    private int deadPlayerCount;

    public override void AssignRoles(out bool runOriginal, LogicRoleSelectionNormal instance)
    {
        runOriginal = false;
        ChameleonRoleAssigner.AssignRoles();
    }

    //Using PostAssignRoles because Initialize doesn't get called, inlining maybe?
    public override void PostAssignRoles(LogicRoleSelectionNormal logic)
    {
        ShipStatus.Instance.BreakEmergencyButton();

        var hud = HudManager.Instance;
        hud.CrewmatesKilled.gameObject.SetActive(true);
        hud.TaskStuff.gameObject.SetActive(false);

        _timer.Begin(hud);
        _tauntTimer.Begin(hud);
    }

    public override void HudUpdate(HudManager instance)
    {
        if (!_timer.IsActive)
            return;

        instance.TaskStuff.gameObject.SetActive(false);
        instance.Chat.gameObject.SetActive(CanUseChat);

        _timer.Update();
        _tauntTimer.Update();
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

    public override bool CanReport(DeadBody body) => false;
    public override bool CanUseMapConsole(MapConsole console) => false;
    public override bool CanUseTasks(Console console) => false;
    public override bool ShouldShowSabotageMap(MapBehaviour map) => false;
    public override bool CanVent(Vent vent, NetworkedPlayerInfo playerInfo) => false;
}
