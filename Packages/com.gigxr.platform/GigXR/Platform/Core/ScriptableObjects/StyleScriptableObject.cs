using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GIGXR.Platform.Core
{
    [CreateAssetMenu(fileName = "New UI Style", menuName = "GIGXR/New UI Style")]
    public class StyleScriptableObject : ScriptableObject
    {
        [Header("Text")]
        public Color primaryTextColor;

        public TMP_FontAsset defaultFont;

        public float defaultFontSize;

        public Vector2 textBoxSize;

        [Header("Button")]
        public Color primaryButtonColor;

        public GameObject buttonPrefab;

        public Vector3 buttonSize;

        public Material buttonBackgroundMaterial;

        [Header("Background")]
        public Color primaryBackgroundColor;

        public Vector2 backgroundSize;

        public Material screenMaterial;

        public Material borderMaterial;

        [Header("Panel")]
        public Color panelColor;

        public Vector2 panelSize;

        public Material panelMaterial;

        [Header("Grip")]
        public Material gripMaterial;

        public Vector2 gripSize;

        [Header("Scrolling")]
        public GameObject scrollPrefab;

        [Header("Slate")]
        public GameObject slatePrefab;

        public GameObject slateButtonPrefab;

        public GameObject slatePureImagePrefab;

        public GameObject slatePureTextPrefab;

        public GameObject slateTextPrefab;

        public GameObject slateTextWithButtonPrefab;       
    }
}