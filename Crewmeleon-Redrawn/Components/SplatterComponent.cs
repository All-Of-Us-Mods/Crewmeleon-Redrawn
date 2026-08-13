using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CrewmeleonRedrawn.Components;

[RegisterInIl2Cpp]
public class SplatterComponent(nint cppPtr) : MonoBehaviour(cppPtr)
{
    private const float FadeDuration = 5f;

    private SpriteRenderer _rend;
    private float _elapsed;
    private float _initialAlpha;

    private void Start()
    {
        _rend = GetComponent<SpriteRenderer>();
        _initialAlpha = _rend.color.a;
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;

        var color = _rend.color;
        color.a = _initialAlpha * Mathf.Clamp01(1f - _elapsed / FadeDuration);
        _rend.color = color;

        if (_elapsed >= FadeDuration)
        {
            Destroy(gameObject);
        }
    }

    public static void CreateSplatter(Vector2 pos, Color32 color, float size)
    {
        var obj = new GameObject("Splatter")
        {
            transform =
            {
                position = new Vector3(pos.x, pos.y, 0.01f),
                localScale = new Vector3(size, size, 1)
            },
            layer = LayerMask.NameToLayer("Players")
        };

        var spriteRenderer = obj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CrewmeleonAssets.SplatterSprites.Random()?.LoadAsset();
        spriteRenderer.color = new Color32(color.r, color.g, color.b, 150);

        obj.AddComponent<SplatterComponent>();
    }
}