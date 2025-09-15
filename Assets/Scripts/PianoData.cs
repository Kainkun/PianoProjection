using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.MusicTheory;
using UnityEngine;


[CreateAssetMenu(fileName = "Piano Data", menuName = "Piano Projection/Piano Data")]
public class PianoData : ScriptableObject
{
    public string deviceName;
    public int lowestMidiNote = 0;
    public int highestMidiNote = 127;

    [ReadOnly] public NoteData lowestKey;
    [ReadOnly] public NoteData highestKey;
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

    private void Awake()
    {
        RefreshData();
    }

    private void OnValidate()
    {
        RefreshData();
    }

    private void RefreshData()
    {
        lowestKey = null;
        highestKey = null;
        whiteKeysCount = 0;
        blackKeysCount = 0;

        for (var midiNote = lowestMidiNote; midiNote <= highestMidiNote; midiNote++)
        {
            NoteData noteData = new NoteData(midiNote);

            if (midiNote == lowestMidiNote)
                lowestKey = noteData;
            else if (midiNote == highestMidiNote)
                highestKey = noteData;

            midiIsSharp[midiNote] = noteData.isSharp;
            if (noteData.isSharp)
                blackKeysCount++;
            else
                whiteKeysCount++;
        }
    }
}