using GIGXR.Platform.Scenarios.GigAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform
{
    public interface IBaseCommand
    {
        void Execute();
    }

    public abstract class BaseAssetCommand : IBaseCommand
    {
        public IAssetMediator AssetMediator => _assetMediator;

        private IAssetMediator _assetMediator;

        public abstract void Execute();

        public void SetAsset(IAssetMediator asset)
        {
            _assetMediator = asset;
        }
    }

    [Flags]
    public enum ProceduralBodyParts
    {
        None = 0,
        LeftEye = 1,
        RightEye = 2,
        LeftHand = 4,
        RightHand = 8,
        Head = 16
    }
}