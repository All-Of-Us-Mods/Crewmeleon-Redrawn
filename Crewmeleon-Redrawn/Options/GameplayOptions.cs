using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace CrewmeleonRedrawn;

public class GameplayOptions : AbstractOptionGroup
{
    public override string GroupName => "Gameplay Options";

    public ModdedToggleOption AllowUndo { get; } =
        new ModdedToggleOption("Allow Undo", false);

    public ModdedNumberOption SeekersCount { get; } =
        new ModdedNumberOption("Seekers Count", 1, 1, 3, 1, "0", "0", MiraNumberSuffixes.None);
    
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
        new ModdedNumberOption("Initial Hide Time", 80, 0, 200, 10, "0", "0", MiraNumberSuffixes.Seconds, halfIncrements: true);
    
    public ModdedNumberOption SeekTime { get; } =
        new ModdedNumberOption("Seek Time", 80, 0, 200, 10, "0", "0", MiraNumberSuffixes.Seconds, halfIncrements: true);

    public ModdedNumberOption ShotgunCooldown { get; } =
        new ModdedNumberOption("Seeker Shot Cooldown", 2.5f, 0.5f, 15, 1, "0", "0", MiraNumberSuffixes.Seconds, halfIncrements: true);
    
    public ModdedNumberOption RevelationTimePerPlayer { get; } =
        new ModdedNumberOption("Revelation time per Player", 5, 0, 30, 1, "0", "0", MiraNumberSuffixes.Seconds);

    public ModdedToggleOption InfectionMode { get; } =
        new ModdedToggleOption("Infection Mode", false);
    
    public ModdedToggleOption AlwaysOnTop { get; } =
        new ModdedToggleOption("Hiders Always On Top Of Objects", true)
        {
            Visible = () => !OptionGroupSingleton<GameplayOptions>.Instance.HideOnObjects.Value,
        };
    
    public ModdedToggleOption HideOnObjects { get; } =
        new ModdedToggleOption("Hiders Can Hide On Map Objects", true);
}