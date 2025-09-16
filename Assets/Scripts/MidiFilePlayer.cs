using System;
using System.Collections;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;

public class MidiFilePlayer : MonoBehaviour
{
    public Action<Note, TempoMap> OnMidiNoteLoaded;
    public Action OnMidiUnloaded;
    public Action<float> OnMidiPositionChanged;

    private Playback _currentPlayback;

    public Action<string> OnMidiFileLoaded;

    public void TogglePausePlayback()
    {
        if (_currentPlayback != null)
        {
            if (_currentPlayback.IsRunning)
            {
                _currentPlayback.Stop();
            }
            else
            {
                _currentPlayback.Start();
            }
        }

        StartCoroutine(Playback());
    }

    public void RestartPlayback()
    {
        StartCoroutine(Playback());
    }

    public void SelectMidiFile(string midiPath, OutputDevice outputDevice)
    {
        OnMidiUnloaded?.Invoke();
        
        var currentMidiFile = MidiFile.Read(midiPath);

        var playbackSettings = new PlaybackSettings
        {
            ClockSettings = new MidiClockSettings { CreateTickGeneratorCallback = () => null }
        };

        if (outputDevice != null)
        {
            _currentPlayback = currentMidiFile.GetPlayback(outputDevice, playbackSettings);
            Debug.Log("Playing on output device");
        }
        else
        {
            _currentPlayback = currentMidiFile.GetPlayback(playbackSettings);
            Debug.Log("Playing without output device");
        }

        var notes = currentMidiFile.GetNotes();
        var tempoMap = currentMidiFile.GetTempoMap();
        foreach (var note in notes)
        {
            OnMidiNoteLoaded?.Invoke(note, tempoMap);
        }
        
        OnMidiFileLoaded?.Invoke(midiPath);
    }

    public void ToggleLoop(bool isLooping)
    {
    }

    public void SetPlaybackSpeed(float speed)
    {
    }

    public void SetVolume(float volume)
    {
    }

    private void Start()
    {
        StartCoroutine(Playback());
    }

    private IEnumerator Playback()
    {
        while (true)
        {
            while (_currentPlayback is { IsRunning: true })
            {
                yield return new WaitForSeconds(0.01f);
                _currentPlayback.TickClock();
                OnMidiPositionChanged?.Invoke(
                    _currentPlayback.GetCurrentTime<MetricTimeSpan>().TotalMicroseconds / 100000.0f
                );
            }

            yield return null;
        }
    }

    public void Dispose()
    {
        if (_currentPlayback == null) return;

        _currentPlayback.Stop();
        _currentPlayback.Dispose();
    }
}