using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WaveGenerator : MonoBehaviour
{
    public Dictionary<float, float> activeFrequencyVelocity = new();

    private Dictionary<float, double> _frequencyPhases = new();

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
        List<KeyValuePair<float, float>> notesCopy;

        lock (_lock)
        {
            // Make a copy to safely iterate
            notesCopy = new List<KeyValuePair<float, float>>(activeFrequencyVelocity);
        }

        for (int i = 0; i < data.Length; i += channels)
        {
            double sample = 0.0;

            foreach (var frequencyVelocityPhase in notesCopy)
            {
                var frequency = frequencyVelocityPhase.Key;
                var velocity = frequencyVelocityPhase.Value;
                double phase = _frequencyPhases.GetValueOrDefault(frequency, 0.0);

                // Add sine wave scaled by velocity
                sample += Math.Sin(phase) * Mathf.Clamp01(velocity);

                // Advance phase
                phase += TWO_PI * frequency / _sampleRate;
                if (phase >= TWO_PI)
                    phase -= TWO_PI;

                // Save phase back
                _frequencyPhases[frequency] = phase;
            }

            // Normalize by number of notes to avoid clipping
            // float outSample = (float)(sample / Math.Max(1, notesCopy.Count));
            float outSample = (float)sample;

            // Write to all channels
            for (int c = 0; c < channels; c++)
                data[i + c] = outSample;
        }

        // Optional: remove phases for notes that are no longer active
        lock (_lock)
        {
            var keysToRemove = new List<float>();
            foreach (var frequency in _frequencyPhases.Keys)
                if (!activeFrequencyVelocity.ContainsKey(frequency))
                    keysToRemove.Add(frequency);
            foreach (var key in keysToRemove)
                _frequencyPhases.Remove(key);
        }
    }
}