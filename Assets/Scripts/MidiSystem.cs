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

        midiFile = MidiFile.Read("C:/Users/Kainkun/Music/Clair de Lune.mid");
        outputDevice = OutputDevice.GetByName("VirtualMIDISynth #1");
        playback = midiFile.GetPlayback(outputDevice, new MidiClockSettings
        {
            CreateTickGeneratorCallback = () => null
        });

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
            yield return new WaitForSeconds(0.01f);
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
