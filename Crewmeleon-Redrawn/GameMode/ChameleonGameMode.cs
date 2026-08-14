using System.Collections;
using CrewmeleonRedrawn.Roles;
using MiraAPI.GameModes;
using MiraAPI.Utilities;
using UnityEngine;

namespace CrewmeleonRedrawn.GameMode;

public class ChameleonGameMode : AbstractGameMode
{
    public override string Name => "Crewmeleon";
    public override string Description => "Paint yourself, blend in with your surroundings\nand survive for as long as possible!";

    public override bool ShowGameModeIntroCutscene => true;
    public override bool GameModeBodyTypeOverride => true;
    public override bool ShowNormalGameSettings => false;
    public override bool ShowNormalRoleSettings => false;
    internal static bool AmImpostor => PlayerControl.LocalPlayer.Data.Role.IsImpostor;

    public override void AssignRoles(out bool runOriginal, LogicRoleSelectionNormal instance)
    {
        runOriginal = false;
        ChameleonRoleAssigner.AssignRoles();
    }

    public override void Initialize()
    {
        ChameleonGameModeManager.Create();
    }

    public override IEnumerator IntroCutscene(IntroCutscene intro)
    {
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
        ChameleonGameModeManager.Instance?.NotifyOfDeath(player);
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
    public override bool CanUseSystemConsole(SystemConsole console) => false;
    public override bool CanUseMapConsole(MapConsole console) => false;
    public override bool CanUseTasks(Console console) => false;
    public override bool ShouldShowSabotageMap(MapBehaviour map) => false;
    public override bool CanVent(Vent vent, NetworkedPlayerInfo playerInfo) => false;
}
