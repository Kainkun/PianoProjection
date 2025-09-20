using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Multimedia;
using UnityEngine;


public class PianoData
{
    private int _lowestMidiNote = int.MaxValue;
    private int _highestMidiNote = int.MinValue;

    public int LowestMidiNote
    {
        get => _lowestMidiNote;
        set
        {
            _lowestMidiNote = value;
            RefreshData();
        }
    }

    public int HighestMidiNote
    {
        get => _highestMidiNote;
        set
        {
            _highestMidiNote = value;
            RefreshData();
        }
    }

    [ReadOnly] public MyNoteData lowestKey;
    [ReadOnly] public MyNoteData highestKey;
    public Dictionary<int, MyNoteData> AllKeys = new();
    public Dictionary<int, bool> midiIsSharp = new();
    [HideInInspector] public int whiteKeysCount;
    [HideInInspector] public int blackKeysCount;
    
    public Action<PianoData> onDataRefreshed;

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

    private void RefreshData()
    {
        lowestKey = null;
        highestKey = null;
        whiteKeysCount = 0;
        blackKeysCount = 0;

        for (var midiNote = LowestMidiNote; midiNote <= HighestMidiNote; midiNote++)
        {
            MyNoteData myNoteData = new MyNoteData(midiNote);

            AllKeys[midiNote] = myNoteData;

            if (midiNote == LowestMidiNote)
                lowestKey = myNoteData;
            else if (midiNote == HighestMidiNote)
                highestKey = myNoteData;

            midiIsSharp[midiNote] = myNoteData.isSharp;
            if (myNoteData.isSharp)
                blackKeysCount++;
            else
                whiteKeysCount++;
        }
        
        onDataRefreshed?.Invoke(this);
    }
}