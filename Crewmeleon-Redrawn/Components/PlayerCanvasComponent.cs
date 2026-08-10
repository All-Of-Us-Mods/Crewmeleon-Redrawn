using Crewmeleon_Redrawn.Buttons.Hider;
using Crewmeleon_Redrawn.Modifiers;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Crewmeleon_Redrawn.Components;

[RegisterInIl2Cpp]
public class PlayerCanvasComponent(nint cppPtr) : MonoBehaviour(cppPtr)
{
    public PlayerControl Player { get; set; }
    public Color BrushColor = Color.black;
    
    private static readonly Color OutlineColor = Color.white;

    private readonly IntRange _brushRadiusRange = new(1, 15);
    private int _brushRadius = 3;
    private SpriteRenderer _playerRend;
    private SpriteRenderer _canvasRend;
    private SpriteRenderer _outlineRend;
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

        _outlineRend = CreateOutlineRenderer(playerSprite, pivot);

        _strokes = [];
        _pendingPixels = [];

        gameObject.SetActive(false);
    }

    private SpriteRenderer CreateOutlineRenderer(Sprite playerSprite, Vector2 pivot)
    {
        var outlinePixels = new Color[_texture.width * _texture.height];
        Array.Fill(outlinePixels, Color.clear);

        for (var y = 0; y < _texture.height; y++)
        for (var x = 0; x < _texture.width; x++)
        {
            if (IsPaintable(x, y) && IsOnPaintableBorder(x, y))
            {
                outlinePixels[y * _texture.width + x] = OutlineColor;
            }
        }

        var outlineTexture = new Texture2D(_texture.width, _texture.height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point
        };
        outlineTexture.SetPixels(new Il2CppStructArray<Color>(outlinePixels));
        outlineTexture.Apply();

        var outlineObj = new GameObject("PaintAreaOutline");
        outlineObj.transform.SetParent(transform, false);

        var rend = outlineObj.AddComponent<SpriteRenderer>();
        rend.sortingLayerID = _playerRend.sortingLayerID;
        rend.sortingOrder = _canvasRend.sortingOrder + 1;
        rend.sprite = Sprite.Create(outlineTexture, playerSprite.rect, pivot, playerSprite.pixelsPerUnit);
        rend.enabled = false;

        return rend;
    }

    private bool IsOnPaintableBorder(int x, int y)
    {
        return !IsPaintable(x - 1, y)
            || !IsPaintable(x + 1, y)
            || !IsPaintable(x, y - 1)
            || !IsPaintable(x, y + 1);
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
        if (!Player || !_playerRend || !_canvasRend || !_outlineRend || !_texture)
            return;

        _playerRend.flipX = _canvasRend.flipX = _outlineRend.flipX = Player.cosmetics.FlipX;

        var painting = Player.AmOwner && Player.HasModifier<PaintingModifier>();

        if (_outlineRend.enabled != painting)
        {
            _outlineRend.enabled = painting;
        }

        if (!painting) return;

        HandleBrushRadiusScroll();

        // couldn't have done it in the button itself because buttons dont have update only fixed update
        if (Input.GetMouseButtonDown(0) && CustomButtonSingleton<PickColorButton>.Instance.WaitingForClick)
        {
            Coroutines.Start(CustomButtonSingleton<PickColorButton>.Instance.CoPickColor(this));
            return;
        }

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
            _pendingColor = BrushColor;
            PaintCircle(x, y, _pendingColor);
        }

        _lastPixel = new Vector2Int(x, y);
        _texture.Apply();
    }

    private void HandleBrushRadiusScroll()
    {
        if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)) return;

        var scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel == 0) return;

        _brushRadius = Mathf.Clamp(_brushRadius + (scrollWheel > 0 ? 1 : -1), _brushRadiusRange.min, _brushRadiusRange.max);
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

    private bool TryGetPixelAtMouse(out int x, out int y)
    {
        x = y = 0;

        Vector2 worldMouse;
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
        }
        else
        {
            worldMouse = Camera.main!.ScreenToWorldPoint(Input.mousePosition);
        }

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
        var r = _brushRadius;
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
