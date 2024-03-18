using Microsoft.MixedReality.QR;
using System;
using UnityEngine;

namespace GIGXR.Platform.HMD.Interfaces
{
    public interface IQrCodeManager
    {
        public delegate void EventHandlerBool(bool successState);
        public delegate void QrCodeDecodedEventHandler(string decoded);
        public delegate void QrCodeCalibrationScannedEventHandler(Transform transform);

        public event EventHandler<bool> QrCodesTrackingStateChanged;
        public event EventHandler<string> QrCodeSeen;
        public event EventHandler<QRCode> QrCodeAdded;
        public event EventHandler<QRCode> QrCodeUpdated;
        public event EventHandler<QRCode> QrCodeRemoved;

        void StartQrTracking(string promptMessage);

        void StopQrTracking();

        void CancelQrTracking();
    }
}