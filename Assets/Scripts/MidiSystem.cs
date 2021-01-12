using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;

public class MidiSystem : MonoBehaviour
{
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

        midiFile = MidiFile.Read("Assets/Midi/Clair.mid");
        outputDevice = OutputDevice.GetByName("VirtualMIDISynth #1");
        playback = midiFile.GetPlayback(outputDevice, new MidiClockSettings());

        StartCoroutine(Play());
    }


    void Update()
    {

    }

    IEnumerator Play()
    {
        playback.Start();
        while (playback.IsRunning)
        {
            playback.TickClock();
            yield return null;
        }
    }

    private void OnApplicationQuit()
    {
        playback.Stop();
        playback.Dispose();
    }
}
