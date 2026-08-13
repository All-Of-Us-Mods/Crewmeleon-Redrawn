using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace CrewmeleonRedrawn;

public class CrewmeleonAssets
{
    private const string SpritesPath = "Crewmeleon-Redrawn.Resources.Sprites";

    private const string ButtonSpritesPath = SpritesPath + ".Buttons";
    public static readonly LoadableAsset<Sprite> LockButton = new LoadableResourceAsset(ButtonSpritesPath + ".Lock.png");
    public static readonly LoadableAsset<Sprite> UnlockButton = new LoadableResourceAsset(ButtonSpritesPath + ".Unlock.png");
    public static readonly LoadableAsset<Sprite> PaintButton = new LoadableResourceAsset(ButtonSpritesPath + ".Paint.png");

    public static readonly LoadableAsset<Sprite> PlayerSprite = new LoadableResourceAsset(SpritesPath + ".PlayerSprite.png");
    public static readonly LoadableAsset<Sprite> PlayerSpriteOutline = new LoadableResourceAsset(SpritesPath + ".PlayerOutline.png");
    public static readonly LoadableAsset<Sprite> ZoomFrame = new LoadableResourceAsset(SpritesPath + ".ZoomFrame.png");
    public static readonly LoadableAsset<Sprite> ColorWheel = new LoadableResourceAsset(SpritesPath + ".ColorWheel.png");
    public static readonly LoadableAsset<Sprite> BrushCursor = new LoadableResourceAsset(SpritesPath + ".BrushCursor.png");
    public static readonly LoadableAsset<Sprite> ColorSwatch = new LoadableResourceAsset(SpritesPath + ".ColorSwatch.png");
    
    public static readonly LoadableAsset<Sprite> Shotgun = new LoadableResourceAsset(SpritesPath + ".Shotgun.png");
    public static readonly LoadableAsset<Sprite> SplatterShotgun = new LoadableResourceAsset(SpritesPath + ".SplatterShotgun.png");
    public static readonly LoadableAsset<Sprite> MuzzleFlash = new LoadableResourceAsset(SpritesPath + ".MuzzleFlash.png");
    public static readonly LoadableAsset<Sprite> Hands = new LoadableResourceAsset(SpritesPath + ".Hands.png");
    public static readonly LoadableAsset<Sprite> TargetSprite = new LoadableResourceAsset(SpritesPath + ".Target.png");
    public static readonly LoadableAudioResourceAsset ShotgunFireSound = new("Crewmeleon-Redrawn.Resources.Sounds.Shotgun.wav");

    private const string SplatterSpritesPath = SpritesPath + ".Splatters";
    public static List<LoadableResourceAsset> SplatterSprites =
    [
        new(SplatterSpritesPath + ".Splatter1.png"), new(SplatterSpritesPath + ".Splatter2.png"),
        new(SplatterSpritesPath + ".Splatter3.png"), new(SplatterSpritesPath + ".Splatter4.png"),
    ];

    public static readonly List<LoadableAudioResourceAsset> TauntSounds =
    [
        new("Crewmeleon-Redrawn.Resources.Sounds.taunt_1.wav"),
        new("Crewmeleon-Redrawn.Resources.Sounds.taunt_2.wav"),
        new("Crewmeleon-Redrawn.Resources.Sounds.taunt_3.wav"),
        new("Crewmeleon-Redrawn.Resources.Sounds.taunt_4.wav"),
        new("Crewmeleon-Redrawn.Resources.Sounds.taunt_5.wav"),
        new("Crewmeleon-Redrawn.Resources.Sounds.taunt_6.wav"),
        new("Crewmeleon-Redrawn.Resources.Sounds.taunt_7.wav"),
        new("Crewmeleon-Redrawn.Resources.Sounds.taunt_8.wav")
    ];
}