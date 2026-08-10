using Crewmeleon_Redrawn.Modifiers;
using MiraAPI.Modifiers;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Crewmeleon_Redrawn.Components;

[RegisterInIl2Cpp]
public class ZoomCameraController(nint cppPtr) : MonoBehaviour(cppPtr)
{
    public static ZoomCameraController Instance { get; private set; }

    private Camera _zoomCamera;
    private MeshRenderer _zoomRend;
    private RenderTexture _camRenderTex;
    private float _zoomSize = 1f;

    private const float ZoomRendFraction = 0.6f;
    private const float ZoomStep = 1.25f;
    private readonly FloatRange ZoomRange = new (0.3f, 3f);
    public Camera Camera => _zoomCamera;
    public bool IsActive => _zoomCamera && _zoomCamera.gameObject.activeSelf;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        var mainCam = Camera.main!;
        _zoomCamera = gameObject.AddComponent<Camera>();
        _zoomCamera.orthographic = true;
        _zoomCamera.orthographicSize = 1f;
        _zoomCamera.clearFlags = mainCam.clearFlags;
        _zoomCamera.backgroundColor = mainCam.backgroundColor;
        _zoomCamera.nearClipPlane = mainCam.nearClipPlane;
        _zoomCamera.farClipPlane = mainCam.farClipPlane;
        _zoomCamera.cullingMask = mainCam.cullingMask & ~LayerMask.GetMask("UI");
        _zoomCamera.depth = mainCam.depth - 10;

        _camRenderTex = new RenderTexture(512, 512, 16);
        _zoomCamera.targetTexture = _camRenderTex;
        
        var displayObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        displayObj.name = "ZoomCamDisplay";
        displayObj.layer = LayerMask.NameToLayer("UI");
        displayObj.transform.SetParent(mainCam.transform, false);
        displayObj.transform.localPosition = new Vector3(0, 0, 5);
        displayObj.GetComponent<Collider>().Destroy();
        
        _zoomRend = displayObj.GetComponent<MeshRenderer>();
        _zoomRend.material = new Material(Shader.Find("Sprites/Default")) { mainTexture = _camRenderTex };
        _zoomRend.sortingOrder = short.MaxValue;

        UpdateRendDisplaySize();
        
        gameObject.SetActive(false);
        displayObj.SetActive(false);
    }

    public void ToggleDisplay(bool show = true)
    {
       gameObject.SetActive(show);
       _zoomRend.gameObject.SetActive(show);
    }

    private void Update()
    {
        if (!PlayerControl.LocalPlayer.HasModifier<PaintingModifier>() && IsActive)
        {
            ToggleDisplay(false);
            return;
        }

        UpdateRendDisplaySize();
        HandleScrollZoom();
    }

    private void HandleScrollZoom()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) return;

        var scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        var axisRaw = ConsoleJoystick.player.GetAxisRaw(55);

        var zoomIn = false;
        var zoomOut = false;

        if (Input.touchCount == 2)
        {
            var touch0 = Input.GetTouch(0);
            var touch1 = Input.GetTouch(1);

            var touch0PrevPos = touch0.position - touch0.deltaPosition;
            var touch1PrevPos = touch1.position - touch1.deltaPosition;

            var prevTouchDeltaMag = (touch0PrevPos - touch1PrevPos).magnitude;
            var currentTouchDeltaMag = (touch0.position - touch1.position).magnitude;
            var deltaMagnitudeDiff = currentTouchDeltaMag - prevTouchDeltaMag;

            switch (deltaMagnitudeDiff)
            {
                case > 0:
                    zoomIn = true;
                    break;
                case < 0:
                    zoomOut = true;
                    break;
            }
        }

        if (scrollWheel > 0 || axisRaw > 0) zoomIn = true;
        else if (scrollWheel < 0 || axisRaw < 0) zoomOut = true;

        if (!zoomIn && !zoomOut) return;

        var size = _zoomCamera.orthographicSize;
        size = zoomIn ? size / ZoomStep : size * ZoomStep;
        _zoomSize = _zoomCamera.orthographicSize = Mathf.Clamp(size, ZoomRange.min, ZoomRange.max);
    }
    
    private void UpdateRendDisplaySize()
    {
        var worldSize = Camera.main!.orthographicSize * 2f * ZoomRendFraction;
        _zoomRend.transform.localScale = new Vector3(worldSize, worldSize, 1f);
    }
    
    public static Rect GetRendScreenRect()
    {
        var size = Screen.height * ZoomRendFraction;
        var x = (Screen.width - size) / 2f;
        var y = (Screen.height - size) / 2f;
        return new Rect(x, y, size, size);
    }
}