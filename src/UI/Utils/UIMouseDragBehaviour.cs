using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ToySerialController.UI
{
    public class UIMouseDragBehaviour : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public RectTransform draggingRect;

        public event EventHandler<PointerEventArgs> OnDragBegin;
        public event EventHandler<PointerEventArgs> OnDragEnd;
        public event EventHandler<PointerEventArgs> OnDragging;

        public void Awake()
        {
            if(draggingRect == null)
                draggingRect = transform.parent.GetComponent<RectTransform>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            SetDraggedPosition(eventData);
            OnDragBegin?.Invoke(this, new PointerEventArgs(eventData));
        }

        public void OnDrag(PointerEventData eventData)
        {
            SetDraggedPosition(eventData);
            OnDragging?.Invoke(this, new PointerEventArgs(eventData));
        }

        private void SetDraggedPosition(PointerEventData eventData)
        {
            var rectTransform = GetComponent<RectTransform>();

            Vector3 worldPoint;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingRect, eventData.position, eventData.pressEventCamera, out worldPoint))
            {
                rectTransform.position = worldPoint;
                rectTransform.rotation = draggingRect.rotation;
            }

            var min = draggingRect.rect.min;
            var max = draggingRect.rect.max;
            var pos = rectTransform.localPosition;
            pos.x = Mathf.Clamp(pos.x, min.x, max.x);
            pos.y = Mathf.Clamp(pos.y, min.y, max.y);
            rectTransform.localPosition = pos;
        }

        public void OnEndDrag(PointerEventData eventData) => OnDragEnd?.Invoke(this, new PointerEventArgs(eventData));
    }
}