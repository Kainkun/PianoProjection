using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;

public class MidiSystem : MonoBehaviour
{
    public string midiPath;
    public string midiDeviceName;

#if UNITY_EDITOR
    [EasyButtons.Button]
    public void GetMidiDevices()
    {
        recentMidiDevices.Clear();
        foreach (var item in OutputDevice.GetAll())
            recentMidiDevices.Add(item.Name);
    }

    public List<string> recentMidiDevices = new();
#endif

    private GameObject _midiNotePrefab;
    private GameObject _midiNoteAccidentalPrefab;

    private Transform _midiNotesContainer;
    private Vector3 _midiNotesHolderStartPosition;

    private MidiFile _currentMidiFile;
    private OutputDevice _currentOutputDevice;
    private Playback _currentPlayback;

    private void Awake()
    {
        _midiNotePrefab = Resources.Load<GameObject>("Prefabs/Midi Note");
        _midiNoteAccidentalPrefab = Resources.Load<GameObject>("Prefabs/Midi Note Accidental");

        _midiNotesHolderStartPosition = new Vector3(0, 15, 10);
        _midiNotesContainer = new GameObject("Midi Notes Container").transform;
        _midiNotesContainer.position = _midiNotesHolderStartPosition;

        _currentMidiFile = MidiFile.Read(midiPath);

        if (midiDeviceName == "")
        {
            Debug.LogWarning("No midi device");
            return;
        }

        var outputDevice = OutputDevice.GetByName(midiDeviceName);

        using (outputDevice)
        {
            outputDevice.EventSent += OnEventSent;

            using (var inputDevice = InputDevice.GetByName(midiDeviceName))
            {
                inputDevice.EventReceived += OnEventReceived;
                inputDevice.StartEventsListening();

                outputDevice.SendEvent(new NoteOnEvent());
                outputDevice.SendEvent(new NoteOffEvent());
            }
        }


        var playbackSettings = new PlaybackSettings
        {
            ClockSettings = new MidiClockSettings { CreateTickGeneratorCallback = () => null }
        };
        _currentPlayback = _currentMidiFile.GetPlayback(outputDevice, playbackSettings);
    }

    void Start()
    {
        var notes = _currentMidiFile.GetNotes();
        var tempoMap = _currentMidiFile.GetTempoMap();
        foreach (var note in notes)
        {
            if (!Piano.instance.keyPositions.ContainsKey(note.NoteNumber))
            {
                continue;
            }

            MakeKey(note, tempoMap);
        }

        StartCoroutine(Play());
    }

    private void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
    {
        var midiDevice = (MidiDevice)sender;
        Console.WriteLine($"Event received from '{midiDevice.Name}' at {DateTime.Now}: {e.Event}");
    }

    private void OnEventSent(object sender, MidiEventSentEventArgs e)
    {
        var midiDevice = (MidiDevice)sender;
        Console.WriteLine($"Event sent to '{midiDevice.Name}' at {DateTime.Now}: {e.Event}");
    }

    void MakeKey(Note note, TempoMap tempoMap)
    {
        var n = (int)note.NoteName;
        bool accidental = (n == 1 || n == 3 || n == 6 || n == 8 || n == 10);

        GameObject tempNoteObject =
            Instantiate(accidental ? _midiNoteAccidentalPrefab : _midiNotePrefab, _midiNotesContainer);
        var x = Piano.instance.keyPositions[note.NoteNumber].position.x;
        var y = note.TimeAs<MetricTimeSpan>(tempoMap).TotalMicroseconds / 100000.0f;
        var z = accidental ? -0.52f : -0.26f;
        tempNoteObject.transform.localPosition = new Vector3(x, y, z);
        x = Piano.instance.keyStep *
            (accidental ? Piano.instance.blackKeyWidthRatio : Piano.instance.whiteKeyWidthRatio);
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

    private void OnApplicationQuit()
    {
        _currentPlayback.Stop();
        _currentPlayback.Dispose();
    }

    public void InitGameNote(float timeOfNote, int noteNumber, float duration)
    {
        transform.position = new Vector3(timeOfNote, -noteNumber);
        GetComponent<SpriteRenderer>().size = new Vector2(duration, 1f);
    }
}