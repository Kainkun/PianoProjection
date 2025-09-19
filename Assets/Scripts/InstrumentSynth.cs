using System.Collections.Generic;
using UnityEngine;

public class InstrumentSynth
{
    public static Dictionary<float, float> RawSynth(Dictionary<int, MyNoteData> noteDatas)
    {
        var d = new Dictionary<float, float>();
        foreach (var noteData in noteDatas.Values)
        {
            d[noteData.frequency] = noteData.velocity;
        }

        return d;
    }

    /// <summary>
    /// Produces fundamental + overtone harmonics to sound more like a piano.
    /// </summary>
    public static Dictionary<float, float> PianoLikeSynth(Dictionary<int, MyNoteData> noteDatas)
    {
        var d = new Dictionary<float, float>();

        // How strong each overtone is (relative to velocity)
        float[] harmonicStrengths = { 1.0f, 0.5f, 0.3f, 0.2f, 0.1f, 0.05f };

        foreach (var noteData in noteDatas.Values)
        {
            for (int i = 0; i < harmonicStrengths.Length; i++)
            {
                float harmonic = (i + 1) * noteData.frequency; // 1x, 2x, 3x, ...
                float amp = noteData.velocity * harmonicStrengths[i];

                if (d.ContainsKey(harmonic))
                    d[harmonic] += amp;
                else
                    d[harmonic] = amp;
            }
        }

        return d;
    }
}