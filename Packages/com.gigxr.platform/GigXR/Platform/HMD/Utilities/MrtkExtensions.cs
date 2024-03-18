using Cysharp.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using UnityEngine;

namespace GIGXR.Platform.Utilities
{
    public static class MrtkExtensions
    {
        public static void AddContent(this GridObjectCollection collection, GameObject content)
        {
            content.transform.SetParent(collection.transform, false);
        }

        public static void UpdateCollectionNextFrame(this GridObjectCollection collection, int framesToWait = 2)
        {
            UniTask.Create(async () =>
            {
                await UniTask.DelayFrame(framesToWait);

                collection.UpdateCollection();
            });
        }

        public static void AddScrollableContent(this ScrollingObjectCollection scrollingCollection, GameObject content, int framesToWait = 2)
        {
            var container = scrollingCollection.GetComponentInChildren<GridObjectCollection>(true);

            scrollingCollection.AddContent(content);

            UniTask.Create(async () =>
            {
                await UniTask.DelayFrame(framesToWait);

                container?.UpdateCollection();

                scrollingCollection.UpdateContent();
            });
        }

        public static void UpdateCollection(this ScrollingObjectCollection scrollingCollection, int framesToWait = 2)
        {
            var container = scrollingCollection.GetComponentInChildren<GridObjectCollection>(true);

            UniTask.Create(async () =>
            {
                await UniTask.DelayFrame(framesToWait);

                container?.UpdateCollection();
            });
        }
    }
}