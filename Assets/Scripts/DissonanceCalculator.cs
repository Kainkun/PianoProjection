using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Computes a rough dissonance score for a set of frequencies and amplitudes.
/// Works for musical notes in Hz.
/// </summary>
public static class DissonanceCalculator
{
    /// <summary>
    /// Computes the total dissonance of a set of partials.
    /// </summary>
    /// <param name="frequencies">Array of frequencies in Hz</param>
    /// <param name="amplitudes">Array of corresponding amplitudes (0..1)</param>
    /// <returns>A scalar dissonance score (higher = more dissonant)</returns>
    public static float ComputeDissonance(float[] frequencies, float[] amplitudes)
    {
        if (frequencies.Length != amplitudes.Length)
            throw new ArgumentException("Frequencies and amplitudes must have the same length.");

        float totalDissonance = 0f;
        int n = frequencies.Length;

        for (int i = 0; i < n; i++)
        {
            float f1 = frequencies[i];
            float a1 = amplitudes[i];

            for (int j = i + 1; j < n; j++)
            {
                float f2 = frequencies[j];
                float a2 = amplitudes[j];

                // Scale frequency difference to match human perception
                float deltaF = Mathf.Abs(f1 - f2);

                // Peak dissonance occurs around 25–50 Hz difference, adjust scale
                float x = deltaF / 0.2f / Mathf.Min(f1, f2); // tuned empirically
                // Or simpler practical scaling:
                // float x = deltaF / 40f;

                // Roughness contribution
                float D = a1 * a2 * (Mathf.Exp(-3f * deltaF / 50f) - Mathf.Exp(-5f * deltaF / 50f));

                // Clamp negative values to zero
                D = Mathf.Max(D, 0f);

                totalDissonance += D;
            }
        }

        return totalDissonance;
    }

    /// <summary>
    /// Convenience method for Dictionary<float, float> input (frequency -> amplitude)
    /// </summary>
    public static float ComputeDissonance(Dictionary<float, float> tones)
    {
        float[] freqs = new float[tones.Count];
        float[] amps = new float[tones.Count];
        int i = 0;
        foreach (var kvp in tones)
        {
            freqs[i] = kvp.Key;
            amps[i] = kvp.Value;
            i++;
        }

        return ComputeDissonance(freqs, amps);
    }

    public static float ComputeCombinedDissonance(Dictionary<float, float> currentTones, Dictionary<float, float> addedTones)
    {
        // Combine current and added tones
        var combinedTones = new Dictionary<float, float>(currentTones);
        foreach (var kvp in addedTones)
        {
            // Assume velocity 1. Can't use current velocity because its predicting future velocity.
            if (combinedTones.ContainsKey(kvp.Key))
                combinedTones[kvp.Key] += 1;
            else
                combinedTones[kvp.Key] = 1;
        }

        return ComputeDissonance(combinedTones);
    }
}