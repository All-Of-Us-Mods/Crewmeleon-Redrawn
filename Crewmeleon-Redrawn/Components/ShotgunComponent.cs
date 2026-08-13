using System.Collections;
using System.Linq;
using Crewmeleon_Redrawn.Networking;
using Crewmeleon_Redrawn.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Crewmeleon_Redrawn.Components;

[RegisterInIl2Cpp]
public class ShotgunComponent(nint cppPtr) : MonoBehaviour(cppPtr)
{
    public PlayerControl Owner;
    public int ZRotation;
        
    private bool _isSplatter;
    private SpriteRenderer _rend;
    private SpriteRenderer _handsRend;
    private SpriteRenderer _muzzleRend;
    private int _lastNetworkedAngle = 0;
    private int _networkedThreshold = 15;
    private float _currentCooldown = 0f;
    private readonly LayerMask _playerLayer = LayerMask.NameToLayer("Players");

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
        _handsRend.sprite = CrewmeleonAssets.Hands.LoadAsset();
        
        var muzzle = new GameObject("Muzzle")
        {
            transform =
            {
                parent = transform,
                localPosition = new Vector3(1.6f, 0.2f, -0.1f),
                localScale = new Vector3(0.5f, 0.5f, 1)
            },
            layer = gameObject.layer
        };

        _muzzleRend = muzzle.AddComponent<SpriteRenderer>();
        _muzzleRend.sprite = CrewmeleonAssets.MuzzleFlash.LoadAsset();
        muzzle.gameObject.SetActive(false);

        Coroutines.Start(CoUpdateHandColor(Owner, _handsRend));
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!Owner.AmOwner) return;

        if (_currentCooldown > 0f)
        {
            _currentCooldown -= Time.deltaTime;
            return;
        }

        if (!Input.GetMouseButtonDown(0)) return;
        if (PassiveButtonManager.Instance.currentOver != null && PassiveButtonManager.Instance.currentOver.gameObject.layer != _playerLayer) return;

        var worldPos = Camera.main!.ScreenToWorldPoint(Input.mousePosition);
        Owner.RpcShootShotgun(worldPos, Palette.PlayerColors.Random(), Random.RandomRange(0.06f, 0.14f));
        _currentCooldown = OptionGroupSingleton<GameplayOptions>.Instance.ShotgunCooldown.Value;
    }

    public IEnumerator CoFlashMuzzle()
    {
        _muzzleRend.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        _muzzleRend.gameObject.SetActive(false);
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
        _muzzleRend.transform.localPosition = new Vector3(_muzzleRend.transform.localPosition.x, shouldFlip ? -0.2f : 0.2f, -0.1f);

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
        Owner.RpcSyncShotgun(_lastNetworkedAngle);
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
                localPosition = new Vector3(0.55f, -0.1f, -1),
                localScale = new Vector3(0.55f, 0.55f, 1)
            },
            layer = player.gameObject.layer
        };
        var spriteRend = shotgunObj.AddComponent<SpriteRenderer>();
        spriteRend.sprite = CrewmeleonAssets.Shotgun.LoadAsset();

        var shotgun = shotgunObj.AddComponent<ShotgunComponent>();
        shotgun.Owner = player;
    }

    private static IEnumerator CoUpdateHandColor(PlayerControl player, SpriteRenderer handsRend)
    {
        yield return new WaitForSeconds(0.5f);
        handsRend.material = player.cosmetics.normalBodySprite.BodySprite.material;
        player.SetPlayerMaterialColors(handsRend);
    }
}