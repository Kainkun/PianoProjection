using System;
using System.Collections.Generic;
using UnityEngine;


public class PianoData
{
    public KeyData lowestKey;
    public KeyData highestKey;
    public Dictionary<int, KeyData> MidiToKeyData = new();
    public List<KeyData> allKeys = new();
    public List<KeyData> whiteKeys = new();
    public List<KeyData> blackKeys = new();

    public class KeyData
    {
        public Transform transform;
        public int midiNote;
        public Note note;
        public string noteString;
        public NoteLetter noteLetter;
        public string noteLetterString;
        public bool isSharp;
        public int octave;
        public float frequency;
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

    public PianoData(int lowestMidiNote, int highestMidiNote)
    {
        for (var midiNote = lowestMidiNote; midiNote <= highestMidiNote; midiNote++)
        {
            var noteIndex = (midiNote % 12);

            var keyData = new KeyData
            {
                midiNote = midiNote,
                note = (Note)noteIndex,
                octave = (midiNote / 12) - 1,
                frequency = 440 * Mathf.Pow(2, (midiNote - 69) / 12f),
                isSharp = noteIndex is 1 or 3 or 6 or 8 or 10
            };
            MidiToKeyData[midiNote] = keyData;

            keyData.noteString = noteIndex switch
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

            keyData.noteLetter = noteIndex switch
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

            keyData.noteLetterString = keyData.noteLetter.ToString();

            if (midiNote == lowestMidiNote)
                lowestKey = keyData;
            else if (midiNote == highestMidiNote)
                highestKey = keyData;

            allKeys.Add(keyData);
            if (keyData.isSharp)
                blackKeys.Add(keyData);
            else
                whiteKeys.Add(keyData);
        }
    }
}