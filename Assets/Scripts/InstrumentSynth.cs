using System.Collections.Generic;
using UnityEngine;

public class InstrumentSynth
{
    public static Dictionary<float, float> PianoLikeSynthNote(
        MyNoteData noteData,
        int harmonicCount = 6,
        float falloff = 0.5f)
    {
        var allFrequencies = new Dictionary<float, float>();
        for (int i = 0; i < harmonicCount; i++)
        {
            float harmonicFreq = (i + 1) * noteData.frequency;
            float harmonicAmp = noteData.velocity * Mathf.Pow(falloff, i);

            allFrequencies[harmonicFreq] = harmonicAmp;
        }

        return allFrequencies;
    }

    public static Dictionary<float, float> PianoLikeSynth(
        Dictionary<int, MyNoteData> noteDatas,
        int harmonicCount = 6,
        float falloff = 0.5f)
    {
        var allFrequencies = new Dictionary<float, float>();
        foreach (var noteData in noteDatas.Values)
        {
            for (int i = 0; i < harmonicCount; i++)
            {
                float harmonicFreq = (i + 1) * noteData.frequency;
                float harmonicAmp = noteData.velocity * Mathf.Pow(falloff, i);

                if (allFrequencies.ContainsKey(harmonicFreq))
                    allFrequencies[harmonicFreq] += harmonicAmp;
                else
                    allFrequencies[harmonicFreq] = harmonicAmp;
            }
        }

        return allFrequencies;
    }
}