using MiraAPI.Utilities.Assets;

namespace Crewmeleon_Redrawn;

public class Assets
{
    private static string ButtonSpritesPath = "Crewmeleon-Redrawn.Resources.Sprites.Buttons";
    public static LoadableResourceAsset LockButton = new LoadableResourceAsset(ButtonSpritesPath + ".Lock.png");
    public static LoadableResourceAsset UnlockButton = new LoadableResourceAsset(ButtonSpritesPath + ".Unlock.png");
}