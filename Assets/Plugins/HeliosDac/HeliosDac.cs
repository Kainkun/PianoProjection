/*
SDK for Helios Laser DAC C# class
Updated for Unity 6 / C# 9
By: adapted for user (original author: Gitle Mikkelsen)
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using LibUsbDotNet;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;
using LibUsbDotNet.WinUsb;

namespace HeliosDac
{
    /// <summary>
    /// Contains functionality for controlling all Helios Laser DACs including searching for connected devices.
    /// </summary>
    public class HeliosController
    {
        // USB consts
        public const ushort HELIOS_VID = 0x1209;
        public const ushort HELIOS_PID = 0xE500;

        private readonly List<HeliosDevice> dacs = new List<HeliosDevice>();

        ~HeliosController()
        {
            CloseDevices();
        }

        /// <summary>
        /// Search for and open all Helios Laser DACs connected to the computer.
        /// </summary>
        /// <returns>The number of DACs found.</returns>
        public int OpenDevices()
        {
            CloseDevices();

            UsbDevice.ForceLibUsbWinBack = true;
            try
            {
                foreach (UsbRegistry usbRegistry in UsbDevice.AllDevices)
                {
                    if (usbRegistry.Pid == HELIOS_PID && usbRegistry.Vid == HELIOS_VID)
                    {
                        if (usbRegistry.Open(out UsbDevice dac))
                        {
                            var libUsbDevice = dac as IUsbDevice;
                            if (libUsbDevice != null)
                            {
                                // Claim (and set alt interface) if this is a libusb-backed device
                                libUsbDevice.ClaimInterface(0);
                                libUsbDevice.SetAltInterface(1);
                            }

                            dacs.Add(new HeliosDevice(dac));
                        }
                    }
                }
            }
            catch (DllNotFoundException ex)
            {
                throw new DllNotFoundException(
                    "Could not find the libusb-1.0 binaries. You need to manually include the file when distributing your program, depending on the platform.\n" +
                    "Libusb can be downloaded here: https://sourceforge.net/projects/libusb/files/libusb-1.0/libusb-1.0.26/\n" +
                    "For example, for Windows x64, add the file libusb-1.0.dll to your build folder (e.g. the VS2015-x64\\dll folder in the libusb archive).",
                    ex);
            }

            return dacs.Count;
        }

        /// <summary>
        /// Close the connection to all Helios DACs. This invalidates all device numbers and HeliosDevice references, to use a DAC again you need to call OpenDevices() once more.
        /// </summary>
        public void CloseDevices()
        {
            foreach (var dac in dacs)
                dac.Close();
            dacs.Clear();
            UsbDevice.Exit();
        }

        public void WriteFrame(int deviceNumber, ushort scanRate, HeliosPoint[] points, byte flags = 0)
        {
            dacs[deviceNumber].WriteFrame(scanRate, points, flags);
        }

        public bool GetStatus(int deviceNumber)
        {
            return dacs[deviceNumber].GetStatus();
        }

        public void StopOutput(int deviceNumber)
        {
            dacs[deviceNumber].StopOutput();
        }

        public void SetShutter(int deviceNumber, bool isShutterClosed)
        {
            dacs[deviceNumber].SetShutter(isShutterClosed);
        }

        public int GetFirmwareVersion(int deviceNumber)
        {
            return dacs[deviceNumber].GetFirmwareVersion();
        }

        public string GetName(int deviceNumber)
        {
            return dacs[deviceNumber].GetName();
        }

        public void SetName(int deviceNumber, string name)
        {
            dacs[deviceNumber].SetName(name);
        }

        public void Close(int deviceNumber)
        {
            dacs[deviceNumber].Close();
        }

        public HeliosDevice GetDevice(int deviceNumber)
        {
            return dacs[deviceNumber];
        }
    }

    /// <summary>
    /// Represents a single connected Helios Laser DAC.
    /// </summary>
    public class HeliosDevice
    {
        // Limits (use int so array sizing and indexing is straightforward)
        public const int MAX_POINTS = 0x1000;
        public const int MIN_RATE = 7;
        public const int HELIOS_SDK_VERSION = 6;

        private readonly UsbDevice usbDevice;
        private readonly UsbEndpointReader interruptEndpointReader;
        private readonly UsbEndpointWriter interruptEndpointWriter;
        private readonly UsbEndpointWriter bulkEndpointWriter;
        private readonly Mutex mutex = new Mutex();
        private readonly byte[] frameBuffer = new byte[(MAX_POINTS * 7) + 5];

        public HeliosDevice(UsbDevice usbDevice)
        {
            this.usbDevice = usbDevice ?? throw new ArgumentNullException(nameof(usbDevice));

            interruptEndpointReader = usbDevice.OpenEndpointReader(ReadEndpointID.Ep03, 32, EndpointType.Interrupt);
            interruptEndpointWriter = usbDevice.OpenEndpointWriter(WriteEndpointID.Ep06, EndpointType.Interrupt);
            bulkEndpointWriter = usbDevice.OpenEndpointWriter(WriteEndpointID.Ep02, EndpointType.Bulk);
        }

        ~HeliosDevice()
        {
            Close();
        }

        /// <summary>
        /// Outputs a frame to the DAC. This blocks the thread while the transfer happens.
        /// NB: Before a frame can be written, GetStatus() must be polled and return true.
        /// </summary>
        public void WriteFrame(ushort scanRate, HeliosPoint[] points, byte flags = 0)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (points.Length == 0 || points.Length > MAX_POINTS) throw new ArgumentOutOfRangeException(nameof(points), "Invalid number of points.");
            if (scanRate < MIN_RATE) throw new ArgumentOutOfRangeException(nameof(scanRate), "Invalid scanrate.");

            bool lockTaken = false;
            try
            {
                lockTaken = mutex.WaitOne(1000);
                if (!lockTaken) throw new TimeoutException("Could not acquire mutex.");

                // MCU bug workaround: avoid transfers with specific sizes
                int scanRateActual = scanRate;
                int numOfPointsActual = points.Length;
                if (((points.Length - 45) % 64) == 0)
                {
                    numOfPointsActual--;
                    scanRateActual = (int)Math.Round(scanRate * (double)numOfPointsActual / (double)points.Length);
                }

                // Prepare frame buffer
                int bufPos = 0;
                for (int i = 0; i < numOfPointsActual; i++)
                {
                    frameBuffer[bufPos++] = (byte)(points[i].X >> 4);
                    frameBuffer[bufPos++] = (byte)(((points[i].X & 0x0F) << 4) | (points[i].Y >> 8));
                    frameBuffer[bufPos++] = (byte)(points[i].Y & 0xFF);
                    frameBuffer[bufPos++] = points[i].Red;
                    frameBuffer[bufPos++] = points[i].Green;
                    frameBuffer[bufPos++] = points[i].Blue;
                    frameBuffer[bufPos++] = points[i].Intensity;
                }
                frameBuffer[bufPos++] = (byte)(scanRateActual & 0xFF);
                frameBuffer[bufPos++] = (byte)(scanRateActual >> 8);
                frameBuffer[bufPos++] = (byte)(numOfPointsActual & 0xFF);
                frameBuffer[bufPos++] = (byte)(numOfPointsActual >> 8);
                frameBuffer[bufPos++] = flags;

                // Send USB transaction
                var result = bulkEndpointWriter.Write(frameBuffer, 0, bufPos, 8 + (bufPos >> 5), out int transferLength);

                if (result != ErrorCode.Ok || transferLength != bufPos)
                {
                    throw new Exception("Could not send frame USB transaction. Error code: " + result.ToString() + ". Make sure you first poll GetStatus() before writing each frame.");
                }
            }
            finally
            {
                if (lockTaken) mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Checks whether the DAC is ready to receive a new frame.
        /// </summary>
        public bool GetStatus()
        {
            bool lockTaken = false;
            try
            {
                lockTaken = mutex.WaitOne(3000);
                if (!lockTaken) throw new TimeoutException("Could not acquire mutex.");

                var errorCode = interruptEndpointWriter.Write(new byte[] { 0x03, 0 }, 16, out int writeTransferLength);
                if (errorCode == ErrorCode.Ok && writeTransferLength == 2)
                {
                    byte[] readData = new byte[32];
                    errorCode = interruptEndpointReader.Read(readData, 16, out int readTransferLength);
                    if (errorCode == ErrorCode.Ok && readTransferLength > 1 && readData[0] == 0x83)
                    {
                        return readData[1] != 0;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    throw new Exception("Failed to send USB interrupt packet. Error code: " + errorCode.ToString());
                }
            }
            finally
            {
                if (lockTaken) mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Stops output from the DAC.
        /// </summary>
        public void StopOutput()
        {
            bool lockTaken = false;
            try
            {
                lockTaken = mutex.WaitOne(5000);
                if (!lockTaken) throw new TimeoutException("Could not acquire mutex.");

                var errorCode = interruptEndpointWriter.Write(new byte[] { 0x01, 0 }, 64, out int writeTransferLength);
                if (!(errorCode == ErrorCode.Ok && writeTransferLength == 2))
                    throw new Exception("Failed to send USB interrupt packet. Error code: " + errorCode.ToString());
            }
            finally
            {
                if (lockTaken) mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Sets the shutter pin of the DAC.
        /// </summary>
        public void SetShutter(bool shutterClosed)
        {
            bool lockTaken = false;
            try
            {
                lockTaken = mutex.WaitOne(5000);
                if (!lockTaken) throw new TimeoutException("Could not acquire mutex.");

                var errorCode = interruptEndpointWriter.Write(new byte[] { 0x02, shutterClosed ? (byte)0 : (byte)1 }, 64, out int writeTransferLength);
                if (!(errorCode == ErrorCode.Ok && writeTransferLength == 2))
                    throw new Exception("Failed to send USB interrupt packet. Error code: " + errorCode.ToString());
            }
            finally
            {
                if (lockTaken) mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Gets the firmware version of the DAC.
        /// </summary>
        public int GetFirmwareVersion()
        {
            bool lockTaken = false;
            try
            {
                lockTaken = mutex.WaitOne(3000);
                if (!lockTaken) throw new TimeoutException("Could not acquire mutex.");

                var errorCode = interruptEndpointWriter.Write(new byte[] { 0x04, 0 }, 64, out int writeTransferLength);
                if (errorCode == ErrorCode.Ok && writeTransferLength == 2)
                {
                    byte[] readData = new byte[32];
                    errorCode = interruptEndpointReader.Read(readData, 16, out int readTransferLength);
                    if (errorCode == ErrorCode.Ok && readTransferLength > 1 && readData[0] == 0x84)
                    {
                        return readData[1];
                    }
                    else
                        throw new Exception("Invalid response to firmware version request. Error code: " + errorCode.ToString());
                }
                else
                    throw new Exception("Failed to send USB interrupt packet. Error code: " + errorCode.ToString());
            }
            finally
            {
                if (lockTaken) mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Gets the name of the DAC.
        /// </summary>
        public string GetName()
        {
            bool lockTaken = false;
            try
            {
                lockTaken = mutex.WaitOne(3000);
                if (!lockTaken) throw new TimeoutException("Could not acquire mutex.");

                var errorCode = interruptEndpointWriter.Write(new byte[] { 0x05, 0 }, 16, out int writeTransferLength);
                if (errorCode == ErrorCode.Ok && writeTransferLength == 2)
                {
                    byte[] readData = new byte[32];
                    errorCode = interruptEndpointReader.Read(readData, 32, out int readTransferLength);
                    if (errorCode == ErrorCode.Ok && readTransferLength > 2 && readData[0] == 0x85)
                    {
                        // readTransferLength includes the first byte (0x85). Name begins at offset 1.
                        int nameLen = Math.Max(0, readTransferLength - 1);
                        // Trim any trailing null bytes
                        int actualLen = 0;
                        for (int i = 0; i < nameLen; i++)
                        {
                            if (readData[1 + i] == 0) break;
                            actualLen++;
                        }
                        return Encoding.ASCII.GetString(readData, 1, actualLen);
                    }
                    else
                        throw new Exception("Did not get valid response from DAC. Error code: " + errorCode.ToString());
                }
                else
                    throw new Exception("Failed to send USB interrupt packet. Error code: " + errorCode.ToString());
            }
            finally
            {
                if (lockTaken) mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Sets the name of the DAC.
        /// </summary>
        public void SetName(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            bool lockTaken = false;
            try
            {
                lockTaken = mutex.WaitOne(5000);
                if (!lockTaken) throw new TimeoutException("Could not acquire mutex.");

                var message = new byte[32];
                message[0] = 0x06;
                var nameBytes = Encoding.ASCII.GetBytes(name);
                for (int i = 0; i < nameBytes.Length && i < 31; i++)
                    message[1 + i] = nameBytes[i];

                message[31] = 0;

                var errorCode = interruptEndpointWriter.Write(message, 64, out int writeTransferLength);
                if (!(errorCode == ErrorCode.Ok && writeTransferLength == message.Length))
                    throw new Exception("Failed to send USB interrupt packet. Error code: " + errorCode.ToString());
            }
            finally
            {
                if (lockTaken) mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Closes the USB connection to the DAC. This object will no longer be usable and if the DAC is needed again you must search for it via HeliosController.OpenDevices() once more.
        /// </summary>
        public void Close()
        {
            try
            {
                usbDevice?.Close();
            }
            catch
            {
                // swallow close exceptions to avoid destructor throwing
            }
        }
    }

    /// <summary>
    /// Coordinates and color data for a point.
    /// </summary>
    public struct HeliosPoint
    {
        public HeliosPoint(ushort x, ushort y, byte red, byte green, byte blue, byte intensity = 0xFF)
        {
            this.X = x;
            this.Y = y;
            this.Red = red;
            this.Green = green;
            this.Blue = blue;
            this.Intensity = intensity;
        }
        public ushort X;       // From 0 to 0xFFF
        public ushort Y;       // From 0 to 0xFFF
        public byte Red;       // From 0 to 0xFF
        public byte Green;     // From 0 to 0xFF
        public byte Blue;      // From 0 to 0xFF
        public byte Intensity; // From 0 to 0xFF
    }

    [Flags]
    public enum HeliosFrameFlags : ushort
    {
        StartImmediately = 0b1,
        DoNotLoop = 0b10,
    }
}
