using System.Collections;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using UnityEngine.Serialization;

public class MidiPlayer : MonoBehaviour
{
    public MidiDeviceManager midiDeviceManager;

    public string midiPath;

    private GameObject _midiNotePrefab;
    private GameObject _midiNoteAccidentalPrefab;

    private Transform _midiNotesContainer;
    private Vector3 _midiNotesHolderStartPosition;

    private MidiFile _currentMidiFile;
    private Playback _currentPlayback;

    private void Awake()
    {
        _midiNotePrefab = Resources.Load<GameObject>("Prefabs/Midi Note");
        _midiNoteAccidentalPrefab = Resources.Load<GameObject>("Prefabs/Midi Note Accidental");

        _midiNotesHolderStartPosition = new Vector3(0, 15, 10);
        _midiNotesContainer = new GameObject("Midi Notes Container").transform;
        _midiNotesContainer.position = _midiNotesHolderStartPosition;
    }

    public void StartPlayback(PianoModel pianoModel, PianoData pianoData)
    {
        var playbackSettings = new PlaybackSettings
        {
            ClockSettings = new MidiClockSettings { CreateTickGeneratorCallback = () => null }
        };

        _currentMidiFile = MidiFile.Read(midiPath);

        if (midiDeviceManager.CurrentOutputDevice != null)
        {
            _currentPlayback = _currentMidiFile.GetPlayback(midiDeviceManager.CurrentOutputDevice, playbackSettings);
            Debug.Log("Playing on output device");
        }
        else
        {
            _currentPlayback = _currentMidiFile.GetPlayback(playbackSettings);
            Debug.Log("Playing without output device");
        }


        var notes = _currentMidiFile.GetNotes();
        var tempoMap = _currentMidiFile.GetTempoMap();
        foreach (var note in notes)
        {
            if (!pianoData.MidiToKeyData.ContainsKey(note.NoteNumber))
            {
                continue;
            }

            MakeKey(note, tempoMap);
        }

        StartCoroutine(Play());
        return;

        void MakeKey(Note note, TempoMap tempoMap)
        {
            var n = (int)note.NoteName;
            var accidental = n is 1 or 3 or 6 or 8 or 10;

            var tempNoteObject =
                Instantiate(accidental ? _midiNoteAccidentalPrefab : _midiNotePrefab, _midiNotesContainer);
            var x = pianoData.MidiToKeyData[note.NoteNumber].transform.position.x;
            var y = note.TimeAs<MetricTimeSpan>(tempoMap).TotalMicroseconds / 100000.0f;
            var z = accidental ? -0.52f : -0.26f;
            tempNoteObject.transform.localPosition = new Vector3(x, y, z);
            x = pianoModel.keyStep *
                (accidental ? pianoModel.blackKeyWidthRatio : pianoModel.whiteKeyWidthRatio);
            y = note.LengthAs<MetricTimeSpan>(tempoMap).TotalMicroseconds / 100000f;
            tempNoteObject.transform.localScale = new Vector3(x, y, 1);
        }

        IEnumerator Play()
        {
            yield return new WaitForSeconds(3f);


            _currentPlayback.Start();
            while (_currentPlayback.IsRunning)
            {
                yield return new WaitForSeconds(0.01f);
                _currentPlayback.TickClock();
                _midiNotesContainer.transform.position = _midiNotesHolderStartPosition +
                                                         Vector3.down *
                                                         (_currentPlayback.GetCurrentTime<MetricTimeSpan>()
                                                              .TotalMicroseconds /
                                                          100000.0f);
                // print(_currentPlayback.GetCurrentTime<MetricTimeSpan>().TotalMicroseconds / 100000.0f);
                yield return null;
            }
        }
    }


    private void OnApplicationQuit()
    {
        if (_currentPlayback != null)
        {
            _currentPlayback.Stop();
            _currentPlayback.Dispose();
        }
    }
}