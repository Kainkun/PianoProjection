using System;
using System.Collections;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;

public class MidiFileManager : MonoBehaviour
{
    public Action<Note, TempoMap> OnMidiNoteLoaded;
    public Action<float> OnMidiPositionChanged;

    private Playback _currentPlayback;

    public void PlayMidiFile(string midiPath, OutputDevice outputDevice = null)
    {
        PreparePlayback(midiPath, outputDevice);
        StartCoroutine(StartPlayback());
    }

    private void PreparePlayback(string midiPath, OutputDevice outputDevice = null)
    {
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
    }

    IEnumerator StartPlayback()
    {
        yield return new WaitForSeconds(3);

        _currentPlayback.Start();
        while (_currentPlayback.IsRunning)
        {
            yield return new WaitForSeconds(0.01f);
            _currentPlayback.TickClock();
            OnMidiPositionChanged?.Invoke(
                _currentPlayback.GetCurrentTime<MetricTimeSpan>().TotalMicroseconds / 100000.0f
            );
        }
    }

    public void Dispose()
    {
        if (_currentPlayback == null) return;

        _currentPlayback.Stop();
        _currentPlayback.Dispose();
    }
}