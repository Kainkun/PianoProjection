using System;
using System.Collections.Generic;
using UnityEngine;
using HeliosDac;

[RequireComponent(typeof(AudioSource))]
public class WaveGenerator : MonoBehaviour
{
    // Public thread-safe property to access frequencies and velocities
    private readonly Dictionary<float, float> _activeFrequencyVelocity = new();

    public float gain = 1;

    public IReadOnlyDictionary<float, float> ActiveFrequencyVelocity
    {
        get
        {
            lock (_lock)
            {
                return new Dictionary<float, float>(_activeFrequencyVelocity);
            }
        }
    }

    // Internal phase tracking
    private readonly Dictionary<float, double> _frequencyPhases = new();
    private readonly object _lock = new();

    private AudioSource _audioSource;
    private int _sampleRate;
    private const double TWO_PI = Math.PI * 2.0;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _sampleRate = AudioSettings.outputSampleRate;
    }

    /// <summary>
    /// Thread-safe way to set all active frequencies and velocities every frame.
    /// </summary>
    public void SetActiveFrequencies(Dictionary<float, float> freqs)
    {
        if (freqs == null) return;

        lock (_lock)
        {
            _activeFrequencyVelocity.Clear();
            foreach (var kv in freqs)
                _activeFrequencyVelocity[kv.Key] = kv.Value;
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        Dictionary<float, float> notesCopy;

        // Copy current frequencies safely
        lock (_lock)
        {
            notesCopy = new Dictionary<float, float>(_activeFrequencyVelocity);
        }

        for (int i = 0; i < data.Length; i += channels)
        {
            double sample = 0.0;

            lock (_lock)
            {
                foreach (var kv in notesCopy)
                {
                    float freq = kv.Key;
                    float vel = kv.Value;
                    double phase = _frequencyPhases.GetValueOrDefault(freq, 0.0);

                    // Sine wave sample
                    sample += Math.Sin(phase) * vel;

                    // Advance phase
                    phase += TWO_PI * freq / _sampleRate;
                    if (phase >= TWO_PI) phase -= TWO_PI;

                    _frequencyPhases[freq] = phase;
                }
            }

            // Normalize output to prevent clipping
            float outSample = (float)(sample / Math.Max(1, notesCopy.Count)) * gain;

            // Write to all channels
            for (int c = 0; c < channels; c++)
                data[i + c] = outSample;
        }

        // Remove phases for frequencies no longer active
        lock (_lock)
        {
            var keysToRemove = new List<float>();
            foreach (var freq in _frequencyPhases.Keys)
            {
                if (!_activeFrequencyVelocity.ContainsKey(freq))
                    keysToRemove.Add(freq);
            }

            foreach (var key in keysToRemove)
                _frequencyPhases.Remove(key);
        }
    }
}