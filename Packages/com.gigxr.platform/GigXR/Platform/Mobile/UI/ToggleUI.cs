using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GIGXR.Platform.Mobile.UI
{
    /// <summary>
    /// Helper class to allow for toggling button color and text whilst invoking Unity Events applied in the editor
    /// </summary>
    public class ToggleUI : MonoBehaviour
    {
        /// <summary>
        /// The button to toggle
        /// </summary>
        [Header("Toggle Button")] [SerializeField]
        private Button button;

        /// <summary>
        /// The text to toggle
        /// </summary>
        [Header("Toggle Text")] [SerializeField]
        private TextMeshProUGUI tmp;

        /// <summary>
        /// The toggle colour
        /// </summary>
        [Header("Toggle Colour")] [SerializeField]
        private Color toggleColor;

        /// <summary>
        /// If the text colour should be toggled
        /// </summary>
        [Header("Toggle Text Colour")] [SerializeField]
        private bool toggleTextColor;

        /// <summary>
        /// The text when toggle is on
        /// </summary>
        [Space] [Header("Toggle Text")] [SerializeField]
        private string toggleOnText;

        /// <summary>
        /// The text when toggle is off
        /// </summary>
        [SerializeField] private string toggleOffText;

        /// <summary>
        /// The events to invoke when toggle is on
        /// </summary>
        [Space] [Header("Toggle Events")] [SerializeField]
        private UnityEvent toggleOnEvent;

        /// <summary>
        /// The events to invoke when toggle is off
        /// </summary>
        [SerializeField] private UnityEvent toggleOffEvent;

        /// <summary>
        /// Reference to the event trigger, assigned in Awake
        /// </summary>
        private EventTrigger trigger;

        /// <summary>
        /// Reference to the start colour, assigned in Awake
        /// </summary>
        private Color startColor;

        private void Awake()
        {
            //Cache the start colour
            startColor = button.image.color;

            //Get the event trigger and add a listener
            trigger = GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((data) => { OnPointerUpDelegate((PointerEventData)data); });
            trigger.triggers.Add(entry);
        }

        /// <summary>
        /// On Pointer Up Delegate which listens for the Pointer Down callback
        /// </summary>
        /// <param name="eventData">The event data passed via the pointer down click</param>
        private void OnPointerUpDelegate(PointerEventData eventData)
        {
            //If the image is in the start state, toggle on
            if (button.image.color == startColor)
            {
                button.image.color = toggleColor;
                if (toggleTextColor)
                    tmp.color = toggleColor;
                toggleOnEvent.Invoke();
                tmp.SetText(toggleOnText);

                print("On");
            }
            //Else toggle off
            else
            {
                button.image.color = startColor;
                if (toggleTextColor)
                    tmp.color = startColor;
                toggleOffEvent.Invoke();
                tmp.SetText(toggleOffText);

                print("Off");
            }
        }
    }
}