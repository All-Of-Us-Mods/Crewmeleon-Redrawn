using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace CrewmeleonRedrawn;

public static class CrewmeleonAssets
{
    private const string SpritesPath = "Crewmeleon-Redrawn.Resources.Sprites";
    private const string SoundsPath = "Crewmeleon-Redrawn.Resources.Sounds";

    private const string ButtonSpritesPath = SpritesPath + ".Buttons";
    public static readonly LoadableAsset<Sprite> LockButton = new LoadableResourceAsset(ButtonSpritesPath + ".Lock.png");
    public static readonly LoadableAsset<Sprite> UnlockButton = new LoadableResourceAsset(ButtonSpritesPath + ".Unlock.png");
    public static readonly LoadableAsset<Sprite> PaintButton = new LoadableResourceAsset(ButtonSpritesPath + ".Paint.png");
    public static readonly LoadableAsset<Sprite> SpectateButton = new LoadableResourceAsset(ButtonSpritesPath + ".Spectate.png");
    public static readonly LoadableAsset<Sprite> PickColorButton = new LoadableResourceAsset(ButtonSpritesPath + ".PickColor.png");
    public static readonly LoadableAsset<Sprite> TauntButton = new LoadableResourceAsset(ButtonSpritesPath + ".Taunt.png");

    private const string PaintingUISpritesPath = SpritesPath + ".UI.Painting";
    public static readonly LoadableAsset<Sprite> PlayerSprite = new LoadableResourceAsset(PaintingUISpritesPath + ".PlayerSprite.png");
    public static readonly LoadableAsset<Sprite> PlayerSpriteOutline = new LoadableResourceAsset(PaintingUISpritesPath + ".PlayerOutline.png");
    public static readonly LoadableAsset<Sprite> ZoomFrame = new LoadableResourceAsset(PaintingUISpritesPath + ".ZoomFrame.png");
    public static readonly LoadableAsset<Sprite> ColorWheel = new LoadableResourceAsset(PaintingUISpritesPath + ".ColorWheel.png");
    public static readonly LoadableAsset<Sprite> BrushCursor = new LoadableResourceAsset(PaintingUISpritesPath + ".BrushCursor.png");
    public static readonly LoadableAsset<Sprite> ColorSwatch = new LoadableResourceAsset(PaintingUISpritesPath + ".ColorSwatch.png");

    private const string ShotgunSpritesPath = SpritesPath + ".Shotgun";
    public static readonly LoadableAsset<Sprite> Shotgun = new LoadableResourceAsset(ShotgunSpritesPath + ".Shotgun.png");
    public static readonly LoadableAsset<Sprite> SplatterShotgun = new LoadableResourceAsset(ShotgunSpritesPath + ".SplatterShotgun.png");
    public static readonly LoadableAsset<Sprite> MuzzleFlash = new LoadableResourceAsset(ShotgunSpritesPath + ".MuzzleFlash.png");
    public static readonly LoadableAsset<Sprite> Hands = new LoadableResourceAsset(ShotgunSpritesPath + ".Hands.png");
    public static readonly LoadableAsset<Sprite> TargetSprite = new LoadableResourceAsset(ShotgunSpritesPath + ".Target.png");
    public static readonly LoadableAudioResourceAsset ShotgunFireSound = new(SoundsPath + ".Shotgun.wav");

    public static readonly LoadableAsset<Sprite> GamemodeIcon = new LoadableResourceAsset(SpritesPath + ".UI.CrewmeleonGamemodeIcon.png");
    
    private const string SplatterSpritesPath = SpritesPath + ".Splatters";
    public static readonly List<LoadableResourceAsset> SplatterSprites =
    [
        new(SplatterSpritesPath + ".Splatter1.png"),
        new(SplatterSpritesPath + ".Splatter2.png"),
        new(SplatterSpritesPath + ".Splatter3.png"),
        new(SplatterSpritesPath + ".Splatter4.png"),
    ];

    public static readonly List<LoadableAudioResourceAsset> TauntSounds =
    [
        new(SoundsPath + ".Taunt1.wav"),
        new(SoundsPath + ".Taunt2.wav"),
        new(SoundsPath + ".Taunt3.wav"),
        new(SoundsPath + ".Taunt4.wav"),
        new(SoundsPath + ".Taunt5.wav"),
        new(SoundsPath + ".Taunt6.wav"),
        new(SoundsPath + ".Taunt7.wav"),
        new(SoundsPath + ".Taunt8.wav"),
        new(SoundsPath + ".Taunt9.wav"),
    ];
    
    private const string CrewmateRulesPath = SpritesPath + ".Intro.Crewmates";
    public static readonly List<LoadableResourceAsset> CrewmateRules =
    [
        new(CrewmateRulesPath + ".image1.png", 600),
        new(CrewmateRulesPath + ".image2.png", 400),
        new(CrewmateRulesPath + ".image3.png", 400)
    ];
    
    private const string ImpostorRulesPath = SpritesPath + ".Intro.Impostors";
    public static readonly List<LoadableResourceAsset> ImpostorRules =
    [
        new(ImpostorRulesPath + ".image1.png", 300),
        new(ImpostorRulesPath + ".image2.png", 350),
        new(ImpostorRulesPath + ".image3.png", 400)
    ];
}