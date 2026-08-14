using CrewmeleonRedrawn.GameMode;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace CrewmeleonRedrawn;

public class TauntingOptions : AbstractOptionGroup<ChameleonGameMode>
{
    public override string GroupName => "Taunting Options";
    public override uint GroupPriority => 3;
    public ModdedToggleOption TauntingEnabled { get; } =
        new ModdedToggleOption("Taunting Enabled", true);
    public ModdedNumberOption TauntCooldown { get; } =
        new ModdedNumberOption("Taunting Cooldown", 20, 10, 60, 10, MiraNumberSuffixes.Seconds)
        {
            Visible = () => OptionGroupSingleton<TauntingOptions>.Instance.TauntingEnabled.Value,
        };
}