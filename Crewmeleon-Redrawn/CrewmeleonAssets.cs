using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace Crewmeleon_Redrawn;

public class CrewmeleonAssets
{
    public static LoadableAsset<Sprite> PlayerSprite { get; } = new LoadableResourceAsset($"Crewmeleon-Redrawn.Resources.PlayerSprite.png");
    public static LoadableAsset<Sprite> ZoomFrame { get; } = new LoadableResourceAsset($"Crewmeleon-Redrawn.Resources.ZoomFrame.png");
    public static LoadableAsset<Sprite> ColorWheel { get; } = new LoadableResourceAsset($"Crewmeleon-Redrawn.Resources.ColorWheel.png");
    public static LoadableAsset<Sprite> BrushCursor { get; } = new LoadableResourceAsset($"Crewmeleon-Redrawn.Resources.BrushCursor.png");
    public static LoadableAsset<Sprite> ColorSwatch { get; } = new LoadableResourceAsset($"Crewmeleon-Redrawn.Resources.ColorSwatch.png");
}