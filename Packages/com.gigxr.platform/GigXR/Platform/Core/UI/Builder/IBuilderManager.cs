using GIGXR.Platform.Core;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public interface IBuilderManager
{
    StyleScriptableObject DefaultStyle { get; }

    Vector3 InteractionLevelOffset { get; }

    GameObject BuildScreenObject();

    GameObject BuildBackground();

    GameObject BuildPanel();

    ScrollingObjectCollection BuildScrollingPanel(Vector3? clippingSize = null, float? cellHeight = null, bool collectionIgnoreInactive = true);

    GameObject BuildImage(Vector2? size = null, Sprite? sprite = null, Color? imageColor = null,
        bool isStretched = false, Transform parent = null);

    GameObject BuildTextInput(string defaultText, float height, float width);

    GameObject BuildMRTKButton(string buttonText, Action buttonClick);

    GameObject BuildButton(UnityAction buttonClick, string buttonText = "", Vector3? buttonSize = null, Color? backgroundColor = null,
                           float? fontSize = null, TextAlignmentOptions? alignmentOptions = null, Vector4? textMargins = null,
                           Material? buttonBackground = null);

    Button BuildSlateButton(UnityAction buttonClick, string buttonText = "", Vector2? buttonSize = null, bool useBackground = false,
                           float? fontSize = null, TextAlignmentOptions? alignmentOptions = null, Vector4? textMargins = null,
                           Transform parent = null, RectTransformExtensions.PivotPresets? pivot = null, RectTransformExtensions.AnchorPresets? anchor = null,
                           Vector3 ? anchorOffset = null);

    GameObject BuildMRTKButton(Action buttonClick, string buttonText = "", Vector3? buttonSize = null, 
                           ButtonIconStyle? buttonIconStyle = null, Vector3? buttonLocation = null, 
                           Color? buttonColor = null, bool showSeeItSayItLabel = false, float? fontSize = null,
                           TextAlignmentOptions? alignmentOptions = null, Vector4? textMargins = null, Material? quadMaterial = null,
                           string quadIconName = null, Vector3? backplateOffset = null);

    GameObject BuildText(string text = "");

    GameObject BuildText(string text = "", Vector2? textBoxSize = null, Vector3? textLocationOffset = null,
                         Color? textColor = null, float? fontSize = null, TextAlignmentOptions? alignmentOptions = null,
                         Vector4? textMargins = null, FontStyles? textStyle = null, TextOverflowModes? overflowMode = null,
                         bool useUGUI = true, RectTransformExtensions.AnchorPresets? anchor = null, Transform parent = null,
                         Vector2? preferedSize = null);

    GameObject BuildQuad(Vector2? size = null, Material material = null, bool isVisible = true);

    GridObjectCollection BuildObjectCollection(LayoutOrder containerLayout, Vector2 cellSize, CollationOrder sortOrder = CollationOrder.ChildOrder, 
                                               bool ignoreInactiveTransforms = true, LayoutAnchor anchor = LayoutAnchor.MiddleCenter);

    GridLayoutGroup BuildObjectCollection(Vector2 cellSize, Vector2 spacing, GridLayoutGroup.Corner corner = GridLayoutGroup.Corner.UpperLeft, 
        GridLayoutGroup.Constraint constraints = GridLayoutGroup.Constraint.Flexible, int constraintCount = 1);

    VerticalLayoutGroup BuildVerticalCollection(float spacing);

    HorizontalLayoutGroup BuildHorizontalCollection(float spacing);

    Slate BuildSlate(string title);
}
