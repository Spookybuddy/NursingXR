using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;

namespace GIGXR.Platform.Utilities
{
    /// <summary>
    /// This is a workaround for an issue with TMP_InputField interaction with the HoloLens system keyboard.
    /// When the HoloLens system keyboard is closed via the return or x button, the TMP_InputField is not deselected.
    /// When the TMP_InputField is subsequently deselected via the selection of something else (or the selection of
    /// nothing a targetless pinch gesture), it brings the keyboard back up in order to close it.
    /// 
    /// This ensures that the TMP_InputField is deselected when the keyboard is closed. More specifically, it ensures
    /// that the TMP_InputField is deselected when it raises its <see cref="TMP_InputField.onEndEdit"/> event is invoked.
    /// 
    /// This should be placed on the same GameObject as the TMP_InputField for which it is intended to manage selection.
    /// </summary>
    [RequireComponent(typeof(TMP_InputField))]
    public class InputFieldDeselectionHelper : MonoBehaviour
    {
        private TMP_InputField inputField;

        private bool isSelected;

        private void Awake()
        {
            inputField = GetComponent<TMP_InputField>();

            inputField.onSelect.AddListener(OnSelect);
            inputField.onDeselect.AddListener(OnDeselect);
            inputField.onEndEdit.AddListener(OnEndEdit);
        }

        private void OnSelect(string inputFieldContents)
        {
            isSelected = true;
        }

        private void OnDeselect(string inputFieldContents)
        {
            isSelected = false;
        }

        private void OnEndEdit(string inputFieldContents)
        {
            var eventSystem = EventSystem.current;
            if (isSelected && !eventSystem.alreadySelecting)
            {
                eventSystem.SetSelectedGameObject(null);
            }
        }
    }
}
