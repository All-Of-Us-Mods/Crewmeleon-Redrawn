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
    
    // the ring in BrushCursor.png stops a pixel short of the sprite edge
    private const float CursorEdgeFraction = 62f / 64f;

    // how far off canvas the brush centre can go and still paint.
    // allows people to use the edge of the brush to paint instead of the center
    private const int MaxOffCanvas = 32;

    private SpriteRenderer _playerRend;
    private SpriteRenderer _canvasRend;
    private SpriteRenderer _brushCursor;
    private Texture2D _texture;
    private Vector2Int? _lastPixel;

    // the click that finishes a colour pick shouldnt also start a stroke
    private bool _paintBlockedUntilRelease;
    private List<PaintStroke> _strokes;

    // undo replays from here rather than from a blank canvas. once the list passes UndoDepth the
    // oldest stroke is baked in and dropped, so a replay is bounded no matter how long the game runs
    private const int UndoDepth = 24;
    private Color32[] _flattened;
    private List<Vector2Int> _pendingPoints;
    private BrushStamp _pendingBrush;

    private int _width;
    private int _height;
    
    private Color32[] _buffer;
    private bool[] _paintable;

    // a stroke draws as one layer, stamps build up coverage and colour goes down once on top of
    // the pre stroke canvas. blending each stamp on its own pushed a 50% brush to ~99%
    private Color32[] _baseBuffer;
    private float[] _coverage;
    private bool[] _touchedFlag;
    private readonly List<int> _touched = [];
    private readonly List<int> _recompose = [];

    // falloff weights for the hardness slider, (2r+1)^2
    private float[] _kernel = [];
    private int _kernelRadius = -1;
    private byte _kernelHardness;

    // mark modified area dirty to avoid having to upload just the touched region each frame, not all 34k pixels
    private int _dirtyMinX, _dirtyMinY, _dirtyMaxX, _dirtyMaxY;
    private bool _dirty;

    private bool _initialized;

    private void Start() => EnsureInitialized();

    /// <summary>
    /// Unity only runs Start on an active object and not until next frame. role assignment comes
    /// in on an RPC and touches the canvas straight away, and throwing there drops the connection,
    /// so setup has to be callable whenever
    /// </summary>
    private void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;

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

            // default is Repeat, which wraps a stray out of range write to the far side
            wrapMode = TextureWrapMode.Clamp
        };

        _width = source.width;
        _height = source.height;

        // kept so strokes can blend and so undo can replay the whole canvas
        _buffer = new Color32[_width * _height];
        _baseBuffer = new Color32[_width * _height];
        _flattened = new Color32[_width * _height];
        _coverage = new float[_width * _height];
        _touchedFlag = new bool[_width * _height];
        ClearBuffer();
        _texture.SetPixels32(new Il2CppStructArray<Color32>(_buffer));
        _texture.Apply();

        var overlayObj = new GameObject("Canvas");
        overlayObj.transform.SetParent(transform, false);
        overlayObj.transform.localPosition = new Vector3(0, 0, -0.01f);
        _canvasRend = overlayObj.AddComponent<SpriteRenderer>();
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
        EnsureInitialized();

        gameObject.SetActive(true);
        _playerRend.material = Player.cosmetics.bodySprites[0].BodySprite.material;
        Player.cosmetics.Visible = false;
        Player.cosmetics.lockVisible = true;
    }

    public void Disable()
    {
        if (!_initialized) return;

        Player.cosmetics.lockVisible = false;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_initialized || !Player || !_playerRend || !_canvasRend || !_texture)
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

            // CoPickColor clears IsPicking before it yields so without this the button youre
            // still holding starts painting next frame
            _paintBlockedUntilRelease = true;
            FinishAnyStroke();
            return;
        }

        // dragging to pick a colour shouldnt leave a stroke behind
        if (CustomButtonSingleton<PickColorButton>.Instance.IsPicking) return;

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
        {
            UndoLastLocalStroke();
            return;
        }

        if (Input.GetKeyDown(PickColorButton.PickKey))
        {
            // otherwise a stroke interrupted mid drag never gets sent or recorded
            FinishAnyStroke();
            CustomButtonSingleton<PickColorButton>.Instance.BeginPick(fromKey: true);
            return;
        }

        if (_paintBlockedUntilRelease)
        {
            if (!Input.GetMouseButton(0)) _paintBlockedUntilRelease = false;
            return;
        }

        if (!Input.GetMouseButton(0)
            || !TryGetPixelAtMouse(out var x, out var y))
        {
            if (_lastPixel.HasValue) EndStroke();

            _lastPixel = null;
            return;
        }

        // centre can sit off the body. StampCircle masks per pixel so only drawable ones change
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

        // StampCircle covers Brush.Radius texture pixels and the canvas renders scaled down
        var paintedDiameter = (Brush.Radius * 2 + 1)
                              / _canvasRend.sprite.pixelsPerUnit
                              * _canvasRend.transform.lossyScale.x;
        var scale = paintedDiameter / CursorEdgeFraction;

        _brushCursor.transform.position = new Vector3(worldMouse.x, worldMouse.y, _canvasRend.transform.position.z - 0.01f);
        _brushCursor.transform.localScale = new Vector3(scale, scale, 1f);
        _brushCursor.color = Brush.Color;
    }

    private void CreateBrushCursor()
    {
        var cursorObj = new GameObject("BrushCursor") { layer = gameObject.layer };

        _brushCursor = cursorObj.AddComponent<SpriteRenderer>();
        _brushCursor.sprite = CrewmeleonAssets.BrushCursor.LoadAsset();

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

    // max points per stroke packet chunk.
    private const int PointsPerChunk = 120;

    private static void SendStroke(BrushStamp brush, Vector2Int[] points)
    {
        var local = PlayerControl.LocalPlayer;

        Rpc<RpcSendStroke>.Instance.Send(local,
            new StrokeChunk(true, points.Length == 0, brush, []), true);

        for (var offset = 0; offset < points.Length; offset += PointsPerChunk)
        {
            var take = Mathf.Min(PointsPerChunk, points.Length - offset);
            var chunk = new Vector2Int[take];
            Array.Copy(points, offset, chunk, 0, take);

            Rpc<RpcSendStroke>.Instance.Send(local,
                new StrokeChunk(false, offset + take >= points.Length, brush, chunk), true);
        }
    }

    /// <summary>finishes whatever stroke is in progress, for input that interrupts a drag</summary>
    private void FinishAnyStroke()
    {
        if (_lastPixel.HasValue) EndStroke();
        _lastPixel = null;
    }

    private void EndStroke()
    {
        if (_pendingPoints.Count == 0) return;

        var points = _pendingPoints.ToArray();
        _pendingPoints.Clear();
        ClearStrokeState();

        TrimHistory();

        _strokes.Add(new PaintStroke(_pendingBrush, points));
        SendStroke(_pendingBrush, points);
    }

    private BrushStamp _remoteBrush;
    private readonly List<Vector2Int> _remotePoints = [];

    public void BeginRemoteStroke(BrushStamp brush)
    {
        EnsureInitialized();

        _remoteBrush = brush;
        _remotePoints.Clear();
    }

    public void AppendRemoteStroke(Vector2Int[] points) => _remotePoints.AddRange(points);

    public void FinishRemoteStroke()
    {
        if (_remotePoints.Count == 0) return;

        ApplyStroke(new PaintStroke(_remoteBrush, _remotePoints.ToArray()));
        _remotePoints.Clear();
    }

    public void ApplyStroke(PaintStroke stroke)
    {
        EnsureInitialized();

        TrimHistory();

        _strokes.Add(stroke);
        Rasterize(stroke);

        Flush();
    }

    /// <summary>undo from outside the input loop, so a half drawn stroke still lands first</summary>
    public void UndoLastLocalStroke()
    {
        FinishAnyStroke();
        UndoLocalStroke();
    }

    private void UndoLocalStroke()
    {
        if (_strokes.Count == 0) return;

        UndoStroke();
        Rpc<RpcUndoStroke>.Instance.Send(PlayerControl.LocalPlayer, true);
    }

    /// <summary>drops the last stroke and replays whats left on top of the flattened canvas</summary>
    public void UndoStroke()
    {
        EnsureInitialized();

        if (_strokes.Count == 0) return;

        _strokes.RemoveAt(_strokes.Count - 1);

        Array.Copy(_flattened, _buffer, _buffer.Length);
        MarkDirty(0, 0);
        MarkDirty(_width - 1, _height - 1);

        foreach (var stroke in _strokes) Rasterize(stroke);

        Flush();
    }

    /// <summary>bakes the oldest stroke into the flattened canvas once history gets too deep</summary>
    private void TrimHistory()
    {
        if (_strokes.Count < UndoDepth) return;

        var oldest = _strokes[0];
        _strokes.RemoveAt(0);
        
        var live = _buffer;
        _buffer = _flattened;

        Rasterize(oldest);

        _flattened = _buffer;
        _buffer = live;
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
        if (_flattened != null) Array.Clear(_flattened, 0, _flattened.Length);
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

    /// <summary>uploads just the region touched since the last flush</summary>
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
    /// falloff weights for the current radius and hardness, rebuilt only when those change. keeps
    /// the sqrt and curve maths to once per brush instead of once per stamped pixel
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
            _kernel[(dy + r) * size + (dx + r)] = brush.WeightAt(dx, dy);
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

        // stamping every pixel along the line is redundant, quarter radius looks the same
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

    /// <summary>snapshot of the canvas so the in progress stroke can be redrawn each frame</summary>
    private void BeginStroke()
    {
        Array.Copy(_buffer, _baseBuffer, _buffer.Length);
        ClearStrokeState();
    }

    /// <summary>puts the strokes built up coverage down over the pre stroke canvas</summary>
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
                // straight alpha, otherwise a soft edge darkens whatever it lands on
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
