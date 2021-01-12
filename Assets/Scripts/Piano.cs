using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piano : MonoBehaviour
{
    public static Piano instance;
    public Camera cam;
    public GameObject whiteKey;
    public GameObject blackKey;
    public int keyCount = 61;
    public int middleCposition = 25;
    public Note leftmostNote = Note.C;
    int whiteKeyCount;
    int blackKeyCount;
    //public float whiteKeyHeightRatio = 5;
    public float blackKeyHeightRatio = 0.6f;
    public float whiteKeyWidthRatio = 0.95f;
    public float blackKeyWidthRatio = 0.5f;

    public Dictionary<int, Transform> keyPositions = new Dictionary<int, Transform>();

    float camHeight;
    float camWidth;

    public enum Note { C, D, E, F, G, A, B }

    private void Awake()
    {
        instance = this;
        CreatePiano();
    }

    void Start()
    {
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




    public float keyStep;
    float whiteKeyHeight;
    float blackKeyHeight;
    public float firstKeyPos;


    void CreatePiano()
    {
        camHeight = 2f * cam.orthographicSize;
        camWidth = camHeight * cam.aspect;

        blackKeyCount = BlackKeyCount(keyCount);
        whiteKeyCount = keyCount - blackKeyCount;

        //c B d B e f B g B a B b c

        keyStep = camWidth / (whiteKeyCount);
        //float whiteKeyHeight = keyStep * whiteKeyHeightRatio;
        whiteKeyHeight = camHeight;
        blackKeyHeight = whiteKeyHeight * blackKeyHeightRatio;
        firstKeyPos = (-camWidth / 2) + (keyStep / 2);




        int keyIndex = 60 - middleCposition + 1;

        for (int i = 0; i < whiteKeyCount; i++)
        {
            Transform wk = Instantiate(whiteKey, transform).transform;
            wk.localScale = new Vector3(keyStep * whiteKeyWidthRatio, whiteKeyHeight, 0.5f);
            wk.localPosition = new Vector3(firstKeyPos + (keyStep * i), 0, 0);
            wk.name = "Wkey(" + keyIndex + ")";
            keyPositions.Add(keyIndex, wk);
            keyIndex++;

            if (i < whiteKeyCount - 1 && (i - 2 + (int)leftmostNote) % 7 != 0 && (i - 6 + (int)leftmostNote) % 7 != 0)
            {
                Transform bk = Instantiate(blackKey, transform).transform;
                bk.localScale = new Vector3(keyStep * blackKeyWidthRatio, blackKeyHeight, 0.5f);
                bk.localPosition = new Vector3((keyStep / 2) + firstKeyPos + (keyStep * i), whiteKeyHeight / 2 - blackKeyHeight / 2, -0.25f);
                bk.name = "Bkey(" + keyIndex + ")";
                keyPositions.Add(keyIndex, bk);
                keyIndex++;
            }

        }
    }

    void DeletePiano()
    {
        int childs = transform.childCount;
        for (int i = childs - 1; i >= 0; i--)
            GameObject.Destroy(transform.GetChild(i).gameObject);
    }

    int BlackKeyCount(int totalKeys)
    {
        int white = 0;
        int black = 0;
        while (white + black < totalKeys)
        {
            white++;
            if ((white - 2 + (int)leftmostNote) % 7 != 0 && (white - 6 + (int)leftmostNote) % 7 != 0)
                black++;
        }
        black--;

        // int c = 0;
        // for (int i = 0; i < whiteKeyCount; i++)
        //     if (i < whiteKeyCount - 1 && (i - 2 + (int)leftmostNote) % 7 != 0 && (i - 6 + (int)leftmostNote) % 7 != 0)
        //         c++;
        return black;
    }
}
