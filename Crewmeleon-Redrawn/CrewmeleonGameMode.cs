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
        else if (AprilFoolsMode.ShouldHorseAround())
        {
            if (player.Data.Role.IsImpostor)
            {
                return PlayerBodyTypes.Normal;
            }
            return PlayerBodyTypes.Horse;
        }
        else if (AprilFoolsMode.ShouldLongAround())
        {
            if (player.Data.Role.IsImpostor)
            {
                return PlayerBodyTypes.LongSeeker;
            }
            return PlayerBodyTypes.Long;
        }
        else
        {
            if (player.Data.Role.IsImpostor)
            {
                return PlayerBodyTypes.Seeker;
            }
            return PlayerBodyTypes.Normal;
        }
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
}
