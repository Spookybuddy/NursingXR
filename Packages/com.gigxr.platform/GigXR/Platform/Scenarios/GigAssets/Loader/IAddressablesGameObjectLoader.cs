namespace GIGXR.Platform.Scenarios.GigAssets.Loader
{
    using Cysharp.Threading.Tasks;
    using System.Collections.Generic;
    using UnityEngine;

    public interface IAddressablesGameObjectLoader
    {
        IReadOnlyDictionary<string, GameObject> LoadedGameObjects { get; }

        UniTask LoadGameObjectAsync(string addressableKey);
        UniTask LoadGameObjectsAsync(ISet<string> addressableKeys);

        UniTask<GameObject> InstantiateGameObject(string key, Transform spawnTransform);
        UniTask UnloadAllAddressableGameObjectsAsync();

        // TODO Not ideal, but we need this to reference the Asset Manager as well
        void SetManager(IGigAssetManager asssetManager);
    }
}