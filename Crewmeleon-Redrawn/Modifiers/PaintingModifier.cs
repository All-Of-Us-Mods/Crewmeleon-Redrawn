using System.Collections;
using Crewmeleon_Redrawn.Utilities;
using MiraAPI.Modifiers;
using Reactor.Utilities;

namespace Crewmeleon_Redrawn.Modifiers;

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

        RefreshButtonsDeferred();
    }

    public override void OnDeactivate()
    {
        Player.moveable = wasMoveable;

        RefreshButtonsDeferred();

        base.OnDeactivate();
    }

    // MiraAPI republishes its ActiveModifiers list *after* these hooks run, so HasModifier<T>()
    // still reports the old state here. Waiting a frame lets the buttons read the new one.
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
