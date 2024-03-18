namespace GIGXR.Platform
{
    using System;

    /// <summary>
    /// Holds the data related to when a a target QR code is hit as well as a specific Action to 
    /// be fired when this occurs. If any QR code is needed, then the target can be left blank.
    /// </summary>
    public class QrTarget
    {
        public QrTarget(string targerQr, Action<string> qrAction)
        {
            TargetQrCode = targerQr;
            QrScanFinishedAction = qrAction;
        }

        public string TargetQrCode { get; }

        public Action<string> QrScanFinishedAction { get; }
    }
}