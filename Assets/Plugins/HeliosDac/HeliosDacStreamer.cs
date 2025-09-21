using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using HeliosDac;

public class HeliosDacStreamer : MonoBehaviour
{
    private HeliosController _heliosController;
    private int _numberOfDevices;

    // Threading
    private CancellationTokenSource _cancel;
    private Task _workerTask;
    private bool _isDestroyed = false;

    // Frame buffer
    private readonly ConcurrentQueue<HeliosPoint[]> _frameQueue = new();
    private HeliosPoint[] _lastFrame;

    [Header("DAC Settings")] public int pointsPerSecond = 30000;
    public int targetFps = 60;

    private void Start()
    {
        InitLaser();

        _cancel = new CancellationTokenSource();
        _workerTask = Task.Run(() => LaserLoop(_cancel.Token), _cancel.Token);
    }

    private void InitLaser()
    {
        _heliosController = new HeliosController();
        try
        {
            _numberOfDevices = _heliosController.OpenDevices();
            Debug.Log($"Found {_numberOfDevices} Helios DACs");

            for (var deviceId = 0; deviceId < _numberOfDevices; deviceId++)
            {
                _heliosController.SetShutter(deviceId, false);
                Debug.Log($"Device {deviceId}: {_heliosController.GetName(deviceId)}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("DAC init failed: " + ex.Message);
        }
    }

    /// <summary>
    /// Submit a frame from Unity (Update/LateUpdate)
    /// </summary>
    public void SubmitFrame(HeliosPoint[] frame)
    {
        if (frame == null || _isDestroyed) return;

        _frameQueue.Enqueue(frame);
        while (_frameQueue.Count > 1 && _frameQueue.TryDequeue(out _))
        {
        }
    }

    private void LaserLoop(CancellationToken token)
    {
        var frameTimeMs = Mathf.RoundToInt(1000f / targetFps);

        try
        {
            while (!token.IsCancellationRequested)
            {
                // Pull latest frame from queue
                while (_frameQueue.TryDequeue(out var newFrame))
                    _lastFrame = newFrame;

                if (_lastFrame != null && _heliosController != null)
                {
                    for (var deviceId = 0; deviceId < _numberOfDevices; deviceId++)
                    {
                        if (token.IsCancellationRequested) break;

                        try
                        {
                            if (_heliosController.GetStatus(deviceId))
                                _heliosController.WriteFrame(deviceId, (ushort)pointsPerSecond, _lastFrame);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("Laser write error: " + ex.Message);
                        }
                    }
                }

                Thread.Sleep(frameTimeMs);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Laser thread terminated: " + ex.Message);
        }
    }

    private void OnDestroy()
    {
        _isDestroyed = true;

        // Stop the laser thread
        if (_cancel != null)
        {
            _cancel.Cancel();
            try
            {
                // Wait until the thread finishes
                _workerTask?.Wait(2000);
            }
            catch (AggregateException) { }
            catch (ObjectDisposedException) { }

            _cancel.Dispose();
            _cancel = null;
        }

        // Clear frame queue safely
        _lastFrame = null;
        while (_frameQueue.TryDequeue(out _)) { }

        // Close DAC only after thread stopped
        if (_heliosController != null)
        {
            try
            {
                _heliosController.CloseDevices();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Error closing DAC: " + ex.Message);
            }

            _heliosController = null;
        }
    }

}