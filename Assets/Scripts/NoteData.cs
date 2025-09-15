using System;
using UnityEngine;

public class NoteData
{
    public int midiNote;
    public Note note;
    [HideInInspector] public string noteString;
    [HideInInspector] public NoteLetter noteLetter;
    [HideInInspector] public string noteLetterString;
    [HideInInspector] public bool isSharp;
    public int octave;
    public float frequency;
    public float velocity;

    public NoteData(int midiNote, int velocity = 0)
    {
        this.midiNote = midiNote;
        this.velocity = velocity / 127f;

        var noteIndex = (midiNote % 12);

        note = (Note)noteIndex;
        octave = (midiNote / 12) - 1;
        frequency = 440 * Mathf.Pow(2, (midiNote - 69) / 12f);
        isSharp = noteIndex is 1 or 3 or 6 or 8 or 10;

        noteString = noteIndex switch
        {
            0 => "C",
            1 => "C#",
            2 => "D",
            3 => "D#",
            4 => "E",
            5 => "F",
            6 => "F#",
            7 => "G",
            8 => "G#",
            9 => "A",
            10 => "A#",
            11 => "B",
            _ => throw new ArgumentOutOfRangeException()
        };

        noteLetter = noteIndex switch
        {
            0 => NoteLetter.C,
            1 => NoteLetter.C,
            2 => NoteLetter.D,
            3 => NoteLetter.D,
            4 => NoteLetter.E,
            5 => NoteLetter.F,
            6 => NoteLetter.F,
            7 => NoteLetter.G,
            8 => NoteLetter.G,
            9 => NoteLetter.A,
            10 => NoteLetter.A,
            11 => NoteLetter.B,
            _ => throw new ArgumentOutOfRangeException()
        };

        noteLetterString = noteLetter.ToString();
    }

    public enum Note
    {
        C,
        CSharp,
        D,
        DSharp,
        E,
        F,
        FSharp,
        G,
        GSharp,
        A,
        ASharp,
        B
    }

    public enum NoteLetter
    {
        C,
        D,
        E,
        F,
        G,
        A,
        B
    }

    public static int NoteToMidi(Note note, int octave)
    {
        var noteIndex = (int)note;
        var midiNote = (octave + 1) * 12 + noteIndex;
        return midiNote;
    }
}