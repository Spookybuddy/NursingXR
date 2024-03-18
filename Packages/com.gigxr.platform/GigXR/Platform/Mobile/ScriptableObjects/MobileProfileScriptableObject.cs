using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GIGXR.Platform.Mobile
{
    [CreateAssetMenu(fileName = "Mobile Profile", menuName = "GIGXR/ScriptableObjects/New Mobile Profile")]
    public class MobileProfileScriptableObject : ScriptableObject
    {
        [Header("AR Properties")]
        //The prefab for the AR placement target
        public GameObject ArTargetPrefab;

        //The prefab for the AR plane used by the Plane Manager
        public GameObject ArPlanePrefab;

        //The min m2 required by the scan to initiate the Placement state
        public float ScanArea;

        [Space]
        [Header("Mobile UI Properties")]
        public MobileText MobileText;

        [Space] public MobileImage MobileImage;

        [Space]
        [Header("Downloads")]
        [Tooltip("Warn for low storage at xMB")]
        public int LowStorageWarning = 512;

        [Space]
        [Header("Firebase Properties")]
        public bool EnableCloudMessaging;

        public bool EnableDynamicLinks;
        public string DynamicLinkUrl;
        public bool ShouldInitFirebase => EnableCloudMessaging && EnableDynamicLinks;
    }

    public enum TextType
    {
        Header,
        SubHeader,
        Body,
        Prompt
    }

    [Serializable]
    public class MobileText
    {
        [Header("Header Properties")] public int HeaderFontSize;
        public TMP_FontAsset HeaderFont;
        [Header("Sub Header Properties")] public int SubHeaderFontSize;
        public TMP_FontAsset SubHeaderFont;
        [Header("Body Properties")] public int BodyFontSize;
        public TMP_FontAsset BodyFont;
        [Header("Prompt Properties")] public int PromptFontSize;
        public TMP_FontAsset PromptFont;

        public TextProperties ReturnTextConfig(TextType textType)
        {
            TextProperties textProperties = new TextProperties();

            switch (textType)
            {
                case TextType.Header:
                    textProperties.FontSize = HeaderFontSize;
                    textProperties.Font = HeaderFont;
                    break;
                case TextType.SubHeader:
                    textProperties.FontSize = SubHeaderFontSize;
                    textProperties.Font = SubHeaderFont;
                    break;
                case TextType.Body:
                    textProperties.FontSize = BodyFontSize;
                    textProperties.Font = BodyFont;
                    break;
                case TextType.Prompt:
                    textProperties.FontSize = PromptFontSize;
                    textProperties.Font = PromptFont;
                    break;
            }

            return textProperties;
        }
    }

    public class TextProperties
    {
        public int FontSize;
        public TMP_FontAsset Font;
    }

    public enum ImageType
    {
        Header,
        Body,
        Prompt
    }

    [Serializable]
    public class MobileImage
    {
        [Header("Header Properties")] public Color HeaderColor;
        [Header("Body Properties")] public Color BodyColor;
        [Header("Prompt Properties")] public Color PromptColor;

        public ImageProperties ReturnImageConfig(ImageType imageType)
        {
            ImageProperties imageProperties = new ImageProperties();
            //todo Add navigation buttons
            switch (imageType)
            {
                case ImageType.Header:
                    imageProperties.Color = HeaderColor;
                    break;
                case ImageType.Body:
                    imageProperties.Color = BodyColor;
                    break;
                case ImageType.Prompt:
                    imageProperties.Color = PromptColor;
                    break;
            }

            return imageProperties;
        }
    }

    public class ImageProperties
    {
        public Color Color;
    }
}