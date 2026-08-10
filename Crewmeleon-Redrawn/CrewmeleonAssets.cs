using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace Crewmeleon_Redrawn;

public class CrewmeleonAssets
{
    public static LoadableAsset<Sprite> PlayerSprite { get; } = new LoadableResourceAsset($"Crewmeleon-Redrawn.Resources.PlayerSprite.png");
    public static LoadableAsset<Sprite> ZoomFrame { get; } = new LoadableResourceAsset($"Crewmeleon-Redrawn.Resources.ZoomFrame.png");
}