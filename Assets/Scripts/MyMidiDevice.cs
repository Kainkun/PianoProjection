using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using UnityEngine;
using Melanchall.DryWetMidi.Multimedia;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class MyMidiDevice : MonoBehaviour
{
    [ReadOnly] public string deviceName;
    public float maxSustainTime = 4f;
    public float noSustainSpeed = 8f;

    public InputDevice Input;
    public OutputDevice Output;

    public bool PrintInputDebug = false;
    public bool PrintOutputDebug = false;

    private Queue<MidiEvent> _midiInputQueue = new();
    private Queue<MidiEvent> _midiOutputQueue = new();

    public Action<NoteData> OnMidiInputDown;
    public Action<NoteData> OnMidiInputUp;
    public Action OnMidiInputPedalDown;
    public Action OnMidiInputPedalUp;

    public bool IsSustainOn { get; private set; }
    public readonly HashSet<int> HeldNotes = new();
    public readonly Dictionary<int, NoteData> noteDatas = new();

    public void Init(string deviceName)
    {
        this.deviceName = deviceName;

        var hasMidiInputDevice = InputDevice.GetAll().Any(device => device.Name == deviceName);
        if (hasMidiInputDevice)
        {
            Input = InputDevice.GetByName(deviceName);
            Input.EventReceived += (sender, e) =>
            {
                _midiInputQueue.Enqueue(e.Event);
                if (PrintInputDebug && e.Event is not ActiveSensingEvent)
                    Debug.Log($"Event received from '{deviceName}': {e.Event}");
            };
            Input.StartEventsListening();
        }
        else
        {
            Debug.LogWarning("No midi input device");
        }

        var hasMidiOutputDevice = OutputDevice.GetAll().Any(device => device.Name == deviceName);
        if (hasMidiOutputDevice)
        {
            Output = OutputDevice.GetByName(deviceName);
            Output.EventSent += (sender, e) =>
            {
                _midiOutputQueue.Enqueue(e.Event);
                if (PrintOutputDebug)
                    Debug.Log($"Midi sent to '{deviceName}': {e.Event}");
            };
        }
        else
        {
            Debug.LogWarning("No midi output device");
        }
    }

    public void Update()
    {
        while (_midiInputQueue.Count > 0)
        {
            var inputEvent = _midiInputQueue.Dequeue();
            switch (inputEvent)
            {
                case NoteOnEvent noteOnEvent:
                    var noteNumber = noteOnEvent.NoteNumber;
                    var velocity = noteOnEvent.Velocity;
                    if (!noteDatas.ContainsKey(noteNumber))
                        noteDatas[noteNumber] = new NoteData(noteNumber, velocity);
                    var noteData = noteDatas[noteNumber];

                    if (velocity > 0)
                    {
                        HeldNotes.Add(noteOnEvent.NoteNumber);
                        noteData.velocity = Mathf.Max(noteData.velocity, noteOnEvent.Velocity / 127f);
                        OnMidiInputDown?.Invoke(noteData);
                    }
                    else
                    {
                        HeldNotes.Remove(noteOnEvent.NoteNumber);
                        OnMidiInputUp?.Invoke(new NoteData(noteOnEvent.NoteNumber));
                    }

                    break;
                case ActiveSensingEvent activeSensingEvent:
                    break;
                case ControlChangeEvent controlChangeEvent:
                    if (controlChangeEvent.ControlValue < 64)
                    {
                        IsSustainOn = false;
                        OnMidiInputPedalUp?.Invoke();
                    }
                    else
                    {
                        IsSustainOn = true;
                        OnMidiInputPedalDown?.Invoke();
                    }

                    break;
                default:
                    Debug.LogWarning($"Unhandled midi input event: {inputEvent}");
                    throw new ArgumentOutOfRangeException();
            }
        }

        foreach (var key in noteDatas.Keys.ToList())
        {
            var noteData = noteDatas[key];

            if (HeldNotes.Contains(key) || IsSustainOn)
            {
                noteData.velocity -= Time.deltaTime / maxSustainTime;
                noteData.velocity = Mathf.Max(noteData.velocity, 0);
            }
            else
            {
                noteData.velocity -= Time.deltaTime * noSustainSpeed;
                noteData.velocity = Mathf.Max(noteData.velocity, 0);
            }
        }

        while (_midiOutputQueue.Count > 0)
        {
            var outputEvent = _midiOutputQueue.Dequeue();
            switch (outputEvent)
            {
                case NoteOnEvent noteOnEvent:
                    break;
                case NoteOffEvent noteOffEvent:
                    break;
                case ControlChangeEvent controlChangeEvent:
                    break;
                case ProgramChangeEvent programChangeEvent:
                    break;
                case PitchBendEvent pitchBendEvent:
                    break;
                default:
                    Debug.LogWarning($"Unhandled midi output event: {outputEvent}");
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public void Dispose()
    {
        if (Input != null)
        {
            Input.StopEventsListening();
            Input.Dispose();
        }

        if (Output != null)
        {
            Output.Dispose();
        }
    }
}