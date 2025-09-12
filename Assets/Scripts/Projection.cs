using UnityEngine;
using System.Collections.Generic;

public class Projection : MonoBehaviour
{
    public Camera projectionCamera;
    public Transform handlesContainer;
    public Transform cursorCross;
    public Material renderMaterial;
    public GameObject shortcutsScreen;
    public GameObject renderTextureGameObject;

    private Vector2 _cursorPosition = new(Screen.width / 2.0f, Screen.height / 2.0f);
    private bool _cursorXFlip;
    private bool _cursorYFlip;

    private bool _cornerEditModeOn = true;
    private Transform _currentlyDraggingHandle;
    private bool IsShowingShortcuts => shortcutsScreen.activeSelf;
    private bool CanEditCorners => !IsShowingShortcuts && _cornerEditModeOn;

    private readonly List<Transform> _handles = new();
    private Mesh _planeMesh;
    private Plane _plane = new(Vector3.up, Vector3.zero);

    private static readonly int Q = Shader.PropertyToID("_Q");


    private void Start()
    {
        RecenterCursor();
        SetupRenderTexturePlane(renderTextureGameObject, renderMaterial, 2, 2, out _planeMesh);
        foreach (Transform children in handlesContainer)
            _handles.Add(children);
        LoadHandlesPlayerPrefs();
        UpdatePlane();
    }

    public void ChangeDisplay(int i)
    {
        var index = i - 1;

        if (index < 0 || index >= Display.displays.Length)
        {
            Debug.LogWarning($"Display {i} does not exist.");
            return;
        }

        Display.displays[index].Activate();
        projectionCamera.targetDisplay = index;
    }

    public void ToggleShortcutScreen()
    {
        shortcutsScreen.SetActive(!shortcutsScreen.activeSelf);
    }

    public void FlipImageX()
    {
        foreach (var handle in _handles)
        {
            handle.localPosition = new Vector3(
                -handle.localPosition.x,
                handle.localPosition.y,
                handle.localPosition.z);
        }

        UpdatePlane();
    }

    public void FlipImageY()
    {
        foreach (var handle in _handles)
        {
            handle.localPosition = new Vector3(
                handle.localPosition.x,
                handle.localPosition.y,
                -handle.localPosition.z);
        }

        UpdatePlane();
    }


    #region Corner Editing

    public void ToggleCornerEditing()
    {
        if (IsShowingShortcuts) return;

        _cornerEditModeOn = !_cornerEditModeOn;

        foreach (Transform t in cursorCross)
            t.GetComponent<MeshRenderer>().enabled = _cornerEditModeOn;
        foreach (Transform t in _handles)
            t.GetComponent<MeshRenderer>().enabled = _cornerEditModeOn;
    }


    public void MoveCursorPosition(Vector2 delta)
    {
        if (!CanEditCorners) return;

        SetCursorPosition(new Vector2(
            _cursorPosition.x + delta.x * (_cursorXFlip ? -1 : 1),
            _cursorPosition.y + delta.y * (_cursorYFlip ? -1 : 1)));
    }

    private void SetCursorPosition(Vector2 position)
    {
        if (!CanEditCorners) return;

        _cursorPosition = position;

        var ray = projectionCamera.ScreenPointToRay(_cursorPosition);
        cursorCross.position = ray.origin - projectionCamera.transform.position + Vector3.up;

        if (!_currentlyDraggingHandle || !_plane.Raycast(ray, out _)) return;

        var intersection = ray.origin - projectionCamera.transform.position;
        _currentlyDraggingHandle.position = intersection;

        UpdatePlane();
    }

    public void RecenterCursor()
    {
        if (!CanEditCorners) return;

        SetCursorPosition(new Vector2(Screen.width / 2.0f, Screen.height / 2.0f));
    }

    public void FlipCursorX()
    {
        if (!CanEditCorners) return;

        _cursorXFlip = !_cursorXFlip;
    }

    public void FlipCursorY()
    {
        if (!CanEditCorners) return;

        _cursorYFlip = !_cursorYFlip;
    }

