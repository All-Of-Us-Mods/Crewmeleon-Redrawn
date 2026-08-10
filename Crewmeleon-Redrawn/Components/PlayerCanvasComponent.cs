using Crewmeleon_Redrawn.Buttons.Hider;
using Crewmeleon_Redrawn.Modifiers;
using Crewmeleon_Redrawn.Utilities;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Crewmeleon_Redrawn.Components;

[RegisterInIl2Cpp]
public class PlayerCanvasComponent(nint cppPtr) : MonoBehaviour(cppPtr)
{
    public PlayerControl Player { get; set; }
    public static BrushSettings Brush => BrushStore.Local;
    
    private const float CursorRingThickness = 3f;

    private SpriteRenderer _playerRend;
    private SpriteRenderer _canvasRend;
    private SpriteRenderer _brushCursor;
    private Texture2D _texture;
    private HashSet<Vector2Int> _unpaintablePixels;
    private Vector2Int? _lastPixel;
    private List<PaintStroke> _strokes;
    private List<Vector2> _pendingPixels;
    private Color _pendingColor;

    private void Start()
    {
        var playerSprite = CrewmeleonAssets.PlayerSprite.LoadAsset();
        var source = playerSprite.texture;
        source.filterMode = FilterMode.Point; // removes outline

        var pivot = new Vector2(
            playerSprite.pivot.x / playerSprite.rect.width,
            playerSprite.pivot.y / playerSprite.rect.height
        );

        _playerRend = gameObject.AddComponent<SpriteRenderer>();
        _playerRend.sprite = Sprite.Create(source, playerSprite.rect, pivot, playerSprite.pixelsPerUnit);

        _texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point // removes outline
        };

        // il2cpp BS to clear a texture lmao
        var clearPixels = new Color[source.width * source.height];
        Array.Fill(clearPixels, Color.clear);
        _texture.SetPixels(new Il2CppStructArray<Color>(clearPixels)); 
        _texture.Apply();

        var overlayObj = new GameObject("Canvas");
        overlayObj.transform.SetParent(transform, false);
        _canvasRend = overlayObj.AddComponent<SpriteRenderer>();
        _canvasRend.sortingLayerID = _playerRend.sortingLayerID;
        _canvasRend.sortingOrder = _playerRend.sortingOrder + 1;
        _canvasRend.sprite = Sprite.Create(_texture, playerSprite.rect, pivot, playerSprite.pixelsPerUnit);
        
        // makes a list of transparent pixels so you cant paint outside the mogus
        var pixels = source.GetPixels();
        _unpaintablePixels = [];
        for (var i = 0; i < pixels.Length; i++)
        {
            if (!(pixels[i].a <= 0)) continue;

            var x = i % _texture.width;
            var y = i / _texture.width;
            _unpaintablePixels.Add(new Vector2Int(x, y));
        }

        _strokes = [];
        _pendingPixels = [];
        
