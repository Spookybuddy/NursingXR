using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Mobile.UI
{
    public class ContentMarkerControlModeHelper : MonoBehaviour
    {
        private ICalibrationManager CalibrationManager { get; set; }

        [InjectDependencies]
        public void Construct(ICalibrationManager calibrationManager)
        {
            CalibrationManager = calibrationManager;

            CalibrationManager.ContentMarkerControlModeSet += CalibrationManager_ContentMarkerControlModeSet;
        }

        private void OnDestroy()
        {
            CalibrationManager.ContentMarkerControlModeSet -= CalibrationManager_ContentMarkerControlModeSet;
        }

        private void CalibrationManager_ContentMarkerControlModeSet(object sender, EventArgs e)
        {
            // If set to host control, do not allow the button to be available
            gameObject.SetActive(CalibrationManager.CurrentContentMarkerControlMode != ContentMarkerControlMode.Host);
        }
    }
}