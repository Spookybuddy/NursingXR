using Microsoft.MixedReality.QR;
using System;
using UnityEngine;

namespace GIGXR.Platform.HMD.QR
{
    [RequireComponent(typeof(SpatialGraphCoordinateSystem))]
    public class QrCodeMonoBehavior : MonoBehaviour
    {
        public string CodeText { get { return QrCode?.Data; } }

        public QRCode QrCode { get; private set; }

        private SpatialGraphCoordinateSystem connectedSpatialGraphCoordinateSystem;

        protected void Awake()
        {
            connectedSpatialGraphCoordinateSystem = GetComponent<SpatialGraphCoordinateSystem>();
        }

        public void Setup(Guid spatialGraphNode, QRCode qrCode)
        {
            connectedSpatialGraphCoordinateSystem.Id = spatialGraphNode;
            QrCode = qrCode;

            transform.up = Vector3.up;
        }
    }
}