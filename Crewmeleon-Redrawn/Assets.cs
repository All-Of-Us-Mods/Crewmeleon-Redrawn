using MiraAPI.Utilities.Assets;
using Mono.Cecil;

namespace Crewmeleon_Redrawn;

public class Assets
{
    private static string SpritesPath = "Crewmeleon-Redrawn.Resources.Sprites";
    private static string SplatterSpritesPath = SpritesPath + ".Splatters";
    private static string ButtonSpritesPath = SpritesPath + ".Buttons";

    public static LoadableResourceAsset LockButton = new LoadableResourceAsset(ButtonSpritesPath + ".Lock.png");
    public static LoadableResourceAsset UnlockButton = new LoadableResourceAsset(ButtonSpritesPath + ".Unlock.png");
    public static LoadableResourceAsset Shotgun = new LoadableResourceAsset(SpritesPath + ".Shotgun.png");
    public static LoadableResourceAsset SplatterShotgun = new LoadableResourceAsset(SpritesPath + ".SplatterShotgun.png");
    public static LoadableResourceAsset MuzzleFlash = new LoadableResourceAsset(SpritesPath + ".MuzzleFlash.png");
    public static LoadableResourceAsset Hands = new LoadableResourceAsset(SpritesPath + ".Hands.png");
    public static LoadableResourceAsset TargetSprite = new LoadableResourceAsset(SpritesPath + ".Target.png");
    public static List<LoadableResourceAsset> SplatterSprites =
    [
        new(SplatterSpritesPath + ".Splatter1.png"), new(SplatterSpritesPath + ".Splatter2.png"),
        new(SplatterSpritesPath + ".Splatter3.png"), new(SplatterSpritesPath + ".Splatter4.png"),
    ];
    
    public static LoadableAudioResourceAsset ShotgunFireSound = new("Crewmeleon-Redrawn.Resources.Sounds.Shotgun.wav");
}