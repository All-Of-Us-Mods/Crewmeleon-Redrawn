using AmongUs.GameOptions;
using BepInEx.Logging;
using CrewmeleonRedrawn.Roles;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;

namespace CrewmeleonRedrawn.GameMode;

/// <summary>
/// Picks the seekers (forced first, then random) and assigns everyone else as hiders.
/// </summary>
public static class ChameleonRoleAssigner
{
    private static ManualLogSource Log => Logger<CrewmeleonRedrawnPlugin>.Instance;

    public static void AssignRoles()
    {
        var players = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => p != null && p.Data != null)
            .ToList();

        if (players.Count == 0)
        {
            Log.LogError("AssignRoles called with zero valid players. Aborting.");
            return;
        }

        if (players.Count == 1)
        {
            players[0].RpcSetRole((RoleTypes)RoleId.Get<SeekerRole>(), false);
            return;
        }

        var seekers = GetForcedSeekers();

        players = players.Randomize();
        players.RemoveAll(seekers.Contains);

        var maxRandom = players.Count;
        var randomSeekerCount = Math.Clamp(
            ChameleonOptions.Gameplay.SeekersCount - seekers.Count,
            0,
            maxRandom
        );

        for (var i = 0; i < randomSeekerCount; i++)
        {
            seekers.Add(players[i]);
            Log.LogMessage($"Randomly assigned seeker to {players[i].Data.PlayerName}.");
        }

        var hiders = players.Where(x => !seekers.Contains(x)).ToList();

        if (hiders.Count == 0)
        {
            Log.LogWarning("Every player ended up as a seeker (0 hiders). Check SeekersCount vs lobby size.");
        }

        AssignTeamRoles(hiders, (RoleTypes)RoleId.Get<HiderRole>());
        AssignTeamRoles(seekers, (RoleTypes)RoleId.Get<SeekerRole>());

        Log.LogMessage($"Crewmeleon RoleGen: Target seekers: {ChameleonOptions.Gameplay.SeekersCount}, " +
                       $"Forced: {seekers.Count - randomSeekerCount}, Random: {randomSeekerCount}, " +
                       $"Total seekers: {seekers.Count}, Total hiders: {hiders.Count}, " +
                       $"Total players: {players.Count + seekers.Count}");
    }

    private static List<PlayerControl> GetForcedSeekers()
    {
        var forced = new List<NetworkedPlayerInfo?>
        {
            ChameleonOptions.Gameplay.Seeker1.GetPlayerValue(),
            ChameleonOptions.Gameplay.Seeker2.GetPlayerValue(),
            ChameleonOptions.Gameplay.Seeker3.GetPlayerValue(),
        }.OfType<NetworkedPlayerInfo>();

        var seekers = new List<PlayerControl>();

        foreach (var info in forced)
        {
            if (info.Object == null || info.Object.Data == null)
            {
                Log.LogWarning(
                    $"Forced seeker '{info.PlayerName}' has no live PlayerControl (likely disconnected). Skipping."
                );
                continue;
            }

            if (seekers.Any(x => x.PlayerId == info.Object.PlayerId))
            {
                Log.LogError(
                    $"Failed to assign seeker to {info.PlayerName}, they are already assigned as a seeker."
                );
                continue;
            }

            seekers.Add(info.Object);
            Log.LogMessage($"Manually assigned seeker to {info.PlayerName}.");
        }

        return seekers;
    }

    private static void AssignTeamRoles(List<PlayerControl> players, RoleTypes role)
    {
        foreach (var player in players)
        {
            if (player == null || player.Data == null)
            {
                Log.LogWarning("Skipped assigning role to a null or stale PlayerControl.");
                continue;
            }

            player.RpcSetRole(role, false);
            Log.LogMessage($"Assigned {role.ToDisplayString()} role to {player.Data.PlayerName}.");
        }
    }
}