using CrewmeleonRedrawn.GameMode;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;

namespace CrewmeleonRedrawn;

public class SpectatingOptions : AbstractOptionGroup<ChameleonGameMode>
{
    public override string GroupName => "Spectating Options";
    public override uint GroupPriority => 5;
    public ModdedToggleOption SpectateHiders { get; } =
        new ModdedToggleOption("Can Spectate Other Hiders", true);
}