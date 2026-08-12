using MiraAPI.Utilities.Assets;
using Mono.Cecil;

namespace Crewmeleon_Redrawn;

public class Assets
{
    private static string SpritesPath = "Crewmeleon-Redrawn.Resources.Sprites";
    private static string ButtonSpritesPath = SpritesPath + ".Buttons";

    public static LoadableResourceAsset LockButton = new LoadableResourceAsset(ButtonSpritesPath + ".Lock.png");
    public static LoadableResourceAsset UnlockButton = new LoadableResourceAsset(ButtonSpritesPath + ".Unlock.png");
    public static LoadableResourceAsset Shotgun = new LoadableResourceAsset(SpritesPath + ".Shotgun.png");
    public static LoadableResourceAsset SplatterShotgun = new LoadableResourceAsset(SpritesPath + ".SplatterShotgun.png");
    public static LoadableResourceAsset Hands = new LoadableResourceAsset(SpritesPath + ".Hands.png");
}