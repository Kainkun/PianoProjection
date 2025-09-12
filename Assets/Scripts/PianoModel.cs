using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PianoModel : MonoBehaviour
{
    public Camera cam;

    public float blackKeyHeightRatio = 0.6f;
    public float whiteKeyWidthRatio = 0.95f;
    public float blackKeyWidthRatio = 0.5f;

    private GameObject _whiteKeyPrefab;
    private GameObject _blackKeyPrefab;

    [HideInInspector] public float keyStep;
    private float _firstKeyPos;

    private void Awake()
    {
        _whiteKeyPrefab = Resources.Load<GameObject>("Prefabs/Piano White Key");
        _blackKeyPrefab = Resources.Load<GameObject>("Prefabs/Piano Black Key");
    }

    public void CreatePianoModel(PianoData pianoData)
    {
        var camHeight = 2f * cam.orthographicSize;
        var camWidth = camHeight * cam.aspect;

        keyStep = camWidth / (pianoData.whiteKeys.Count);

        var whiteKeyHeight = camHeight;
        var blackKeyHeight = whiteKeyHeight * blackKeyHeightRatio;
        _firstKeyPos = (-camWidth / 2) + (keyStep / 2);

        var currentX = _firstKeyPos;
        foreach (var keyData in pianoData.allKeys)
        {
            if (!keyData.isSharp)
            {
                keyData.transform = Instantiate(_whiteKeyPrefab, transform).transform;
                keyData.transform.localScale = new Vector3(keyStep * whiteKeyWidthRatio, whiteKeyHeight, 0.5f);
                keyData.transform.localPosition = new Vector3(currentX, 0, 0);
                keyData.transform.name = $"({keyData.midiNote}) White Key";
                currentX += keyStep;
            }
            else
            {
                keyData.transform = Instantiate(_blackKeyPrefab, transform).transform;
                keyData.transform.localScale = new Vector3(keyStep * blackKeyWidthRatio, blackKeyHeight, 0.5f);
                keyData.transform.localPosition =
                    new Vector3(currentX - (keyStep * whiteKeyWidthRatio / 2),
                        whiteKeyHeight / 2 - blackKeyHeight / 2, -0.25f);
                keyData.transform.name = $"   ({keyData.midiNote}) Black Key";
            }
        }
    }

    private void DeletePiano()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }
}