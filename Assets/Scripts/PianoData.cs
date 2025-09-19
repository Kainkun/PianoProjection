using System.Collections.Generic;
using Melanchall.DryWetMidi.Multimedia;
using UnityEngine;


public class PianoData
{
    public int lowestMidiNote = 0;
    public int highestMidiNote = 127;

    [ReadOnly] public MyNoteData lowestKey;
    [ReadOnly] public MyNoteData highestKey;
    public Dictionary<int, bool> midiIsSharp = new();
    [HideInInspector] public int whiteKeysCount;
    [HideInInspector] public int blackKeysCount;

#if UNITY_EDITOR
    public List<string> recentMidiOutputDevices = new();
    [EasyButtons.Button]
    public void GetMidiOutputDevices()
    {
        recentMidiOutputDevices.Clear();
        foreach (var item in OutputDevice.GetAll())
            recentMidiOutputDevices.Add(item.Name);
    }
#endif

    public PianoData(int lowestMidiNote, int highestMidiNote)
    {
        this.lowestMidiNote = lowestMidiNote;
        this.highestMidiNote = highestMidiNote;
        RefreshData();
    }

    public void RefreshData()
    {
        lowestKey = null;
        highestKey = null;
        whiteKeysCount = 0;
        blackKeysCount = 0;

        for (var midiNote = lowestMidiNote; midiNote <= highestMidiNote; midiNote++)
        {
            MyNoteData myNoteData = new MyNoteData(midiNote);

            if (midiNote == lowestMidiNote)
                lowestKey = myNoteData;
            else if (midiNote == highestMidiNote)
                highestKey = myNoteData;

            midiIsSharp[midiNote] = myNoteData.isSharp;
            if (myNoteData.isSharp)
                blackKeysCount++;
            else
                whiteKeysCount++;
        }
    }
}