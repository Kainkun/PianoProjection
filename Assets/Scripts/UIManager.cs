using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using SimpleFileBrowser;

public class UIManager : MonoBehaviour
{
    public Camera uiCamera;

    public TextMeshProUGUI UIDisplayText;
    public TextMeshProUGUI ProjectionDisplayText;
    public TextMeshProUGUI SelectedMidiFileText;
    public TextMeshProUGUI PlaybackSpeedText;
    public TMP_Dropdown MidiDeviceDropdown;

    public Action OnTogglePausePlayback;
    public Action OnRestartPlayback;
    public Action<string> OnSelectMidiFile;
    public Action<bool> OnToggleLoop;
    public Action<float> OnSetPlaybackSpeed;
    public Action<bool> OnOutputMidiAudio;
    public Action<string> OnSelectMidiDevice;

    public Action<int> OnUIDisplayChanged;

    private void Start()
    {
        StartCoroutine(RebuildLayout());
    }

    IEnumerator RebuildLayout()
    {
        yield return new WaitForEndOfFrame();
        ChangeUIDisplay(1);
    }

    private void TogglePausePlayback()
    {
        OnTogglePausePlayback?.Invoke();
    }

    private void RestartPlayback()
    {
        OnRestartPlayback?.Invoke();
    }

    public void SetAvailableMidiDevices(List<string> midiDeviceNames)
    {
        MidiDeviceDropdown.ClearOptions();
        MidiDeviceDropdown.AddOptions(new List<string> { "None" });
        MidiDeviceDropdown.AddOptions(midiDeviceNames);
    }

    public void SelectMidiDevice(int dropdownIndex)
    {
        if (dropdownIndex == 0)
        {
            OnSelectMidiDevice?.Invoke(null);
            return;
        }

        var deviceName = MidiDeviceDropdown.options[dropdownIndex].text;
        Debug.Log($"Selected MIDI Device: {deviceName}");
        OnSelectMidiDevice?.Invoke(deviceName);
    }

    private void SelectMidiFile()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("MIDI Files", ".mid", ".midi"));
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);
        FileBrowser.ShowLoadDialog((paths) =>
            {
                Debug.Log("Selected: " + paths[0]);
                OnSelectMidiFile?.Invoke(paths[0]);
            },
            () => { Debug.Log("Canceled"); },
            FileBrowser.PickMode.Files, false, null, null, "Select MIDI File", "Select");
    }

    private void ToggleLoop(bool isLooping)
    {
        OnToggleLoop?.Invoke(isLooping);
    }

    private void SetPlaybackSpeed(float speed)
    {
        PlaybackSpeedText.text = $"Playback Speed: {speed:F2}x";
        OnSetPlaybackSpeed?.Invoke(speed);
    }

    private void ToggleOutputMidiAudio(bool isOutputtingMidiAudio)
    {
        OnOutputMidiAudio?.Invoke(isOutputtingMidiAudio);
    }

    public void OnProjectionDisplayChanged(int displayIndex)
    {
        ProjectionDisplayText.text = $"Current Projection Display: {displayIndex}";
    }

    public void OnMidiFileSelected(string path)
    {
        SelectedMidiFileText.text = $"Selected MIDI File: {path}";
    }


    public void ChangeUIDisplay(int i)
    {
        var index = i - 1;

#if !UNITY_EDITOR
        if (index < 0 || index >= Display.displays.Length)
        {
            Debug.LogWarning($"Display {i}/{Display.displays.Length} does not exist.");
            return;
        }

        Display.displays[index].Activate();
#endif
        uiCamera.targetDisplay = index;

        UIDisplayText.text = $"Current Projection Display: {index}";

        OnUIDisplayChanged?.Invoke(i);
    }
}