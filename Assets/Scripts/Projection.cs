using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Projection : MonoBehaviour
{
    Transform heldHandle;
    public Transform[] handles;

    public Transform cross;
    Camera cam;
    Mesh planeMesh;
    GameObject planeObject;
    public Material renderMat;
    Plane plane = new Plane(Vector3.up, Vector3.zero);
    Vector2 mousePosition;
    bool mouseXflip;
    bool mouseYflip;
    public RawImage white;

    void Start()
    {
        cam = Camera.main;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        mousePosition = new Vector2(Screen.width / 2, Screen.height / 2);

        planeMesh = CreateMesh(2, 2);
        planeObject = CreateObject(planeMesh);

        if (PlayerPrefs.HasKey("Handle" + 0 + "x"))
            PlayerPrefLoadAll();
        else
            PlayerPrefSaveAll();

    }

    void Update()
    {

        mousePosition.x += Input.GetAxisRaw("Mouse X") * 30 * (mouseXflip ? -1 : 1);
        mousePosition.y += Input.GetAxisRaw("Mouse Y") * 30 * (mouseYflip ? -1 : 1);
        if (Input.GetKeyDown(KeyCode.Home))
            mousePosition = new Vector2(Screen.width / 2, Screen.height / 2);
        if (Input.GetKeyDown(KeyCode.Delete))
            SceneManager.LoadScene(0);
        if (Input.GetKeyDown(KeyCode.Insert))
            white.enabled = !white.enabled;
        if (Input.GetKeyDown(KeyCode.Backspace))
            ResetPlayerPref();
        if (Input.GetKeyDown(KeyCode.X))
            mouseXflip = !mouseXflip;
        if (Input.GetKeyDown(KeyCode.Y))
            mouseYflip = !mouseYflip;

        if (Input.GetKeyDown(KeyCode.Space))
            ToggleHandles();
        EscapeInputUpdate();
        DisplayInputUpdate();
        HandlesUpdatee();
        PlaneUpdate();
    }

    void EscapeInputUpdate()
    {
        if (Input.GetKey("escape"))
            Application.Quit();
    }

    void DisplayInputUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
            ChangeDisplay(0);
        if (Input.GetKeyDown(KeyCode.Alpha1))
            ChangeDisplay(1);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            ChangeDisplay(2);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            ChangeDisplay(3);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            ChangeDisplay(4);
    }

    void ChangeDisplay(int i)
    {
        if (Display.displays.Length > i)
        {
            Display.displays[i].Activate();
            cam.targetDisplay = i;
        }
    }

    Mesh CreateMesh(float width, float height)
    {
        Mesh m = new Mesh();
        m.name = "ScriptedMesh";
        m.vertices = new Vector3[]
    {
         new Vector3(-width, -height, 0),
         new Vector3(-width, height, 0),
         new Vector3(width, height, 0),
         new Vector3(width, -height, 0)
 };
        m.uv = new Vector2[]
{
         new Vector2 (0, 0),
         new Vector2 (0, 1),
         new Vector2(1, 1),
         new Vector2 (1, 0)
 };
        m.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        m.RecalculateNormals();

        return m;
    }

    GameObject CreateObject(Mesh m)
    {
        var plane = new GameObject("Plane");
        MeshFilter meshFilter = (MeshFilter)plane.AddComponent(typeof(MeshFilter));
        meshFilter.mesh = m;
        MeshRenderer renderer = plane.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        renderer.material = renderMat;

        return plane;
    }

    void UpdateVerticies(Vector3[] verticies)
    {
        planeMesh.SetVertices(verticies);
    }

    void HandlesUpdatee()
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(mousePosition);
        cross.position = ray.origin - cam.transform.position + Vector3.up;

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag == "CornerHandle")
                {
                    heldHandle = hit.transform;
                }
            }
        }
        if (heldHandle != null)
        {
            int i = Array.IndexOf(handles, heldHandle);

            float dist;
            if (plane.Raycast(ray, out dist))
            {
                Vector3 intersection = ray.origin - cam.transform.position;
                heldHandle.position = intersection;
                SetShader();
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (i != -1)
                    PlayerPrefSave(i);
                heldHandle = null;
            }
        }
    }

    void PlaneUpdate()
    {
        Vector3[] verts = new Vector3[4];
        for (int i = 0; i < handles.Length; i++)
            verts[i] = handles[i].position;
        UpdateVerticies(verts);
    }

    bool visible = true;
    void ToggleHandles()
    {
        visible = !visible;
        SetHandlesVisibility(visible);
    }

    void SetHandlesVisibility(bool visible)
    {
        foreach (Transform t in cross)
            t.GetComponent<MeshRenderer>().enabled = visible;
        foreach (Transform t in handles)
            t.GetComponent<MeshRenderer>().enabled = visible;
    }

    void PlayerPrefLoad(int index)
    {
        Vector3 pos = Vector3.zero;
        pos.x = PlayerPrefs.GetFloat("Handle" + index + "x");
        pos.z = PlayerPrefs.GetFloat("Handle" + index + "y");
        handles[index].position = pos;
    }
    void PlayerPrefSave(int index)
    {
        PlayerPrefs.SetFloat("Handle" + index + "x", handles[index].position.x);
        PlayerPrefs.SetFloat("Handle" + index + "y", handles[index].position.z);
    }

    void PlayerPrefLoadAll()
    {
        for (int i = 0; i < handles.Length; i++)
        {
            PlayerPrefLoad(i);
            SetShader();
        }
    }

    void PlayerPrefSaveAll()
    {
        for (int i = 0; i < handles.Length; i++)
            PlayerPrefSave(i);
    }

    void ResetPlayerPref()
    {
        handles[0].position = new Vector3(-3, 0, -3);
        handles[1].position = new Vector3(-3, 0, 3);
        handles[2].position = new Vector3(3, 0, 3);
        handles[3].position = new Vector3(3, 0, -3);
        PlayerPrefSaveAll();
        SetShader();
    }

    void SetShader()
    {
        float[] arr = new float[4];

        float x1 = handles[1].position.x;
        float y1 = handles[1].position.z;
        float x2 = handles[3].position.x;
        float y2 = handles[3].position.z;
        float x3 = handles[0].position.x;
        float y3 = handles[0].position.z;
        float x4 = handles[2].position.x;
        float y4 = handles[2].position.z;

        Vector2 center = DiagCenter(x1, y1, x2, y2, x3, y3, x4, y4);

        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = GetQ(i, center);
        }

        renderMat.SetFloatArray("_Q", arr);
    }

    Vector2 DiagCenter(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
    {
        Vector2 v = Vector2.zero;

        float Xnum = (x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4);
        float Xden = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

        float Ynum = (x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4);
        float Yden = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

        v.x = Xnum / Xden;
        v.y = Ynum / Yden;
        return v;
    }

    float GetQ(int index, Vector2 center)
    {
        int oppositeindex = (index + 2) % 4;
        Vector2 main = new Vector2(handles[index].position.x, handles[index].position.z);
        Vector2 opposite = new Vector2(handles[oppositeindex].position.x, handles[oppositeindex].position.z);
        float d = Vector3.Distance(main, center);
        float d2 = Vector3.Distance(opposite, center);
        return ((d + d2) / d2);
    }



}
