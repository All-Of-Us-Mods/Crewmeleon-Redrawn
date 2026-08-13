using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace Crewmeleon_Redrawn;

public class CrewmeleonAssets
{
    private static string SpritesPath = "Crewmeleon-Redrawn.Resources.Sprites";
    private static string SplatterSpritesPath = SpritesPath + ".Splatters";
    private static string ButtonSpritesPath = SpritesPath + ".Buttons";
    
    public static LoadableAsset<Sprite> PlayerSprite { get; } = new LoadableResourceAsset(SpritesPath + ".PlayerSprite.png");
    public static LoadableAsset<Sprite> PlayerSpriteOutline { get; } = new LoadableResourceAsset(SpritesPath + ".PlayerOutline.png");
    public static LoadableAsset<Sprite> ZoomFrame { get; } = new LoadableResourceAsset(SpritesPath + ".ZoomFrame.png");
    public static LoadableAsset<Sprite> ColorWheel { get; } = new LoadableResourceAsset(SpritesPath + ".ColorWheel.png");
    public static LoadableAsset<Sprite> BrushCursor { get; } = new LoadableResourceAsset(SpritesPath + ".BrushCursor.png");
    public static LoadableAsset<Sprite> ColorSwatch { get; } = new LoadableResourceAsset(SpritesPath + ".ColorSwatch.png");
    public static LoadableAsset<Sprite> LockButton = new LoadableResourceAsset(ButtonSpritesPath + ".Lock.png");
    public static LoadableAsset<Sprite> UnlockButton = new LoadableResourceAsset(ButtonSpritesPath + ".Unlock.png");
    public static LoadableAsset<Sprite> Shotgun = new LoadableResourceAsset(SpritesPath + ".Shotgun.png");
    public static LoadableAsset<Sprite> SplatterShotgun = new LoadableResourceAsset(SpritesPath + ".SplatterShotgun.png");
    public static LoadableAsset<Sprite> MuzzleFlash = new LoadableResourceAsset(SpritesPath + ".MuzzleFlash.png");
    public static LoadableAsset<Sprite> Hands = new LoadableResourceAsset(SpritesPath + ".Hands.png");
    public static LoadableAsset<Sprite> TargetSprite = new LoadableResourceAsset(SpritesPath + ".Target.png");
    public static List<LoadableResourceAsset> SplatterSprites =
    [
        new(SplatterSpritesPath + ".Splatter1.png"), new(SplatterSpritesPath + ".Splatter2.png"),
        new(SplatterSpritesPath + ".Splatter3.png"), new(SplatterSpritesPath + ".Splatter4.png"),
    ];
    
    public static LoadableAudioResourceAsset ShotgunFireSound = new("Crewmeleon-Redrawn.Resources.Sounds.Shotgun.wav");
}