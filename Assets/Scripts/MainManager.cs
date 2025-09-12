using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    public Projection projection;
    public PianoModel pianoModel;
    public PianoData pianoData;
    public MidiPlayer midiPlayer;
    public MidiDeviceManager midiDeviceManager;

    public int lowestMidiNote = 36;
    public int highestMidiNote = 96;

    public float mouseSensitivity = 30;


    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        pianoData = new PianoData(lowestMidiNote, highestMidiNote);
    }

    private void Start()
    {
        pianoModel.CreatePianoModel(pianoData);

        midiPlayer.StartPlayback(pianoModel, pianoData);
    }

    private void Update()
    {
        UpdateInput();
    }

    private void ResetPlayerPref()
    {
        projection.ResetHandlePositions();
    }

    private void UpdateInput()
    {
        if (Input.GetKeyDown(KeyCode.S))
            projection.ToggleShortcutScreen();

        if (!Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.X))
            projection.FlipImageX();
        if (!Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y))
            projection.FlipImageY();

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.X))
            projection.FlipCursorX();
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y))
            projection.FlipCursorY();

        if (Input.GetKeyDown(KeyCode.R))
            projection.RecenterCursor();

        if (Input.GetKeyDown(KeyCode.Space))
            projection.ToggleCornerEditing();

        if (Input.GetKey(KeyCode.Escape))
            Application.Quit();

        if (!Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Delete))
            SceneManager.LoadScene(0);
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Delete))
            ResetPlayerPref();

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            projection.ChangeDisplay(1);
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            projection.ChangeDisplay(2);
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            projection.ChangeDisplay(3);
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            projection.ChangeDisplay(4);
        if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
            projection.ChangeDisplay(5);
        if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
            projection.ChangeDisplay(6);
        if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
            projection.ChangeDisplay(7);
        if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8))
            projection.ChangeDisplay(8);

        projection.MoveCursorPosition(new Vector2(
            Input.mousePositionDelta.x * mouseSensitivity,
            Input.mousePositionDelta.y * mouseSensitivity));

        if (Input.GetMouseButtonDown(0))
            projection.TryGrabHandle();

        if (Input.GetMouseButtonUp(0))
            projection.TryReleaseHandle();
    }
}