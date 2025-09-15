using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

    private PianoData _pianoData;

    public void SetupPianoModel(PianoData pianoData)
    {
        _pianoData = pianoData;

        var whiteKeyPrefab = Resources.Load<GameObject>("Prefabs/Piano White Key");
        var blackKeyPrefab = Resources.Load<GameObject>("Prefabs/Piano Black Key");

        var keysContainer = new GameObject("Keys Container").transform;
        keysContainer.SetParent(transform, false);

        var camHeight = 2f * cam.orthographicSize;
        var camWidth = camHeight * cam.aspect;

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
                go = Instantiate(whiteKeyPrefab, keysContainer);
                go.transform.localScale =
                    new Vector3(keyStep * whiteKeyWidthRatio, whiteKeyHeight, 0.5f);
                go.transform.localPosition = new Vector3(currentX, 0, 0);
                go.name = $"({midiNote}) White Key";
                currentX += keyStep;
            }
            else
            {
                go = Instantiate(blackKeyPrefab, keysContainer);
                go.transform.localScale =
                    new Vector3(keyStep * blackKeyWidthRatio, blackKeyHeight, 0.5f);
                go.transform.localPosition =
                    new Vector3(currentX - (keyStep * whiteKeyWidthRatio / 2),
                        whiteKeyHeight / 2 - blackKeyHeight / 2, -0.25f);
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

    private void DeletePiano()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }
}