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

    public static Dictionary<float, float> PianoLikeSynth(
        Dictionary<int, MyNoteData> noteDatas,
        int harmonicCount = 6,
        float falloff = 0.5f)
    {
        var d = new Dictionary<float, float>();

        foreach (var noteData in noteDatas.Values)
        {
            for (int i = 0; i < harmonicCount; i++)
            {
                float harmonicFreq = (i + 1) * noteData.frequency;
                float harmonicAmp = noteData.velocity * Mathf.Pow(falloff, i);

                if (d.ContainsKey(harmonicFreq))
                    d[harmonicFreq] += harmonicAmp;
                else
                    d[harmonicFreq] = harmonicAmp;
            }
        }

        return d;
    }
}