    public void TryGrabHandle()
    {
        if (!CanEditCorners) return;

        var ray = projectionCamera.ScreenPointToRay(_cursorPosition);
        if (!Physics.Raycast(ray, out var hit)) return;

        if (hit.transform.CompareTag("CornerHandle"))
            _currentlyDraggingHandle = hit.transform;
    }

    public void TryReleaseHandle()
    {
        if (!CanEditCorners) return;

        if (_currentlyDraggingHandle == null) return;

        SaveHandlePositions();
        _currentlyDraggingHandle = null;
    }


    private void SaveHandlePositions()
    {
        for (var i = 0; i < _handles.Count; i++)
        {
            PlayerPrefs.SetFloat($"Handle {i} X", _handles[i].position.x);
            PlayerPrefs.SetFloat($"Handle {i} Y", _handles[i].position.z);
        }
    }

    public void ResetHandlePositions()
    {
        if (!CanEditCorners) return;

        _handles[0].localPosition = new Vector3(-5, 0, -5);
        _handles[1].localPosition = new Vector3(-5, 0, 5);
        _handles[2].localPosition = new Vector3(5, 0, 5);
        _handles[3].localPosition = new Vector3(5, 0, -5);
        SaveHandlePositions();
    }

    private void LoadHandlesPlayerPrefs()
    {
        for (var i = 0; i < _handles.Count; i++)
        {
            if (!PlayerPrefs.HasKey($"Handle {i} X") || !PlayerPrefs.HasKey($"Handle {i} X")) continue;

            var pos = Vector3.zero;
            pos.x = PlayerPrefs.GetFloat($"Handle {i} X");
            pos.z = PlayerPrefs.GetFloat($"Handle {i} Y");
            _handles[i].localPosition = pos;
        }
    }

    #endregion

    #region Render Texture Plane

    private void UpdatePlane()
    {
        var vertices = new Vector3[4];
        for (var i = 0; i < _handles.Count; i++)
            vertices[i] = _handles[i].localPosition;
        _planeMesh.SetVertices(vertices);
        UpdateShaderQ();
        return;

        void UpdateShaderQ()
        {
            var arr = new float[4];

            var x1 = _handles[1].localPosition.x;
            var y1 = _handles[1].localPosition.z;
            var x2 = _handles[3].localPosition.x;
            var y2 = _handles[3].localPosition.z;
            var x3 = _handles[0].localPosition.x;
            var y3 = _handles[0].localPosition.z;
            var x4 = _handles[2].localPosition.x;
            var y4 = _handles[2].localPosition.z;

            var center = DiagCenter();

            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = GetQ(i);
            }

            renderMaterial.SetFloatArray(Q, arr);
            return;

            float GetQ(int index)
            {
                var oppositeIndex = (index + 2) % 4;
                var main = new Vector2(_handles[index].localPosition.x, _handles[index].localPosition.z);
                var opposite = new Vector2(_handles[oppositeIndex].localPosition.x,
                    _handles[oppositeIndex].localPosition.z);
                var d = Vector3.Distance(main, center);
                var d2 = Vector3.Distance(opposite, center);
                return ((d + d2) / d2);
            }

            Vector2 DiagCenter()
            {
                var v = Vector2.zero;

                var xNum = (x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4);
                var xDen = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

                var yNum = (x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4);
                var yDen = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

                v.x = xNum / xDen;
                v.y = yNum / yDen;
                return v;
            }
        }
    }

    private static void SetupRenderTexturePlane(
        GameObject gameObject,
        Material renderMaterial,
        float width, float height,
        out Mesh createdMesh)
    {
        createdMesh = new Mesh
        {
            name = "ScriptedMesh",
            vertices = new Vector3[]
            {
                new(-width, -height, 0),
                new(-width, height, 0),
                new(width, height, 0),
                new(width, -height, 0)
            },
            uv = new Vector2[]
            {
                new(0, 0),
                new(0, 1),
                new(1, 1),
                new(1, 0)
            },
            triangles = new[] { 0, 1, 2, 0, 2, 3 }
        };
        createdMesh.RecalculateNormals();

        gameObject.GetComponent<MeshFilter>().mesh = createdMesh;
        gameObject.GetComponent<MeshRenderer>().material = renderMaterial;
    }

    #endregion
}