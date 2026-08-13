using MiraAPI.GameOptions;

namespace CrewmeleonRedrawn.GameMode;

public static class ChameleonOptions
{
    public static GameplayOptions   Gameplay => OptionGroupSingleton<GameplayOptions>.Instance;
    public static TauntingOptions   Taunting => OptionGroupSingleton<TauntingOptions>.Instance;
    public static SpectatingOptions Spectating => OptionGroupSingleton<SpectatingOptions>.Instance;
    public static ChatOptions       Chat => OptionGroupSingleton<ChatOptions>.Instance;
    public static OutlineOptions    Outline => OptionGroupSingleton<OutlineOptions>.Instance;
}
