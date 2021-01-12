using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Interaction;

public class MidiSystem : MonoBehaviour
{
    public Transform notesHolder;
    Vector3 notesHolderStartpos;
    public GameObject noteObject;
    public GameObject noteAccedentalObject;
    MidiFile midiFile;
    OutputDevice outputDevice;
    Playback playback;

    void Start()
    {
        // print(OutputDevice.GetDevicesCount());
        // foreach (var item in OutputDevice.GetAll())
        // {
        //     print(item.Name);
        // }

        notesHolderStartpos = notesHolder.position;

        midiFile = MidiFile.Read("C:/Users/Kainkun/Music/Clair de Lune.mid");
        outputDevice = OutputDevice.GetByName("VirtualMIDISynth #1");
        playback = midiFile.GetPlayback(outputDevice, new MidiClockSettings
        {
            CreateTickGeneratorCallback = () => null
        });

        var notes = midiFile.GetNotes();
        var tempoMap = midiFile.GetTempoMap();
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

    void MakeKey(Note note, TempoMap tempoMap)
    {
        var n = (int)note.NoteName;
        bool accidental = (n == 1 || n == 3 || n == 6 || n == 8 || n == 10);

        GameObject tempNoteObject = Instantiate(accidental ? noteAccedentalObject : noteObject, notesHolder);
        var x = Piano.instance.keyPositions[note.NoteNumber].position.x;
        var y = note.TimeAs<MetricTimeSpan>(tempoMap).TotalMicroseconds / 100000.0f;
        var z = accidental ? -0.52f : -0.26f;
        tempNoteObject.transform.localPosition = new Vector3(x, y, z);
        x = Piano.instance.keyStep * (accidental ? Piano.instance.blackKeyWidthRatio : Piano.instance.whiteKeyWidthRatio);
        y = note.LengthAs<MetricTimeSpan>(tempoMap).TotalMicroseconds / 100000f;
        tempNoteObject.transform.localScale = new Vector3(x, y, 1);
    }

    IEnumerator Play()
    {
        yield return new WaitForSeconds(3f);


        playback.Start();
        while (playback.IsRunning)
        {
            yield return new WaitForSeconds(0.01f);
            playback.TickClock();
            notesHolder.transform.position = notesHolderStartpos + Vector3.down * (playback.GetCurrentTime<MetricTimeSpan>().TotalMicroseconds / 100000.0f);
            print(playback.GetCurrentTime<MetricTimeSpan>().TotalMicroseconds / 100000.0f);
            yield return null;
        }
    }

    private void OnApplicationQuit()
    {
        playback.Stop();
        playback.Dispose();
    }

    public void InitGameNote(float timeOfNote, int noteNumber, float duration)
    {
        transform.position = new Vector3(timeOfNote, -noteNumber);
        GetComponent<SpriteRenderer>().size = new Vector2(duration, 1f);
    }
}
