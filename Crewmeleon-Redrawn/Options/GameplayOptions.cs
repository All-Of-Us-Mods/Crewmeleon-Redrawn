using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Crewmeleon_Redrawn;

public class GameplayOptions : AbstractOptionGroup
{
    public override string GroupName => "Gameplay Options";

    public ModdedToggleOption AllowUndo { get; } =
        new ModdedToggleOption("Allow Undo", false);

    public ModdedNumberOption SeekersCount { get; } =
        new ModdedNumberOption("Seekers Count", 1, 1, 3, 1, MiraNumberSuffixes.None);
    
    public ModdedNumberOption HideTime { get; } =
        new ModdedNumberOption("Initial Hide Time", 80, 0, 200, 10, "0", "0", MiraNumberSuffixes.Seconds,
            halfIncrements: true);
    
    public ModdedNumberOption SeekTime { get; } =
        new ModdedNumberOption("Seek Time", 80, 0, 200, 10, "0", "0", MiraNumberSuffixes.Seconds,
            halfIncrements: true);
}