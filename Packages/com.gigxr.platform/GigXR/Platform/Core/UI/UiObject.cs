using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace GIGXR.Platform.UI
{
    using HMD;
    using System;

    [SelectionBase]
    public class UiObject : BaseUiObject
    {
        [SerializeField] private MeshRenderer[] assignedRenderers;

        [SerializeField] [Range(0, 1)]
        
        private float fadeAlpha = .2f;

        [SerializeField] private bool fadeRgb = false;

        private Color color;
        private float fullAlpha = 1;
        protected bool ignoreGlobalDisable = false;

        //protected MeshRenderer[] Renderers
        //{
        //    get
        //    {
        //        if (_renderers == null)
        //        {
        //            InitializeRenderers();
        //        }

        //        return _renderers;
        //    }
        //}

        //private MeshRenderer[] _renderers;
        //private List<TextMeshProUGUI> rendererTexts = new List<TextMeshProUGUI>();
        //private List<Material> rendererMaterials = new List<Material>();

        //protected Image[] Sprites
        //{
        //    get
        //    {
        //        if(_sprites == null)
        //            _sprites = GetComponentsInChildren<Image>(true);

        //        return _sprites;
        //    }
        //}

        //private Image[] _sprites;

        private Image image;
        private Button mobileButton;
        protected Vector3 startLocalPosition;

        // --- Public Properties:

        public string Name { get { return name; } }
        public bool IsActive { get { return gameObject.activeInHierarchy; } }
        public Transform StartTransform { get; set; }

        public Vector3 Position { get; set; }
        public Collider Collider { get; set; }

        public Vector3 LocalPosition
        {
            get { return transform.localPosition; }
        }

        // --- Public Methods:

        /// <summary>
        /// Initialises the UIObject's data. Call after instantiation and parenting.
        /// </summary>
        public virtual void Initialize()
        {
            if (gameObject.activeSelf) 
                SetActive(true);

            StartTransform = GetTransform();
            startLocalPosition = transform.localPosition;
            Collider = GetComponent<Collider>();
#if UNITY_IOS || UNITY_ANDROID
            mobileButton = GetComponent<Button>();
            image = GetComponent<Image>();
#endif
        }

        /// <summary>
        /// Apply a position to this GameObject
        /// </summary>
        /// <param name="position">The position to apply to this GameObject</param>
        public void SetLocalPosition(Vector3 position)
        {
            transform.localPosition = position;
        }

        /// <summary>
        /// Apply a rotation to this GameObject
        /// </summary>
        /// <param name="rotation">The rotation to apply to this object</param>
        public void SetLocalRotation(Quaternion rotation)
        {
            transform.localRotation = rotation;
        }

        /// <summary>
        /// Define whether this GameObject is active
        /// </summary>
        /// <param name="isActive">Set the active state of this GameObject</param>
        public virtual void SetActive(bool isActive)
        {
            // TODO thhis crashes on mobile sometimes (NRE) due to there being no MRTK InputSystem
            try
            {
                gameObject.SetActive(isActive);
            }
            catch(Exception e)
            {
                Debug.Log("Caught exception when trying to activate UiObject: ");
                Debug.LogException(e);
            }


            if (Collider != null) Collider.enabled = isActive;
        }

        /// <summary>
        /// Set the alpha of this objects material
        /// </summary>
        /// <param name="alpha">The alpha value to set</param>
        public void SetAlpha(float alpha, bool useStartValue = false)
        {
#if UNITY_IOS || UNITY_ANDROID
            if (image != null)
            {
                var imageColor = image.color;
                imageColor.a = alpha;
                image.color = imageColor;
            }
            else
            {
                var targetGraphic = mobileButton.targetGraphic;
                var buttonColor = targetGraphic.color;
                buttonColor.a = alpha;
                targetGraphic.color = buttonColor;
            }
#endif
            //if (Renderers == null) return;

            //foreach (var material in rendererMaterials)
            //{
            //    color = material.color;
            //    if (fadeRgb)
            //    {
            //        color.r = useStartValue ? fullAlpha : alpha;
            //        color.g = useStartValue ? fullAlpha : alpha;
            //        color.b = useStartValue ? fullAlpha : alpha;
            //    }
            //    else
            //    {
            //        color.a = useStartValue ? fullAlpha : alpha;
            //    }

            //    material.color = color;
            //}

            //foreach(var text in rendererTexts)
            //{
            //    text.alpha = useStartValue ? fullAlpha : alpha;
            //}

            //foreach (Image sprite in Sprites)
            //{
            //    sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b,
            //        useStartValue ? fullAlpha : alpha);
            //}
        }

        /// <summary>
        /// Get the current position of the UI object
        /// </summary>
        /// <returns></returns>
        public Transform GetTransform()
        {
            return transform;
        }

        /// <summary>
        /// Set a Transform as parent
        /// </summary>
        /// <param name="parentTransform">The transform to set as the parent</param>
        public void SetTransform(Transform parentTransform)
        {
            transform.parent = parentTransform;
        }

        /// <summary>
        /// Remove the UI object from its parent gameobject
        /// </summary>
        public void RemoveFromParent()
        {
            transform.parent = null;
        }

        /// <summary>
        /// Set the UI object Collider enabled
        /// </summary>
        /// <param name="disable"></param>
        /// <param name="fade"></param>
        public virtual void IsDisabled(bool disable, bool fade = false)
        {
#if UNITY_IOS || UNITY_ANDROID
            mobileButton.enabled = !disable;
            return;
#endif
            if (ignoreGlobalDisable) 
                return;
            
            if (Collider != null)
            {
                Collider.enabled = !disable;
            }

            if (fade) 
                SetAlpha(fadeAlpha, !disable);
            else 
                SetAlpha(fullAlpha);
        }

        /// <summary>
        /// Sets the UI object to ignore the global disable function. Useful for prompts.
        /// </summary>
        /// <param name="ignore"></param>
        public void IgnoreGlobalDisable(bool ignore)
        {
            ignoreGlobalDisable = ignore;
        }

        protected override void SubscribeToEventBuses()
        {
            // Not needed here
        }

        //public void InitializeRenderers()
        //{
        //    if (assignedRenderers == null || assignedRenderers.Length == 0)
        //        _renderers = GetComponentsInChildren<MeshRenderer>(true);
        //    else
        //        _renderers = assignedRenderers;

        //    foreach (var renderer in _renderers)
        //    {
        //        if (renderer != null)
        //        {
        //            if (renderer.material.HasProperty("_Color"))
        //            {
        //                rendererMaterials.Add(renderer.material);
        //            }
        //            else if (renderer.transform.TryGetComponent<TextMeshProUGUI>(out var text))
        //            {
        //                rendererTexts.Add(text);
        //            }
        //        }
        //    }
        //}
    }
}