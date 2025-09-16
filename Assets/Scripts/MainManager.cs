using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Multimedia;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MainManager : MonoBehaviour
{
    public float mouseSensitivity = 30;
    public string midiPath;

    public PianoModel pianoModel;
    public ProjectionManager projectionManager;
    public MidiFilePlayer midiFilePlayer;
    public UIManager uiManager;

    private MyMidiDevice _myMidiDevice;
    private MidiVisualizer _midiVisualizer;

    private PianoShader _pianoShader;

    private void Awake()
    {
        Debug.developerConsoleEnabled = true;
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    private void Start()
    {
        _midiVisualizer = new MidiVisualizer(pianoModel);

        // _pianoShader = new PianoShaderVolume(pianoModel, _myMidiDevice);

        midiFilePlayer.OnMidiNoteLoaded += _midiVisualizer.TryInstantiateMidiKey;
        midiFilePlayer.OnMidiPositionChanged += _midiVisualizer.UpdateMidiPosition;
        midiFilePlayer.OnMidiUnloaded += _midiVisualizer.ClearMidiNotes;
        midiFilePlayer.OnMidiFileLoaded += path => uiManager.OnMidiFileSelected(Path.GetFileNameWithoutExtension(path));

        uiManager.OnTogglePausePlayback += midiFilePlayer.TogglePausePlayback;
        uiManager.OnRestartPlayback += midiFilePlayer.RestartPlayback;
        uiManager.OnSelectMidiFile += selectedMidiPath =>
            midiFilePlayer.SelectMidiFile(selectedMidiPath, _myMidiDevice?.Output);
        uiManager.OnToggleLoop += midiFilePlayer.ToggleLoop;
        uiManager.OnSetPlaybackSpeed += midiFilePlayer.SetPlaybackSpeed;
        uiManager.OnOutputMidiAudio += midiFilePlayer.ToggleMidiAudioOutput;
        uiManager.SetAvailableMidiDevices(OutputDevice.GetAll().Select(item => item.Name).ToList());
        uiManager.OnSelectMidiDevice += selectedMidiDeviceName => { SetupPianoAndMidi(selectedMidiDeviceName); };


        projectionManager.OnProjectionDisplayChanged += uiManager.OnProjectionDisplayChanged;
    }

    private void OnApplicationQuit()
    {
        _myMidiDevice.Dispose();
        midiFilePlayer.Dispose();
        midiFilePlayer.OnMidiNoteLoaded -= _midiVisualizer.TryInstantiateMidiKey;
        midiFilePlayer.OnMidiPositionChanged -= _midiVisualizer.UpdateMidiPosition;
    }

    private void SetupPianoAndMidi(string midiDeviceName, int lowestMidiNote = 36, int highestMidiNote = 96)
    {
        if (_myMidiDevice)
        {
            _myMidiDevice.Dispose();
            Destroy(_myMidiDevice);
        }

        if (midiDeviceName == null)
        {
            if (_myMidiDevice)
                Destroy(_myMidiDevice);
            pianoModel.DeletePiano();
            return;
        }

        var pianoData = new PianoData(lowestMidiNote, highestMidiNote);
        pianoModel.SetupPianoModel(pianoData);

        _myMidiDevice = gameObject.AddComponent<MyMidiDevice>();
        _myMidiDevice.Init(midiDeviceName);
    }

    private void Update()
    {
        UpdateInput();

        _pianoShader?.Draw();
    }

    private void ResetPlayerPref()
    {
        projectionManager.ResetHandlePositions();
    }

    private void UpdateInput()
    {
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

        if (Input.GetKeyDown(KeyCode.Alpha1))
            projectionManager.ChangeProjectionDisplay(1);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            projectionManager.ChangeProjectionDisplay(2);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            projectionManager.ChangeProjectionDisplay(3);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            projectionManager.ChangeProjectionDisplay(4);
        if (Input.GetKeyDown(KeyCode.Alpha5))
            projectionManager.ChangeProjectionDisplay(5);
        if (Input.GetKeyDown(KeyCode.Alpha6))
            projectionManager.ChangeProjectionDisplay(6);
        if (Input.GetKeyDown(KeyCode.Alpha7))
            projectionManager.ChangeProjectionDisplay(7);
        if (Input.GetKeyDown(KeyCode.Alpha8))
            projectionManager.ChangeProjectionDisplay(8);

        if (Input.GetKeyDown(KeyCode.F1))
            uiManager.ChangeUIDisplay(1);
        if (Input.GetKeyDown(KeyCode.F2))
            uiManager.ChangeUIDisplay(2);
        if (Input.GetKeyDown(KeyCode.F3))
            uiManager.ChangeUIDisplay(3);
        if (Input.GetKeyDown(KeyCode.F4))
            uiManager.ChangeUIDisplay(4);
        if (Input.GetKeyDown(KeyCode.F5))
            uiManager.ChangeUIDisplay(5);
        if (Input.GetKeyDown(KeyCode.F6))
            uiManager.ChangeUIDisplay(6);
        if (Input.GetKeyDown(KeyCode.F7))
            uiManager.ChangeUIDisplay(7);
        if (Input.GetKeyDown(KeyCode.F8))
            uiManager.ChangeUIDisplay(8);

        projectionManager.MoveCursorPosition(new Vector2(
            Input.mousePositionDelta.x * mouseSensitivity,
            Input.mousePositionDelta.y * mouseSensitivity));

        if (Input.GetMouseButtonDown(0))
            projectionManager.TryGrabHandle();

        if (Input.GetMouseButtonUp(0))
            projectionManager.TryReleaseHandle();
    }
}