using MiraAPI.GameOptions;

namespace Crewmeleon_Redrawn.GameMode;

public static class ChameleonOptions
{
    public static GameplayOptions Gameplay => OptionGroupSingleton<GameplayOptions>.Instance;
    public static TauntingOptions Taunting => OptionGroupSingleton<TauntingOptions>.Instance;
    public static ChatOptions Chat => OptionGroupSingleton<ChatOptions>.Instance;
}
