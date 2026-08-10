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
    private List<Vector2Int> _pendingPoints;
    private BrushStamp _pendingBrush;
    private Il2CppStructArray<Color32> _buffer;

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

        // kept around so strokes can be alpha-blended and the whole canvas replayed on undo
        _buffer = new Il2CppStructArray<Color32>(source.width * source.height);
        ClearBuffer();
        _texture.SetPixels32(_buffer);
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
        _pendingPoints = [];
        
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

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
        {
            UndoLocalStroke();
            return;
        }

        if (!Input.GetMouseButton(0)
            || !TryGetPixelAtMouse(out var x, out var y)
            || !IsPaintable(x, y))
        {
            if (_lastPixel.HasValue) EndStroke();

            _lastPixel = null;
            return;
        }

        var point = new Vector2Int(x, y);

        if (!_lastPixel.HasValue)
        {
            _pendingBrush = BrushStamp.From(Brush);
            _pendingPoints.Clear();
            _pendingPoints.Add(point);
            StampCircle(point, _pendingBrush);
        }
        else if (_lastPixel.Value != point)
        {
            _pendingPoints.Add(point);
            StampLine(_lastPixel.Value, point, _pendingBrush);
        }

        _lastPixel = point;
        _texture.SetPixels32(_buffer);
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
        if (_pendingPoints.Count == 0) return;

        var stroke = new PaintStroke(_pendingBrush, _pendingPoints.ToArray());
        _pendingPoints.Clear();

        _strokes.Add(stroke);
        Rpc<RpcSendStroke>.Instance.Send(PlayerControl.LocalPlayer, stroke, true);
    }

    public void ApplyStroke(PaintStroke stroke)
    {
        _strokes.Add(stroke);
        Rasterize(stroke);

        _texture.SetPixels32(_buffer);
        _texture.Apply();
    }

    private void UndoLocalStroke()
    {
        if (_strokes.Count == 0) return;

        UndoStroke();
        Rpc<RpcUndoStroke>.Instance.Send(PlayerControl.LocalPlayer, true);
    }

    /// <summary>Drops the last stroke and replays the rest — the canvas holds no per-stroke history.</summary>
    public void UndoStroke()
    {
        if (_strokes.Count == 0) return;

        _strokes.RemoveAt(_strokes.Count - 1);

        ClearBuffer();
        foreach (var stroke in _strokes) Rasterize(stroke);

        _texture.SetPixels32(_buffer);
        _texture.Apply();
    }

    private void Rasterize(PaintStroke stroke)
    {
        if (stroke.Points.Length == 0) return;

        StampCircle(stroke.Points[0], stroke.Brush);
        for (var i = 1; i < stroke.Points.Length; i++)
        {
            StampLine(stroke.Points[i - 1], stroke.Points[i], stroke.Brush);
        }
    }

    private void ClearBuffer()
    {
        var clear = new Color32(0, 0, 0, 0);
        for (var i = 0; i < _buffer.Length; i++) _buffer[i] = clear;
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

    private void StampCircle(Vector2Int center, BrushStamp brush)
    {
        var r = brush.Radius;
        for (var dx = -r; dx <= r; dx++)
        for (var dy = -r; dy <= r; dy++)
        {
            if (dx * dx + dy * dy > r * r) continue;

            int px = center.x + dx, py = center.y + dy;
            if (!IsPaintable(px, py)) continue;

            var alpha = brush.AlphaAt(Mathf.Sqrt(dx * dx + dy * dy));
            if (alpha <= 0f) continue;

            Blend(py * _texture.width + px, brush.Color, alpha);
        }
    }

    private void StampLine(Vector2Int from, Vector2Int to, BrushStamp brush)
    {
        int dx = Mathf.Abs(to.x - from.x), sx = from.x < to.x ? 1 : -1;
        int dy = -Mathf.Abs(to.y - from.y), sy = from.y < to.y ? 1 : -1;
        var err = dx + dy;

        while (true)
        {
            StampCircle(from, brush);
            if (from.x == to.x && from.y == to.y) break;
            var e2 = 2 * err;
            if (e2 >= dy) { err += dy; from.x += sx; }
            if (e2 > dx) continue;
            err += dx;
            from.y += sy;
        }
    }

    private void Blend(int index, Color32 color, float alpha)
    {
        if (index < 0 || index >= _buffer.Length) return;

        var dst = _buffer[index];
        var dstAlpha = dst.a / 255f;
        var outAlpha = alpha + dstAlpha * (1f - alpha);
        if (outAlpha <= 0f) return;

        // straight alpha, so a soft edge over an existing stroke doesn't darken it
        _buffer[index] = new Color32(
            (byte) Mathf.RoundToInt((color.r * alpha + dst.r * dstAlpha * (1f - alpha)) / outAlpha),
            (byte) Mathf.RoundToInt((color.g * alpha + dst.g * dstAlpha * (1f - alpha)) / outAlpha),
            (byte) Mathf.RoundToInt((color.b * alpha + dst.b * dstAlpha * (1f - alpha)) / outAlpha),
            (byte) Mathf.RoundToInt(outAlpha * 255f));
    }
}
