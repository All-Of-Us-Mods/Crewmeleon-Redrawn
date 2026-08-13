using Reactor.Utilities.Extensions;
using UnityEngine;

namespace CrewmeleonRedrawn.Components;

/// <summary>swatch of whatever colour is under the pointer while picking</summary>
public class ColorPickPreview
{
    private const float ScreenHeightFraction = 0.09f;
    private const float PointerClearanceFraction = 0.06f;

    private SpriteRenderer? _renderer;

    public void Show(Vector2 screenPosition, Color color)
    {
        _renderer ??= Create();

        var cam = Camera.main!;
        
        var offsetY = Screen.height * PointerClearanceFraction;
        var world = cam.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y + offsetY, 1f));
        world.z = cam.transform.position.z + 1f;

        var size = cam.orthographicSize * 2f * ScreenHeightFraction;

        _renderer.transform.position = world;
        _renderer.transform.localScale = new Vector3(size, size, 1f);
        _renderer.color = color;

        if (!_renderer.gameObject.activeSelf)
            _renderer.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (_renderer && _renderer!.gameObject.activeSelf)
            _renderer.gameObject.SetActive(false);
    }

    public void Destroy()
    {
        if (_renderer) _renderer!.gameObject.Destroy();
        _renderer = null;
    }

    private static SpriteRenderer Create()
    {
        // UI layer is culled by the zoom camera so this stays out of the zoomed view
        var previewObj = new GameObject("ColorPickPreview") { layer = LayerMask.NameToLayer("UI") };

        var renderer = previewObj.AddComponent<SpriteRenderer>();
        renderer.sprite = CrewmeleonAssets.ColorSwatch.LoadAsset();
        renderer.sortingOrder = short.MaxValue;

        previewObj.SetActive(false);

        return renderer;
    }
}
