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
        foreach (var key in _pianoModel.Keys.Keys)
        {
            _pianoModel.ColorKey(key, Color.white);
        }

        foreach (NoteData noteData in _midiDevice.noteDatas.Values)
        {
            _pianoModel.ColorKey(noteData.midiNote, Color.Lerp(Color.white, Color.red, noteData.velocity));
        }
    }
}

public class PianoShaderDissonance : PianoShader
{
    protected MyMidiDevice _midiDevice;

    private float lastFrequency = -1;

    public PianoShaderDissonance(PianoModel pianoModel, MyMidiDevice midiDevice) : base(pianoModel)
    {
        _midiDevice = midiDevice;
        midiDevice.OnMidiInputDown += pressedNoteData =>
        {
            lastFrequency = pressedNoteData.frequency;

            foreach (NoteData noteData in _midiDevice.noteDatas.Values)
            {
                _pianoModel.ColorKey(noteData.midiNote,
                    Color.Lerp(Color.green, Color.red, Dissonance(noteData.frequency, lastFrequency)));
            }

            _pianoModel.ColorKey(pressedNoteData.midiNote, Color.magenta);
        };
    }

    float Dissonance(float frequencyA, float frequencyB)
    {
        var s = 0.24f / (0.021f * Mathf.Min(frequencyA, frequencyB) + 19);
        var x = Mathf.Abs(frequencyB - frequencyA);
        return Mathf.Exp(-3.5f * s * x) - Mathf.Exp(-5.75f * s * x);
    }
}