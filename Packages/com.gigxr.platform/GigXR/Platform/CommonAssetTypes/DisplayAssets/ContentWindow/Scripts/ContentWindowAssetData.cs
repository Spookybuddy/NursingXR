using UnityEngine;

namespace GIGXR.Platform.CommonAssetTypes.DisplayAssets.ContentWindow.Scripts
{
    using GIGXR.Platform.Scenarios.GigAssets.Data;
    using System;

    [System.Serializable]
    public class ContentWindowAssetData : BaseAssetData
    {
        /// <summary>
        /// The text to display at the top of the content window.
        /// </summary>
        public AssetPropertyDefinition<string> headerText;

        /// <summary>
        /// Content can be text or an image.
        /// If the content starts with the text [KEY], then the
        /// asset will try to display an image using the subsequent
        /// text as an Addressable key.
        /// Example. [KEY]scroll-sizer
        /// </summary>
        public AssetPropertyDefinition<TextBox> content;

        /// <summary>
        /// A serializable field which can be used to store text.
        /// This field supports subscripts, superscripts, italics, etc.
        /// See TextMeshPro docs for details on how to display various
        /// things. (Typically using tags like <b>hello</b>)
        /// </summary>
        [Serializable]
        public class TextBox
        {
            [TextArea(5, 10)]
            public string text;

            public TextBox(string setText)
            {
                this.text = setText;
            }
        }
    }
}
