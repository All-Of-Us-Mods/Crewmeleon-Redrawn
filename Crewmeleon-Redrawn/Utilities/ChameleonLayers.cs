using UnityEngine;

namespace CrewmeleonRedrawn.Utilities;

public static class ChameleonLayers
{
    private static readonly string[] HideableLayerNames = ["ShortObjects", "Objects"];

    private static int? _hideableMask;

    public static int HideableMask => _hideableMask ??= BuildHideableMask();

    public static int ShotBlockingMask =>
        Constants.ShipAndAllObjectsMask & ~(HideableMask | Constants.PlayersOnlyMask);

    private static int BuildHideableMask()
    {
        var mask = 0;
        foreach (var layerName in HideableLayerNames) mask |= 1 << LayerMask.NameToLayer(layerName);
        return mask;
    }
}
