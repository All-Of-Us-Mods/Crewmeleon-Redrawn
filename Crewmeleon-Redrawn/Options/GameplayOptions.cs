using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Crewmeleon_Redrawn;

public class GameplayOptions : AbstractOptionGroup
{
    public override string GroupName => "Gameplay Options";

    public ModdedNumberOption SeekersCount { get; } =
        new("Seekers Count", 1, 1, 3, 1, MiraNumberSuffixes.None);
    
    public ModdedPlayerOption Seeker1 { get; } =
        new("Forced Seeker #1")
        {
            Visible = () => OptionGroupSingleton<GameplayOptions>.Instance.SeekersCount.Value >= 1
        };

    public ModdedPlayerOption Seeker2 { get; } =
        new("Forced Seeker #2")
        {
            Visible = () => OptionGroupSingleton<GameplayOptions>.Instance.SeekersCount.Value >= 2
        };

    public ModdedPlayerOption Seeker3 { get; } =
        new("Forced Seeker #3")
        {
            Visible = () => OptionGroupSingleton<GameplayOptions>.Instance.SeekersCount.Value >= 3
        };
    
    public ModdedNumberOption HideTime { get; } =
        new("Initial Hide Time", 80, 0, 200, 10, "0", "0", MiraNumberSuffixes.Seconds,
            halfIncrements: true);
    
    public ModdedNumberOption SeekTime { get; } =
        new("Seek Time", 80, 0, 200, 10, "0", "0", MiraNumberSuffixes.Seconds,
            halfIncrements: true);
}