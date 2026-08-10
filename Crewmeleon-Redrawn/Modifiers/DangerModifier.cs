using System.Linq;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Crewmeleon_Redrawn.Modifiers;

public class DangerModifier : BaseModifier
{
    public override string ModifierName => "FearModifier";

    private const float MaxFear = 5.25f;

    private float _fear;

    public override void Update()
    {
        base.Update();

        var impostorNearby = Helpers.GetClosestPlayers(Player, 3, false)
            .Any(control => control.Data && control.Data.Role && control.Data.Role.IsImpostor);

        _fear += Time.deltaTime * (impostorNearby ? 2.5f : -1f);
        _fear = Mathf.Clamp(_fear, 0f, MaxFear);

        if (Player.AmOwner)
        {
            HudManager.Instance.DangerMeter.SetDangerValue(_fear, _fear);
        }

        if (_fear > 5) Shake();
        else Player.transform.eulerAngles = Vector3.zero;
    }

    private void Shake() => Player.transform.eulerAngles = new Vector3(0, 0, Random.RandomRange(-10, 10));
}