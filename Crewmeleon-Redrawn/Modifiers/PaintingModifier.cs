using CrewmeleonRedrawn.Components;
using CrewmeleonRedrawn.Utilities;
using MiraAPI.Modifiers;

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

        if (Player.AmOwner)
            ZoomCameraController.Instance?.ToggleDisplay(true);

        CustomButtonUtilities.RefreshActionButtonsDeferred(Player);
    }

    public override void OnDeactivate()
    {
        Player.moveable = wasMoveable;

        if (Player.AmOwner)
            ZoomCameraController.Instance?.ToggleDisplay(false);

        CustomButtonUtilities.RefreshActionButtonsDeferred(Player);

        base.OnDeactivate();
    }

    public override void OnDeath(DeathReason reason)
    {
        Player.RpcRemoveModifier<PaintingModifier>();
    }
}
