namespace GIGXR.Platform.Scenarios.GigAssets.Loader
{
    using Cysharp.Threading.Tasks;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Responsible for loading asset types into memory.
    /// </summary>
    public interface IAssetTypeLoader
    {
        IReadOnlyDictionary<string, GameObject> LoadedAssetTypes { get; }

        UniTask LoadAssetTypesAsync(ISet<string> assetTypeIds);
        UniTask UnloadAllAssetTypesAsync();
    }
}