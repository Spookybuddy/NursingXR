using Microsoft.MixedReality.QR;

namespace GIGXR.Platform.HMD.AppEvents.Events
{
    /// <summary>
    /// An event that indicates that QR tracking has stopped, not that it will stop QR tracking. Use a reference to IQrCodeManager if you
    /// need to manage or use QR Tracking.
    /// </summary>
    public class UpdateQrObjectEvent : QrCodeEvent
    {
        public QRCode QrCodeToUpdate { get; }

        public bool Remove { get; }

        public UpdateQrObjectEvent(QRCode qrCode, bool remove = false)
        {
            QrCodeToUpdate = qrCode;
            Remove = remove;
        }
    }
}