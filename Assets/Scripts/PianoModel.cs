using System;
using System.Collections.Generic;
using UnityEngine;

public class PianoModel : MonoBehaviour
{
    public Camera cam;

    public float blackKeyHeightRatio = 0.6f;
    public float whiteKeyWidthRatio = 0.95f;
    public float blackKeyWidthRatio = 0.5f;

    public readonly Dictionary<int, GameObject> Keys = new();
    [HideInInspector] public float keyStep;

    private float _firstKeyPos;

    private Transform _keysContainer;

    public void SetupPianoModel(PianoData pianoData)
    {
        DeletePiano();
        Keys.Clear();
        _firstKeyPos = 0;


        var whiteKeyPrefab = Resources.Load<GameObject>("Prefabs/Piano White Key");
        var blackKeyPrefab = Resources.Load<GameObject>("Prefabs/Piano Black Key");

        _keysContainer = new GameObject("Keys Container").transform;
        _keysContainer.SetParent(transform, false);

        var camHeight = 2f * cam.orthographicSize;
        var camWidth = camHeight * cam.aspect;
        cam.transform.localPosition = new Vector3(0, camHeight / 2f, -10);

        keyStep = camWidth / (pianoData.whiteKeysCount);

        var whiteKeyHeight = camHeight;
        var blackKeyHeight = whiteKeyHeight * blackKeyHeightRatio;
        _firstKeyPos = (-camWidth / 2) + (keyStep / 2);

        var currentX = _firstKeyPos;
        foreach (var keyValuePair in pianoData.midiIsSharp)
        {
            var midiNote = keyValuePair.Key;
            var isSharp = keyValuePair.Value;

            GameObject go;
            if (!isSharp)
            {
                go = Instantiate(whiteKeyPrefab, _keysContainer);
                go.transform.localScale =
                    new Vector3(keyStep * whiteKeyWidthRatio, whiteKeyHeight, 0.5f);
                go.transform.localPosition = new Vector3(currentX, whiteKeyHeight / 2f, 0);
                go.name = $"({midiNote}) White Key";
                currentX += keyStep;
            }
            else
            {
                go = Instantiate(blackKeyPrefab, _keysContainer);
                go.transform.localScale =
                    new Vector3(keyStep * blackKeyWidthRatio, blackKeyHeight, 0.5f);
                go.transform.localPosition =
                    new Vector3(currentX - (keyStep * whiteKeyWidthRatio / 2),
                        (whiteKeyHeight / 2 - blackKeyHeight / 2) + whiteKeyHeight / 2f, -0.25f);
                go.name = $"   ({midiNote}) Black Key";
            }

            Keys[midiNote] = go;
        }
    }

    public void ColorKey(int midiNote, Color color)
    {
        var keyMaterial = Keys[midiNote].GetComponent<Renderer>().material;
        keyMaterial.color = color;
    }

    public void DeletePiano()
    {
        if (!_keysContainer) return;
        for (var i = _keysContainer.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }
}