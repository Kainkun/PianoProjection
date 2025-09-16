using System;
using Melanchall.DryWetMidi.Interaction;
using UnityEngine;
using Object = UnityEngine.Object;

public class MidiVisualizer
{
    private readonly PianoModel _pianoModel;

    private readonly GameObject _midiNotePrefab;
    private readonly GameObject _midiNoteAccidentalPrefab;

    private readonly Transform _midiNotesContainer;


    public MidiVisualizer(PianoModel pianoModel)
    {
        _pianoModel = pianoModel;

        _midiNotePrefab = Resources.Load<GameObject>("Prefabs/Midi Note");
        _midiNoteAccidentalPrefab = Resources.Load<GameObject>("Prefabs/Midi Note Accidental");

        _midiNotesContainer = new GameObject("Midi Notes Container").transform;
        _midiNotesContainer.SetParent(pianoModel.transform, false);
    }

    public void ClearMidiNotes()
    {
        foreach (Transform child in _midiNotesContainer)
        {
            Object.Destroy(child.gameObject);
        }
    }

    public void TryInstantiateMidiKey(Note note, TempoMap tempoMap)
    {
        if (!_pianoModel.Keys.ContainsKey(note.NoteNumber)) return;
        _midiNotesContainer.transform.localPosition = Vector3.zero;

        var n = (int)note.NoteName;
        var accidental = n is 1 or 3 or 6 or 8 or 10;

        var tempNoteObject =
            Object.Instantiate(accidental ? _midiNoteAccidentalPrefab : _midiNotePrefab, _midiNotesContainer);
        var x = _pianoModel.Keys[note.NoteNumber].transform.localPosition.x;
        var y = note.TimeAs<MetricTimeSpan>(tempoMap).TotalMicroseconds / 100000.0f;
        var z = accidental ? -0.52f : -0.26f;
        tempNoteObject.transform.localPosition = new Vector3(x, y, z);
        x = _pianoModel.keyStep *
            (accidental ? _pianoModel.blackKeyWidthRatio : _pianoModel.whiteKeyWidthRatio);
        y = note.LengthAs<MetricTimeSpan>(tempoMap).TotalMicroseconds / 100000f;
        tempNoteObject.transform.localScale = new Vector3(x, y, 1);
    }

    public void UpdateMidiPosition(float midiPosition)
    {
        _midiNotesContainer.transform.localPosition = Vector3.down * midiPosition;
    }
}