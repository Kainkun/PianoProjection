using System;
using System.Collections;
using TMPro;
using UnityEngine;
using SimpleFileBrowser;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Camera uiCamera;

    public TextMeshProUGUI UIDisplayText;
    public TextMeshProUGUI ProjectionDisplayText;
    public TextMeshProUGUI SelectedMidiFileText;

    public Action OnTogglePausePlayback;
    public Action OnRestartPlayback;
    public Action<string> OnSelectMidiFile;
    public Action<bool> OnToggleLoop;
    public Action<float> OnSetPlaybackSpeed;
    public Action<float> OnSetVolume;

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
        OnSetPlaybackSpeed?.Invoke(speed);
    }

    private void SetVolume(float volume)
    {
        OnSetVolume?.Invoke(volume);
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