using System.Linq;
using Melanchall.DryWetMidi.Core;
using UnityEngine;
using Melanchall.DryWetMidi.Multimedia;

public class MyMidiDevice
{
    public readonly InputDevice input;
    public readonly OutputDevice output;

    public bool PrintInputDebug;
    public bool PrintOutputDebug;

    public MyMidiDevice(string deviceName)
    {
        var hasMidiInputDevice = InputDevice.GetAll().Any(device => device.Name == deviceName);
        if (hasMidiInputDevice)
        {
            input = InputDevice.GetByName(deviceName);
            input.EventReceived += (sender, e) =>
            {
                var midiDevice = (Melanchall.DryWetMidi.Multimedia.MidiDevice)sender;
                if (PrintInputDebug && e.Event is not ActiveSensingEvent)
                    Debug.Log($"Event received from '{deviceName}': {e.Event}");
            };
            input.StartEventsListening();
        }
        else
        {
            Debug.LogWarning("No midi input device");
        }

        var hasMidiOutputDevice = OutputDevice.GetAll().Any(device => device.Name == deviceName);
        if (hasMidiOutputDevice)
        {
            output = OutputDevice.GetByName(deviceName);
            output.EventSent += (sender, e) =>
            {
                var midiDevice = (Melanchall.DryWetMidi.Multimedia.MidiDevice)sender;
                if (PrintOutputDebug)
                    Debug.Log($"Midi sent to '{deviceName}': {e.Event}");
            };
        }
        else
        {
            Debug.LogWarning("No midi output device");
        }
    }

    public void Dispose()
    {
        if (input != null)
        {
            input.StopEventsListening();
            input.Dispose();
        }

        if (output != null)
        {
            output.Dispose();
        }
    }
}