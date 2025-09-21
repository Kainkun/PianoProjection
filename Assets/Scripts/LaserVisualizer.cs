using System;
using System.Collections.Generic;
using UnityEngine;
using HeliosDac;

public class LaserVisualizer : MonoBehaviour
{
    public WaveGenerator waveGen;
    public HeliosDacStreamer dacStreamer;
    public int laserPoints = 500;

    public float amplitudeScale = 0.8f;
    public float horizontalScale = 1f;
    [Range(0f, 1f)] public float verticalOffset = 0.5f;


    void Update()
    {
        if (dacStreamer == null || waveGen == null) return;

        var frame = CreateLaserFrame();
        dacStreamer.SubmitFrame(frame);
    }

    private HeliosPoint[] CreateLaserFrame()
    {
        var frame = new HeliosPoint[laserPoints];

        // Thread-safe copy of active notes
        var activeNotes = waveGen.ActiveFrequencyVelocity;

        for (int i = 0; i < laserPoints; i++)
        {
            float xNorm = i / (float)(laserPoints - 1);
            double sample = 0.0;

            // Sum all frequencies as sine waves
            foreach (var kv in activeNotes)
            {
                float freq = kv.Key; // frequency in Hz
                float vel = kv.Value; // amplitude
                sample += Math.Sin(xNorm * 2.0 * Math.PI * freq * horizontalScale) * vel;
            }

            // Normalize to prevent clipping
            int nNotes = Math.Max(1, activeNotes.Count);
            sample /= nNotes;

            // Map to laser coordinates
            ushort laserX = (ushort)(xNorm * 0xFFF);
            ushort laserY = (ushort)((sample * 0.5 * amplitudeScale + verticalOffset) * 0xFFF);

            frame[i] = new HeliosPoint
            {
                X = laserX,
                Y = laserY,
                Red = 0xFF,
                Green = 0xFF,
                Blue = 0xFF,
                Intensity = 0xFF
            };
        }

        // int extraPoints = 5; // a few points to lift the laser
        // Array.Resize(ref frame, laserPoints + extraPoints);
        //
        // for (int i = laserPoints; i < laserPoints + extraPoints; i++)
        // {
        //     frame[i] = new HeliosPoint
        //     {
        //         X = frame[laserPoints - 1].X, // keep last X
        //         Y = frame[laserPoints - 1].Y, // keep last Y
        //         Red = 0,
        //         Green = 0,
        //         Blue = 0,
        //         Intensity = 0 // blank the laser
        //     };
        // }

        // Shift all points by 1 and add a blank start point
        var oldFrame = frame;
        frame = new HeliosPoint[oldFrame.Length + 1];
        frame[0] = new HeliosPoint { X = oldFrame[0].X, Y = oldFrame[0].Y, Intensity = 0 };
        Array.Copy(oldFrame, 0, frame, 1, oldFrame.Length);

        return frame;
    }
}