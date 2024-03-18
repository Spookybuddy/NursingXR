using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GIGXR.Platform.ExtensionClasses
{
    using Object = UnityEngine.Object;

    public static class TransformExtensions
    {
        /// <summary>
        /// Non-recursive - gets a list of children beneath this transform. 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<Transform> GetChildren(this Transform t)
        {
            foreach (Transform c in t)
                yield return c;
        }

        /// <summary>
        /// Recursively searches the transform's children to find one with this name.
        /// Returns the first one found.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Transform FindChildOrGrandchild(this Transform t, string name)
        {
            foreach (Transform child in t.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name)
                {
                    return child;
                }
            }

            return null;
        }

        /// <summary>
        /// Non-recursive - just destroys all children found beneath this Transform.
        /// </summary>
        public static void DestroyChildren(this Transform transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                Object.Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Get a count of all immediate children whose GameObjects are currently active.
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static int GetActiveImmediateChildCount(this Transform transform)
        {
            int activeChildren = 0;
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeSelf)
                {
                    activeChildren++;
                }
            }
            return activeChildren;
        }

        public static void DestroyChildrenImmediate(this Transform transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                Object.DestroyImmediate(child.gameObject);
            }
        }
    }
}