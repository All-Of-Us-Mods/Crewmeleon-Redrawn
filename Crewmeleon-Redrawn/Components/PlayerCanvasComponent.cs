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

    // how far the brush centre may sit outside the canvas and still paint; only needs to exceed
    // the largest radius, and keeps stroke points inside what the RPC can encode
    private const int MaxOffCanvas = 32;

    private SpriteRenderer _playerRend;
    private SpriteRenderer _canvasRend;
    private SpriteRenderer _brushCursor;
    private Texture2D _texture;
    private Vector2Int? _lastPixel;
    private List<PaintStroke> _strokes;
    private List<Vector2Int> _pendingPoints;
    private BrushStamp _pendingBrush;

    private int _width;
    private int _height;

    // managed, not Il2CppStructArray: every blend reads and writes a pixel, and crossing the
    // interop boundary per element dominated the cost at large brush sizes
    private Color32[] _buffer;
    private bool[] _paintable;

    // a stroke composites as a single layer: stamps accumulate coverage, and the colour is laid
    // over the pre-stroke canvas once. Blending each stamp separately drove a 50% brush to ~99%.
    private Color32[] _baseBuffer;
    private float[] _coverage;
    private bool[] _touchedFlag;
    private readonly List<int> _touched = [];
    private readonly List<int> _recompose = [];

    // (2r+1)^2 falloff weights, rebuilt only when radius or hardness changes
    private float[] _kernel = [];
    private int _kernelRadius = -1;
    private byte _kernelHardness;

    // only the touched region is uploaded each frame instead of all 34k pixels
    private int _dirtyMinX, _dirtyMinY, _dirtyMaxX, _dirtyMaxY;
    private bool _dirty;

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
            filterMode = FilterMode.Point, // removes outline

            // Repeat is the default; any stray out-of-range write would wrap a brush across to
            // the opposite edge of the player
            wrapMode = TextureWrapMode.Clamp
        };

        _width = source.width;
        _height = source.height;

        // kept around so strokes can be alpha-blended and the whole canvas replayed on undo
        _buffer = new Color32[_width * _height];
        _baseBuffer = new Color32[_width * _height];
        _coverage = new float[_width * _height];
        _touchedFlag = new bool[_width * _height];
        ClearBuffer();
        _texture.SetPixels32(new Il2CppStructArray<Color32>(_buffer));
        _texture.Apply();

        var overlayObj = new GameObject("Canvas");
        overlayObj.transform.SetParent(transform, false);
        _canvasRend = overlayObj.AddComponent<SpriteRenderer>();
        _canvasRend.sortingLayerID = _playerRend.sortingLayerID;
        _canvasRend.sortingOrder = _playerRend.sortingOrder + 1;
        _canvasRend.sprite = Sprite.Create(_texture, playerSprite.rect, pivot, playerSprite.pixelsPerUnit);
        
        // makes a list of transparent pixels so you cant paint outside the mogus
        var pixels = source.GetPixels();
        _paintable = new bool[_width * _height];
        for (var i = 0; i < pixels.Length; i++) _paintable[i] = pixels[i].a > 0;

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
            || !TryGetPixelAtMouse(out var x, out var y))
        {
            if (_lastPixel.HasValue) EndStroke();

            _lastPixel = null;
            return;
        }

        // the centre may sit off the body — StampCircle masks per pixel, so only drawable
        // pixels under the brush are touched
        var point = new Vector2Int(
            Mathf.Clamp(x, -MaxOffCanvas, _width + MaxOffCanvas),
            Mathf.Clamp(y, -MaxOffCanvas, _height + MaxOffCanvas));

        if (!_lastPixel.HasValue)
        {
            _pendingBrush = BrushStamp.From(Brush);
            _pendingPoints.Clear();
            _pendingPoints.Add(point);

            BeginStroke();
            StampCircle(point, _pendingBrush);
        }
        else if (_lastPixel.Value != point)
        {
            _pendingPoints.Add(point);
            StampLine(_lastPixel.Value, point, _pendingBrush);
        }

        CompositeStroke(_pendingBrush);

        _lastPixel = point;
        Flush();
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
        ClearStrokeState();

        _strokes.Add(stroke);
        Rpc<RpcSendStroke>.Instance.Send(PlayerControl.LocalPlayer, stroke, true);
    }

    public void ApplyStroke(PaintStroke stroke)
    {
        _strokes.Add(stroke);
        Rasterize(stroke);

        Flush();
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

        Flush();
    }

    private void Rasterize(PaintStroke stroke)
    {
        if (stroke.Points.Length == 0) return;

        BeginStroke();

        StampCircle(stroke.Points[0], stroke.Brush);
        for (var i = 1; i < stroke.Points.Length; i++)
        {
            StampLine(stroke.Points[i - 1], stroke.Points[i], stroke.Brush);
        }

        CompositeStroke(stroke.Brush);
        ClearStrokeState();
    }

    private void ClearBuffer()
    {
        Array.Clear(_buffer, 0, _buffer.Length);
        MarkDirty(0, 0);
        MarkDirty(_width - 1, _height - 1);
    }

    private void MarkDirty(int x, int y)
    {
        if (!_dirty)
        {
            _dirtyMinX = _dirtyMaxX = x;
            _dirtyMinY = _dirtyMaxY = y;
            _dirty = true;
            return;
        }

        if (x < _dirtyMinX) _dirtyMinX = x;
        if (x > _dirtyMaxX) _dirtyMaxX = x;
        if (y < _dirtyMinY) _dirtyMinY = y;
        if (y > _dirtyMaxY) _dirtyMaxY = y;
    }

    /// <summary>Uploads only the region touched since the last flush.</summary>
    private void Flush()
    {
        if (!_dirty) return;

        var w = _dirtyMaxX - _dirtyMinX + 1;
        var h = _dirtyMaxY - _dirtyMinY + 1;

        var block = new Color32[w * h];
        for (var row = 0; row < h; row++)
        {
            Array.Copy(_buffer, (_dirtyMinY + row) * _width + _dirtyMinX, block, row * w, w);
        }

        _texture.SetPixels32(_dirtyMinX, _dirtyMinY, w, h, new Il2CppStructArray<Color32>(block));
        _texture.Apply(false);

        _dirty = false;
    }

    /// <summary>
    /// Falloff weights for the current radius/hardness. Rebuilt only when those change, so the
    /// per-pixel sqrt and curve maths happen once per brush rather than once per stamped pixel.
    /// </summary>
    private void EnsureKernel(BrushStamp brush)
    {
        if (_kernelRadius == brush.Radius && _kernelHardness == brush.Hardness) return;

        _kernelRadius = brush.Radius;
        _kernelHardness = brush.Hardness;

        var r = brush.Radius;
        var size = 2 * r + 1;
        _kernel = new float[size * size];

        for (var dy = -r; dy <= r; dy++)
        for (var dx = -r; dx <= r; dx++)
        {
            var distance = Mathf.Sqrt(dx * dx + dy * dy);
            _kernel[(dy + r) * size + (dx + r)] = distance > r ? 0f : brush.FalloffAt(distance);
        }
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

        return true;
    }

    private bool IsPaintable(int x, int y)
    {
        return x >= 0 && x < _width && y >= 0 && y < _height && _paintable[y * _width + x];
    }

    private void StampCircle(Vector2Int center, BrushStamp brush)
    {
        EnsureKernel(brush);

        var r = brush.Radius;
        var size = 2 * r + 1;

        var minX = Mathf.Max(center.x - r, 0);
        var maxX = Mathf.Min(center.x + r, _width - 1);
        var minY = Mathf.Max(center.y - r, 0);
        var maxY = Mathf.Min(center.y + r, _height - 1);

        for (var py = minY; py <= maxY; py++)
        {
            var rowOffset = py * _width;
            var kernelRow = (py - center.y + r) * size + r - center.x;

            for (var px = minX; px <= maxX; px++)
            {
                var index = rowOffset + px;
                if (!_paintable[index]) continue;

                var weight = _kernel[kernelRow + px];
                if (weight <= _coverage[index]) continue;

                _coverage[index] = weight;
                _recompose.Add(index);

                if (_touchedFlag[index]) continue;
                _touchedFlag[index] = true;
                _touched.Add(index);
            }
        }
    }

    private void StampLine(Vector2Int from, Vector2Int to, BrushStamp brush)
    {
        int dx = Mathf.Abs(to.x - from.x), sx = from.x < to.x ? 1 : -1;
        int dy = -Mathf.Abs(to.y - from.y), sy = from.y < to.y ? 1 : -1;
        var err = dx + dy;

        // overlapping discs are redundant; quarter-radius spacing is visually identical
        var spacing = Mathf.Max(1, brush.Radius / 4);
        var step = 0;

        while (true)
        {
            var atEnd = from.x == to.x && from.y == to.y;
            if (step % spacing == 0 || atEnd) StampCircle(from, brush);
            if (atEnd) break;

            step++;
            var e2 = 2 * err;
            if (e2 >= dy) { err += dy; from.x += sx; }
            if (e2 > dx) continue;
            err += dx;
            from.y += sy;
        }
    }

    /// <summary>Snapshots the canvas so the in-progress stroke can be recomposited each frame.</summary>
    private void BeginStroke()
    {
        Array.Copy(_buffer, _baseBuffer, _buffer.Length);
        ClearStrokeState();
    }

    /// <summary>Lays the stroke's accumulated coverage over the pre-stroke canvas.</summary>
    private void CompositeStroke(BrushStamp brush)
    {
        var opacity = brush.Opacity / 255f;
        var color = brush.Color;

        foreach (var index in _recompose)
        {
            var alpha = _coverage[index] * opacity;

            var dst = _baseBuffer[index];
            var dstAlpha = dst.a / 255f;
            var outAlpha = alpha + dstAlpha * (1f - alpha);

            if (outAlpha <= 0f)
            {
                _buffer[index] = new Color32(0, 0, 0, 0);
            }
            else
            {
                // straight alpha, so a soft edge over an existing stroke doesn't darken it
                _buffer[index] = new Color32(
                    (byte) Mathf.RoundToInt((color.r * alpha + dst.r * dstAlpha * (1f - alpha)) / outAlpha),
                    (byte) Mathf.RoundToInt((color.g * alpha + dst.g * dstAlpha * (1f - alpha)) / outAlpha),
                    (byte) Mathf.RoundToInt((color.b * alpha + dst.b * dstAlpha * (1f - alpha)) / outAlpha),
                    (byte) Mathf.RoundToInt(outAlpha * 255f));
            }

            MarkDirty(index % _width, index / _width);
        }

        _recompose.Clear();
    }

    private void ClearStrokeState()
    {
        foreach (var index in _touched)
        {
            _coverage[index] = 0f;
            _touchedFlag[index] = false;
        }

        _touched.Clear();
        _recompose.Clear();
    }
}
