namespace GIGXR.Platform
{
    using GIGXR.GMS.Models.Classes.Resposnes;
    using GIGXR.GMS.Models.Sessions;
    using GIGXR.Platform.Downloads.Data;
    using Newtonsoft.Json.Utilities;
    using System;
    using UnityEngine;

    public class AotTypeEnforcer : MonoBehaviour
    {
        private void Awake()
        {
            AotHelper.EnsureList<Guid>();
            AotHelper.EnsureList<ClassLeafView>();
            AotHelper.EnsureList<SessionResourceView>();
            AotHelper.EnsureList<DownloadInfo>();
        }
    }
}
