/// <summary>
/// write by 52cwalk,if you have some question ,please contract lycwalk@gmail.com
/// Heavily refactored by Nic
/// </summary>

using UnityEngine;
using System;
using ZXing;
using Microsoft.MixedReality.Toolkit.Utilities;
using GIGXR.Platform.Utilities;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

public class QRCodeDecodeControllerForWSA : MonoBehaviour
{
    public event EventHandler<string> onQRScanFinished; //declare a event with the delegate to trigger the complete event

    public DeviceCameraController e_DeviceController;

    private BarcodeReader barReader;

    public bool IsRunning { get { return runningTokenSource != null; } }

    private CancellationTokenSource runningTokenSource;

    private void Awake()
    {
        barReader = new BarcodeReader();

        barReader.Options.PossibleFormats = new List<BarcodeFormat>() { BarcodeFormat.QR_CODE };
        barReader.Options.TryHarder = false;
        barReader.AutoRotate = false;

        if (e_DeviceController == null)
        {
            e_DeviceController = FindObjectOfType<DeviceCameraController>();

            if (e_DeviceController == null)
            {
                DebugUtilities.LogError("The Device Controller does not exist, Please Drag DeviceCamera from project to Hierarchy");
            }
        }
    }

    private async UniTask UpdateReplacement(CancellationTokenSource tokenSource)
    {
        while (!tokenSource.IsCancellationRequested)
        {
            await BuildTargetArray();
        }
    }

    private int ImageWidth { get { return e_DeviceController.cameraTexture.width; } }

    private int ImageHeight { get { return e_DeviceController.cameraTexture.height; } }

    private async UniTask BuildTargetArray()
    {
        if (e_DeviceController == null ||
            !e_DeviceController.IsPlaying)
        {
            DebugUtilities.LogError($"[QRCodeDecodeControllerForWSA] Could not BuildTargetArray: e_DeviceController:{e_DeviceController} !isPlaying:{!e_DeviceController.IsPlaying}");
            return;
        }

        if (ImageWidth < 100 ||
            ImageHeight < 100)
        {
            return;
        }

        // Use the texture attached to the device controller to get the webcam image
        await ProcessScanCode(e_DeviceController.cameraTexture.GetPixels32().ToByteArray());
    }

    private async UniTask ProcessScanCode(byte[] targetByte)
    {
        // TODO Figure out BitmapFormat, the versions I thought did not work
        LuminanceSource luminanceSource = new RGBLuminanceSource(targetByte,
                                                                 ImageWidth,
                                                                 ImageHeight,
                                                                 RGBLuminanceSource.BitmapFormat.Unknown);

        // Decode takes the longest amount of time when it comes to the frame hit for QR code reading
        await DecodeBarcode(luminanceSource);
    }

    private async UniTask DecodeBarcode(LuminanceSource luminanceSource)
    {
        await UniTask.SwitchToThreadPool();

        var results = barReader.Decode(luminanceSource);

        await UniTask.SwitchToMainThread();

        if (results != null) // if get the result success
        {
            onQRScanFinished?.Invoke(this, results.Text);
        }
    }

    /// <summary>
    /// Starts the work.
    /// </summary>
    public void StartWork()
    {
        if (e_DeviceController != null)
        {
            e_DeviceController.StartWork();

            runningTokenSource = new CancellationTokenSource();

            _ = UpdateReplacement(runningTokenSource);
        }
    }

    public void StopWork()
    {
        if (e_DeviceController != null)
        {
            e_DeviceController.StopWork();
        }

        if(runningTokenSource != null)
        {
            runningTokenSource.Cancel();
            runningTokenSource.Dispose();
            runningTokenSource = null;
        }
    }
}