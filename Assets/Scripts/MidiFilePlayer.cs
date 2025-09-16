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

    private OutputDevice _currentOutputDevice;

    public void TogglePausePlayback()
    {
        if (_currentPlayback == null) return;

        if (_currentPlayback.IsRunning)
        {
            _currentPlayback.Stop();
        }
        else
        {
            _currentPlayback.Start();
        }
    }

    public void RestartPlayback()
    {
        if (_currentPlayback == null) return;
        _currentPlayback.MoveToStart();
        OnMidiPositionChanged?.Invoke(0);
    }

    public void SelectMidiFile(string midiPath, OutputDevice outputDevice = null)
    {
        OnMidiUnloaded?.Invoke();

        _currentOutputDevice = outputDevice;

        var currentMidiFile = MidiFile.Read(midiPath);

        var playbackSettings = new PlaybackSettings
        {
            ClockSettings = new MidiClockSettings { CreateTickGeneratorCallback = () => null }
        };

        _currentPlayback = currentMidiFile.GetPlayback(playbackSettings);
        _currentPlayback.OutputDevice = _currentOutputDevice;

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
        if (_currentPlayback == null) return;
        _currentPlayback.Loop = isLooping;
    }

    public void SetPlaybackSpeed(float speed)
    {
        if (_currentPlayback == null) return;
        _currentPlayback.Speed = speed;
    }

    public void ToggleMidiAudioOutput(bool isOutputtingMidiAudio)
    {
        if (_currentPlayback == null) return;
        _currentPlayback.OutputDevice = isOutputtingMidiAudio ? _currentOutputDevice : null;
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