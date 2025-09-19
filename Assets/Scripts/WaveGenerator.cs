using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WaveGenerator : MonoBehaviour
{
    [Tooltip("Dictionary of active notes. Key can be note ID.")]
    public Dictionary<int, MyNoteData> activeNotes = new();

    // Keep per-note phase for continuity
    private Dictionary<int, double> _notePhases = new();

    private AudioSource _audioSource;
    private int _sampleRate;
    private readonly object _lock = new();
    private const double TWO_PI = Math.PI * 2.0;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _sampleRate = AudioSettings.outputSampleRate;
    }


    void OnAudioFilterRead(float[] data, int channels)
    {
        List<KeyValuePair<int, MyNoteData>> notesCopy;

        lock (_lock)
        {
            // Make a copy to safely iterate
            notesCopy = new List<KeyValuePair<int, MyNoteData>>(activeNotes);
        }

        for (int i = 0; i < data.Length; i += channels)
        {
            double sample = 0.0;

            foreach (var kvp in notesCopy)
            {
                int key = kvp.Key;
                MyNoteData note = kvp.Value;

                // Get phase for this note, initialize if missing
                if (!_notePhases.TryGetValue(key, out double phase))
                    phase = 0.0;

                // Add sine wave scaled by velocity
                sample += Math.Sin(phase) * Mathf.Clamp01(note.velocity);

                // Advance phase
                phase += TWO_PI * note.frequency / _sampleRate;
                if (phase >= TWO_PI)
                    phase -= TWO_PI;

                // Save phase back
                _notePhases[key] = phase;
            }

            // Normalize by number of notes to avoid clipping
            float outSample = (float)(sample / Math.Max(1, notesCopy.Count));

            // Write to all channels
            for (int c = 0; c < channels; c++)
                data[i + c] = outSample;
        }

        // Optional: remove phases for notes that are no longer active
        lock (_lock)
        {
            var keysToRemove = new List<int>();
            foreach (var key in _notePhases.Keys)
                if (!activeNotes.ContainsKey(key))
                    keysToRemove.Add(key);
            foreach (var key in keysToRemove)
                _notePhases.Remove(key);
        }
    }
}