using UnityEngine;
using HeliosDac;

public class LaserTest : MonoBehaviour
{
    public HeliosDacStreamer dacStreamer;

    void Update()
    {
        var frame = new HeliosPoint[100];
        for (int i = 0; i < frame.Length; i++)
        {
            frame[i] = new HeliosPoint
            {
                X = (ushort)(i * 0xFFF / frame.Length),
                Y = (ushort)(Random.Range(0, 0xFFF)),
                Red = 0xFF,
                Green = 0x00,
                Blue = 0x00,
                Intensity = 0xFF
            };
        }

        dacStreamer.SubmitFrame(frame);
    }
}
