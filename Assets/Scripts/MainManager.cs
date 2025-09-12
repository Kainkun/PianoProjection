using System;
using Melanchall.DryWetMidi.Multimedia;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    public PianoData pianoData;
    public float mouseSensitivity = 30;
    public string midiPath;

    public PianoModel pianoModel;
    public ProjectionManager projectionManager;
    public MidiFileManager midiFileManager;

    private MyMidiDevice _myMidiDevice;
    private MidiVisualizer _midiVisualizer;


    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        _myMidiDevice = new MyMidiDevice(pianoData.deviceName);

        pianoModel.SetupPianoModel(pianoData);
        _midiVisualizer = new MidiVisualizer(pianoModel);

        midiFileManager.OnMidiNoteLoaded += _midiVisualizer.TryInstantiateMidiKey;
        midiFileManager.OnMidiPositionChanged += _midiVisualizer.UpdateMidiPosition;
        midiFileManager.PlayMidiFile(midiPath, _myMidiDevice.output);
    }

    private void OnApplicationQuit()
    {
        _myMidiDevice.Dispose();
        midiFileManager.Dispose();
        midiFileManager.OnMidiNoteLoaded -= _midiVisualizer.TryInstantiateMidiKey;
        midiFileManager.OnMidiPositionChanged -= _midiVisualizer.UpdateMidiPosition;
    }

    private void Update()
    {
        UpdateInput();
    }

    private void ResetPlayerPref()
    {
        projectionManager.ResetHandlePositions();
    }

    private void UpdateInput()
    {
        if (Input.GetKeyDown(KeyCode.S))
            projectionManager.ToggleShortcutScreen();

        if (!Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.X))
            projectionManager.FlipImageX();
        if (!Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y))
            projectionManager.FlipImageY();

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.X))
            projectionManager.FlipCursorX();
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y))
            projectionManager.FlipCursorY();

        if (Input.GetKeyDown(KeyCode.R))
            projectionManager.RecenterCursor();

        if (Input.GetKeyDown(KeyCode.Space))
            projectionManager.ToggleCornerEditing();

        if (Input.GetKey(KeyCode.Escape))
            Application.Quit();

        if (!Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Delete))
            SceneManager.LoadScene(0);
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Delete))
            ResetPlayerPref();

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            projectionManager.ChangeDisplay(1);
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            projectionManager.ChangeDisplay(2);
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            projectionManager.ChangeDisplay(3);
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            projectionManager.ChangeDisplay(4);
        if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
            projectionManager.ChangeDisplay(5);
        if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
            projectionManager.ChangeDisplay(6);
        if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
            projectionManager.ChangeDisplay(7);
        if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8))
            projectionManager.ChangeDisplay(8);

        projectionManager.MoveCursorPosition(new Vector2(
            Input.mousePositionDelta.x * mouseSensitivity,
            Input.mousePositionDelta.y * mouseSensitivity));

        if (Input.GetMouseButtonDown(0))
            projectionManager.TryGrabHandle();

        if (Input.GetMouseButtonUp(0))
            projectionManager.TryReleaseHandle();
    }
}