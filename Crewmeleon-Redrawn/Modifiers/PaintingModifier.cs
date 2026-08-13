using System.Collections;
using CrewmeleonRedrawn.Components;
using CrewmeleonRedrawn.Utilities;
using MiraAPI.Modifiers;
using Reactor.Utilities;

namespace CrewmeleonRedrawn.Modifiers;

public class PaintingModifier : BaseModifier
{
    public override string ModifierName => "Painting";
    public override bool HideOnUi => true;

    private bool wasMoveable;

    public override void OnActivate()
    {
        base.OnActivate();

        wasMoveable = Player.moveable;
        Player.moveable = false;
        Player.NetTransform.Halt();

        if (Player.AmOwner) ZoomCameraController.Instance?.ToggleDisplay(true);

        RefreshButtonsDeferred();
    }

    public override void OnDeactivate()
    {
        Player.moveable = wasMoveable;

        if (Player.AmOwner) ZoomCameraController.Instance?.ToggleDisplay(false);

        RefreshButtonsDeferred();

        base.OnDeactivate();
    }

    // MiraAPI updates ActiveModifiers after these hooks run so HasModifier<T>() still reports
    // the old state in here. waiting a frame lets the buttons see the new one
    private void RefreshButtonsDeferred()
    {
        if (Player == null || !Player.AmOwner) return;

        Coroutines.Start(CoRefreshButtons());
    }

    private static IEnumerator CoRefreshButtons()
    {
        yield return null;
        CustomButtonUtilities.RefreshVisibility();
    }
}
