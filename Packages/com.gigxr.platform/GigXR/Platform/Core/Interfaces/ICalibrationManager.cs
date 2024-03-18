namespace GIGXR.Platform.Interfaces
{
    using System;
    using UnityEngine;

    public enum ContentMarkerControlMode
    {
        None,
        Self,
        Host
    }

    /// <summary>
    /// Interface to define the process of calibrating the app so that the user can place content in the location of their choice.
    /// </summary>
    public interface ICalibrationManager : IBaseManager
    {
        enum CalibrationModes
        {
            None,
            Qr,
            Manual
        }

        event EventHandler<EventArgs> ContentMarkerControlModeSet;

        CalibrationModes CurrentCalibrationMode { get; }

        CalibrationModes LastUsedCalibrationMode { get; }

        ContentMarkerControlMode CurrentContentMarkerControlMode { get; }

        void StartCalibration(CalibrationModes calibrationMode);

        void StopCalibration(bool calibrationWasCancelled, Vector3 targetPosition, Quaternion targetOrientation);

        void SetContentMarkerMode(ContentMarkerControlMode contentMarkerControlMode);
    }
}