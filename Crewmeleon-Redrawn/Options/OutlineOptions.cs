using CrewmeleonRedrawn.GameMode;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;

namespace CrewmeleonRedrawn;

public class OutlineOptions : AbstractOptionGroup<ChameleonGameMode>
{
    public override string GroupName => "Outline";
    public override uint GroupPriority => 4;
    public ModdedEnumOption<OutlineStrength> OutlineStrengthOption { get; } =
        new("Outline Strength", OutlineStrength.Strong);

    public enum OutlineStrength
    {
        VerySubtle,
        Subtle,
        Strong,
        Disabled,
    }
}

public static class OutlineStrengthExtensions
{
    public static float Opacity(this OutlineOptions.OutlineStrength strength) => strength switch
    {
        OutlineOptions.OutlineStrength.VerySubtle => 0.2f,
        OutlineOptions.OutlineStrength.Subtle => 0.3f,
        OutlineOptions.OutlineStrength.Strong => 0.4f,
        _ => 0f,
    };
}