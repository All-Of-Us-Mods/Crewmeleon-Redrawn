using System.Collections;
using Crewmeleon_Redrawn.Networking;
using Crewmeleon_Redrawn.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Crewmeleon_Redrawn.Components;

[RegisterInIl2Cpp]
public class ShotgunComponent(nint cppPtr) : MonoBehaviour(cppPtr)
{
    public PlayerControl Owner;
    public int ZRotation;
        
    private bool _isSplatter;
    private SpriteRenderer _rend;
    private SpriteRenderer _handsRend;
    private int _lastNetworkedAngle = 0;
    private int _networkedThreshold = 15;

    private void Start()
    {
        _rend = GetComponent<SpriteRenderer>();
        
        var hands = new GameObject("Hands")
        {
            transform =
            {
                parent = transform,
                localPosition = Vector2.zero,
                localScale = Vector2.one
            },
            layer = gameObject.layer
        };

        _handsRend = hands.AddComponent<SpriteRenderer>();
        _handsRend.sprite = Assets.Hands.LoadAsset();
        Coroutines.Start(CoUpdateHandColor(Owner, _handsRend));
    }

    private void FixedUpdate()
    {
        if (!Owner || !_rend || !_handsRend) return;

        if (!Owner.AmOwner) // sync shotgun rotation, lerp to make it smoothy
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, ZRotation), Time.deltaTime * 10f);
        }

        var shouldFlip = transform.eulerAngles.z is > 90 and < 270;
        _rend.flipY = _handsRend.flipY = shouldFlip;
        transform.localPosition = new Vector3(shouldFlip ? -0.55f : 0.55f, transform.localPosition.y, transform.localPosition.z);

        if (Owner.MyPhysics.Velocity.sqrMagnitude < 0.0001f)
        {
            Owner.cosmetics.SetFlipX(shouldFlip);
        }
        
        if (!Owner.AmOwner || !Application.isFocused) return;
        
        var vector = Camera.main!.ScreenToWorldPoint(Input.mousePosition) - Owner.transform.position;
        vector.Normalize();

        var num = Mathf.Atan2(vector.y, vector.x);
        if (transform.lossyScale.x < 0f)
        {
            num += 3.1415927f;
        }

        var clamped = ClampAngle(num * 57.29578f, transform.eulerAngles.z);
        transform.rotation = Quaternion.Euler(0f, 0f, clamped);

        if (!(Mathf.Abs(Mathf.DeltaAngle(_lastNetworkedAngle, transform.eulerAngles.z)) > _networkedThreshold)) return;

        _lastNetworkedAngle = (int) transform.eulerAngles.z;
        RpcSyncShotgun(Owner, _lastNetworkedAngle);
    }

    private static float ClampAngle(float angle, float previousAngle)
    {
        const float guard = 10f;

        var upDelta = Mathf.DeltaAngle(90f, angle);
        if (Mathf.Abs(upDelta) < guard)
        {
            return 90f + guard * Mathf.Sign(Mathf.DeltaAngle(90f, previousAngle));
        }

        var downDelta = Mathf.DeltaAngle(270f, angle);
        if (Mathf.Abs(downDelta) < guard)
        {
            return 270f + guard * Mathf.Sign(Mathf.DeltaAngle(270f, previousAngle));
        }

        return angle;
    }

    public static void CreateShotgun(PlayerControl player)
    {
        var shotgunObj = new GameObject("Shotgun")
        {
            transform =
            {
                parent = player.transform,
                localPosition = new Vector3(0.55f, -0.05f, -1),
                localScale = new Vector3(0.55f, 0.55f, 1)
            },
            layer = player.gameObject.layer
        };
        var spriteRend = shotgunObj.AddComponent<SpriteRenderer>();
        spriteRend.sprite = Assets.Shotgun.LoadAsset();

        var shotgun = shotgunObj.AddComponent<ShotgunComponent>();
        shotgun.Owner = player;
    }

    [MethodRpc((uint)CrRpcs.SyncShotgun)]
    public static void RpcSyncShotgun(PlayerControl player, int zRot)
    {
        if (!player.GetPlayerShotgun(out var shotgun)) return;
        shotgun!.ZRotation = zRot;
    }

    private static IEnumerator CoUpdateHandColor(PlayerControl player, SpriteRenderer handsRend)
    {
        yield return new WaitForSeconds(0.5f);
        handsRend.material = player.cosmetics.normalBodySprite.BodySprite.material;
        player.SetPlayerMaterialColors(handsRend);
    }
}