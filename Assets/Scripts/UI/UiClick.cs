using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TopDown
{
    /// <summary>Forwards pointer clicks / hover on a UI element to callbacks.</summary>
    public class UiClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Action<PointerEventData> Clicked;
        public Action Entered;
        public Action Exited;

        public void OnPointerClick(PointerEventData eventData) => Clicked?.Invoke(eventData);
        public void OnPointerEnter(PointerEventData eventData) => Entered?.Invoke();
        public void OnPointerExit(PointerEventData eventData) => Exited?.Invoke();
    }
}
