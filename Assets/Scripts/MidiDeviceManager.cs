using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using UnityEngine;
using Melanchall.DryWetMidi.Multimedia;

public class MidiDeviceManager : MonoBehaviour
{
    public string midiDeviceName;

    private GameObject _midiNotePrefab;
    private GameObject _midiNoteAccidentalPrefab;

    private Transform _midiNotesContainer;
    private Vector3 _midiNotesHolderStartPosition;

    public InputDevice CurrentInputDevice;
    public OutputDevice CurrentOutputDevice;

    public bool printInputDebug;
    public bool printOutputDebug;

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
        var hasMidiInputDevice = InputDevice.GetAll().Any(device => device.Name == midiDeviceName);
        if (hasMidiInputDevice)
        {
            CurrentInputDevice = InputDevice.GetByName(midiDeviceName);
            CurrentInputDevice.EventReceived += OnMidiReceived;
            CurrentInputDevice.StartEventsListening();
        }
        else
        {
            Debug.LogWarning("No midi input device");
        }

        var hasMidiOutputDevice = OutputDevice.GetAll().Any(device => device.Name == midiDeviceName);
        if (hasMidiOutputDevice)
        {
            CurrentOutputDevice = OutputDevice.GetByName(midiDeviceName);
            CurrentOutputDevice.EventSent += OnMidiSent;
        }
        else
        {
            Debug.LogWarning("No midi output device");
        }
    }

    private void OnMidiReceived(object sender, MidiEventReceivedEventArgs e)
    {
        var midiDevice = (MidiDevice)sender;
        if (printInputDebug && e.Event is not ActiveSensingEvent)
            print($"Event received from '{midiDevice.Name}': {e.Event}");
    }

    private void OnMidiSent(object sender, MidiEventSentEventArgs e)
    {
        var midiDevice = (MidiDevice)sender;
        if (printOutputDebug)
            print($"Midi sent to '{midiDevice.Name}': {e.Event}");
    }

    private void OnApplicationQuit()
    {
        if (CurrentInputDevice != null)
        {
            CurrentInputDevice.EventReceived -= OnMidiReceived;
            CurrentInputDevice.StopEventsListening();
            CurrentInputDevice.Dispose();
        }

        if (CurrentOutputDevice != null)
        {
            CurrentOutputDevice.EventSent -= OnMidiSent;
            CurrentOutputDevice.Dispose();
        }
    }
}