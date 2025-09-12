using System.Collections.Generic;
using UnityEngine;

public class Piano : MonoBehaviour
{
    public Camera cam;

    public int keyCount = 61;
    public int middleCPosition = 25;
    public Note leftmostNote = Note.C;

    public float blackKeyHeightRatio = 0.6f;
    public float whiteKeyWidthRatio = 0.95f;
    public float blackKeyWidthRatio = 0.5f;

    private GameObject _whiteKeyPrefab;
    private GameObject _blackKeyPrefab;

    private int _whiteKeyCount;
    private int _blackKeyCount;

    public readonly Dictionary<int, Key> KeyPositions = new();

    public float keyStep;

    public float firstKeyPos;

    private float _whiteKeyHeight;
    private float _blackKeyHeight;

    public class Key
    {
        public int position;
        public Transform transform;
        public Note note;
        public int octave;
        public bool isSharp;
    }

    public enum Note
    {
        C,
        D,
        E,
        F,
        G,
        A,
        B
    }

    private void Awake()
    {
        _whiteKeyPrefab = Resources.Load<GameObject>("Prefabs/Piano White Key");
        _blackKeyPrefab = Resources.Load<GameObject>("Prefabs/Piano Black Key");

        CreatePiano();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            blackKeyHeightRatio += 0.01f;
            DeletePiano();
            CreatePiano();
        }

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            blackKeyHeightRatio -= 0.01f;
            DeletePiano();
            CreatePiano();
        }
    }


    private void CreatePiano()
    {
        var camHeight = 2f * cam.orthographicSize;
        var camWidth = camHeight * cam.aspect;

        _blackKeyCount = BlackKeyCount(keyCount);
        _whiteKeyCount = keyCount - _blackKeyCount;

        //c B d B e f B g B a B b c

        keyStep = camWidth / (_whiteKeyCount);

        _whiteKeyHeight = camHeight;
        _blackKeyHeight = _whiteKeyHeight * blackKeyHeightRatio;
        firstKeyPos = (-camWidth / 2) + (keyStep / 2);


        var keyIndex = 60 - middleCPosition + 1;


        for (var i = 0; i < _whiteKeyCount; i++)
        {
            var whiteKey = new Key
            {
                transform = Instantiate(_whiteKeyPrefab, transform).transform
            };
            whiteKey.transform.localScale = new Vector3(keyStep * whiteKeyWidthRatio, _whiteKeyHeight, 0.5f);
            whiteKey.transform.localPosition = new Vector3(firstKeyPos + (keyStep * i), 0, 0);
            whiteKey.transform.name = $"White Key ({keyIndex})";

            KeyPositions.Add(keyIndex, whiteKey);
            keyIndex++;

            if (i < _whiteKeyCount - 1 && (i - 2 + (int)leftmostNote) % 7 != 0 && (i - 6 + (int)leftmostNote) % 7 != 0)
            {
                var blackKey = new Key
                {
                    transform = Instantiate(_blackKeyPrefab, transform).transform
                };
                blackKey.transform.localScale = new Vector3(keyStep * blackKeyWidthRatio, _blackKeyHeight, 0.5f);
                blackKey.transform.localPosition = new Vector3((keyStep / 2) + firstKeyPos + (keyStep * i),
                    _whiteKeyHeight / 2 - _blackKeyHeight / 2, -0.25f);
                blackKey.transform.name = $"Black Key ({keyIndex})";
                KeyPositions.Add(keyIndex, blackKey);
                keyIndex++;
            }
        }
    }

    private void DeletePiano()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
            GameObject.Destroy(transform.GetChild(i).gameObject);
    }

    private int BlackKeyCount(int totalKeys)
    {
        var white = 0;
        var black = 0;
        while (white + black < totalKeys)
        {
            white++;
            if ((white - 2 + (int)leftmostNote) % 7 != 0 && (white - 6 + (int)leftmostNote) % 7 != 0)
                black++;
        }

        black--;
        return black;
    }
}