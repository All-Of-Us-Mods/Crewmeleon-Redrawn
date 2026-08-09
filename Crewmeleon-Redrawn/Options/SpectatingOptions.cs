using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Crewmeleon_Redrawn;

public class SpectatingOptions : AbstractOptionGroup
{
    public override string GroupName => "Spectating Options";

    public ModdedToggleOption SpectateHiders { get; } =
        new ModdedToggleOption("Can Spectate Other Hiders", true);
}