        gameObject.SetActive(false);
    }

    public void Enable()
    {
        gameObject.SetActive(true);
        _playerRend.material = Player.cosmetics.bodySprites[0].BodySprite.material;
        Player.cosmetics.Visible = false;
        Player.cosmetics.lockVisible = true;
    }

    public void Disable()
    {
        Player.cosmetics.lockVisible = false;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!Player || !_playerRend || !_canvasRend || !_texture)
            return;

        _playerRend.flipX = _canvasRend.flipX = Player.cosmetics.FlipX;

        if (!Player.AmOwner || !Player.HasModifier<PaintingModifier>())
        {
            ShowBrushCursor(false);
            return;
        }

        HandleBrushRadiusScroll();
        UpdateBrushCursor();

        // couldn't have done it in the button itself because buttons dont have update only fixed update
        if (CustomButtonSingleton<PickColorButton>.Instance.ShouldCommitPick())
        {
            Coroutines.Start(CustomButtonSingleton<PickColorButton>.Instance.CoPickColor(this));
            return;
        }

        // dragging to pick a colour must not lay down a stroke
        if (CustomButtonSingleton<PickColorButton>.Instance.IsPicking) return;

        if (!Input.GetMouseButton(0) 
            || !TryGetPixelAtMouse(out var x, out var y) 
            || !IsPaintable(x, y))
        {
            if (_lastPixel.HasValue)
            {
                EndStroke();
                _pendingPixels.Clear();
            }

            _lastPixel = null;
            return;
        }

        if (_lastPixel.HasValue)
        {
            PaintLine(_lastPixel.Value, x, y, _pendingColor);
        }
        else
        {
            _pendingColor = Brush.Color;
            PaintCircle(x, y, _pendingColor);
        }

        _lastPixel = new Vector2Int(x, y);
        _texture.Apply();
    }

    private void UpdateBrushCursor()
    {
        if (!_brushCursor) CreateBrushCursor();

        if (CustomButtonSingleton<PickColorButton>.Instance.IsPicking
            || !TryGetWorldMouse(out var worldMouse))
        {
            ShowBrushCursor(false);
            return;
        }

        ShowBrushCursor(true);

        // PaintCircle covers a disc of Brush.Radius texture pixels, and the canvas renders scaled down
        var paintedDiameter = (Brush.Radius * 2 + 1)
                              / _canvasRend.sprite.pixelsPerUnit
                              * _canvasRend.transform.lossyScale.x;
        var scale = paintedDiameter / CircleSprite.DrawnDiameterFraction;

        _brushCursor.transform.position = new Vector3(worldMouse.x, worldMouse.y, _canvasRend.transform.position.z - 0.01f);
        _brushCursor.transform.localScale = new Vector3(scale, scale, 1f);
        _brushCursor.color = Brush.Color;
    }

    private void CreateBrushCursor()
    {
        var cursorObj = new GameObject("BrushCursor") { layer = gameObject.layer };

        _brushCursor = cursorObj.AddComponent<SpriteRenderer>();
        _brushCursor.sprite = CircleSprite.CreateRing(CursorRingThickness);
        _brushCursor.sortingLayerID = _canvasRend.sortingLayerID;
        _brushCursor.sortingOrder = _canvasRend.sortingOrder + 1;

        cursorObj.SetActive(false);
    }

    private void ShowBrushCursor(bool show)
    {
        if (_brushCursor && _brushCursor.gameObject.activeSelf != show)
            _brushCursor.gameObject.SetActive(show);
    }

    private void OnDisable()
    {
        ShowBrushCursor(false);
    }

    private void OnDestroy()
    {
        if (_brushCursor) _brushCursor.gameObject.Destroy();
    }

    private void HandleBrushRadiusScroll()
    {
        if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)) return;

        var scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel == 0) return;

        Brush.Radius += scrollWheel > 0 ? 1 : -1;
    }

    private void EndStroke()
    {
        var pixels = _pendingPixels.ToArray();

        // have to split into multiple chunks cus too much data. TODO: optimize and make this not scuffed
        for (var offset = 0; offset < pixels.Length; offset += 500)
        {
            var chunk = pixels.Skip(offset).Take(500).ToArray();
            var stroke = new PaintStroke(chunk, _pendingColor);
            _strokes.Add(stroke);

            Rpc<RpcSendStroke>.Instance.Send(PlayerControl.LocalPlayer, stroke, true);
        }

        _pendingPixels.Clear();
    }

    public void ApplyStroke(PaintStroke stroke)
    {
        _strokes.Add(stroke);
        foreach (var pixel in stroke.Pixels)
        {
            _texture.SetPixel((int)pixel.x, (int)pixel.y, stroke.Color);
        }

        _texture.Apply();
    }

    private static bool TryGetWorldMouse(out Vector2 worldMouse)
    {
        worldMouse = default;

        var zoom = ZoomCameraController.Instance;

        if (zoom != null && zoom.IsActive)
        {
            var rect = ZoomCameraController.GetRendScreenRect();
            var mouse = Input.mousePosition;
            if (!rect.Contains(mouse)) return false;

            var u = (mouse.x - rect.x) / rect.width;
            var v = (mouse.y - rect.y) / rect.height;

            var cam = zoom.Camera;
            var camPos = cam.transform.position;
            worldMouse = new Vector2(
                camPos.x + (u - 0.5f) * cam.orthographicSize * 2f * cam.aspect,
                camPos.y + (v - 0.5f) * cam.orthographicSize * 2f
            );

            return true;
        }

        worldMouse = Camera.main!.ScreenToWorldPoint(Input.mousePosition);
        return true;
    }

    private bool TryGetPixelAtMouse(out int x, out int y)
    {
        x = y = 0;

        if (!TryGetWorldMouse(out var worldMouse)) return false;

        Vector2 localMouse = transform.InverseTransformPoint(worldMouse);

        if (_canvasRend.flipX) localMouse.x = -localMouse.x;

        var sprite = _canvasRend.sprite;
        var pixel = new Vector2(
            localMouse.x * sprite.pixelsPerUnit + sprite.pivot.x,
            localMouse.y * sprite.pixelsPerUnit + sprite.pivot.y
        );

        x = Mathf.FloorToInt(pixel.x);
        y = Mathf.FloorToInt(pixel.y);

        return x >= 0 && x < _texture.width && y >= 0 && y < _texture.height;
    }

    private bool IsPaintable(int x, int y)
    {
        return x > 0 && x <= _texture.width &&
            y > 0 && y <= _texture.height &&
            !_unpaintablePixels.Contains(new Vector2Int(x, y));
    }

    private void PaintCircle(int cx, int cy, Color color)
    {
        var r = Brush.Radius;
        for (var dx = -r; dx <= r; dx++)
        for (var dy = -r; dy <= r; dy++)
        {
            if (dx * dx + dy * dy > r * r) continue;
            int px = cx + dx, py = cy + dy;
            if (!IsPaintable(px, py)) continue;
            _texture.SetPixel(px, py, color);
            _pendingPixels.Add(new Vector2Int(px, py));
        }
    }

    private void PaintLine(Vector2Int lastPixel, int x1, int y1, Color color)
    {
        int dx = Mathf.Abs(x1 - lastPixel.x), sx = lastPixel.x < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - lastPixel.y), sy = lastPixel.y < y1 ? 1 : -1;
        var err = dx + dy;

        while (true)
        {
            PaintCircle(lastPixel.x, lastPixel.y, color);
            if (lastPixel.x == x1 && lastPixel.y == y1) break;
            var e2 = 2 * err;
            if (e2 >= dy) { err += dy; lastPixel.x += sx; }
            if (e2 > dx) continue;
            err += dx;
            lastPixel.y += sy;
        }
    }
}
