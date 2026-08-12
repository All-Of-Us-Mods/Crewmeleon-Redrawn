using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;

namespace Crewmeleon_Redrawn;

public class OutlineOptions : AbstractOptionGroup
{
    public override string GroupName => "Outline";

    public ModdedEnumOption<OutlineStrength> OutlineStrengthOption = new("Outline Strength", OutlineStrength.Strong);

    // enum value is the percentage of opacity (Strong = 40%)
    public enum OutlineStrength
    {
        Disabled = 0,
        VerySubtle = 20,
        Subtle = 30,
        Strong = 40
    }

}