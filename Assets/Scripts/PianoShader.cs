using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class PianoShader
{
    protected PianoModel _pianoModel;

    public PianoShader(PianoModel pianoModel)
    {
        _pianoModel = pianoModel;
    }

    public virtual void Draw()
    {
    }
}

public class PianoShaderVolume : PianoShader
{
    protected MyMidiDevice _midiDevice;

    public PianoShaderVolume(PianoModel pianoModel, MyMidiDevice midiDevice) : base(pianoModel)
    {
        _midiDevice = midiDevice;
    }

    public override void Draw()
    {
        foreach (var key in _pianoModel.AllKeys.Keys)
        {
            _pianoModel.ColorKey(key, Color.white);
        }

        foreach (MyNoteData noteData in _midiDevice.noteDatas.Values)
        {
            _pianoModel.ColorKey(noteData.midiNote, Color.Lerp(Color.white, Color.red, noteData.velocity));
        }
    }
}

public class PianoShaderDissonance : PianoShader
{
    protected MyMidiDevice _midiDevice;

    public PianoShaderDissonance(PianoModel pianoModel, MyMidiDevice midiDevice) : base(pianoModel)
    {
        _midiDevice = midiDevice;
    }

    public void Draw(HashSet<int> heldKeys, Dictionary<int, float> dissonanceDeltas, float maxDissonanceDelta)
    {
        foreach (int midiNote in _pianoModel.AllKeys.Keys)
        {
            if (heldKeys.Contains(midiNote))
            {
                _pianoModel.ColorKey(midiNote, Color.blue);
                continue;
            }

            if (maxDissonanceDelta > 0)
                _pianoModel.ColorKey(midiNote,
                    Color.Lerp(Color.green, Color.red, dissonanceDeltas[midiNote] / maxDissonanceDelta));
        }
    }
}