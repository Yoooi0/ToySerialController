using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ToySerialController.UI
{
    public class UIHorizontalGroup
    {
        public readonly UIDynamic container;
        public readonly GameObject gameObject;

        public List<GameObject> items = new List<GameObject>();

        public UIHorizontalGroup(UIDynamic container, float width, float height, Vector2 spacing, int count, Func<int, Transform> itemCreator)
        {
            this.container = container;

            gameObject = new GameObject();
            gameObject.transform.SetParent(container.gameObject.transform, false);
            
            var rectTransform = gameObject.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, 0);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            
            var gridLayout = gameObject.AddComponent<GridLayoutGroup>();
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = count;
            gridLayout.spacing = spacing;
            gridLayout.cellSize = new Vector2((width - spacing.x * (count - 1)) / count, height);
            gridLayout.childAlignment = TextAnchor.MiddleCenter;

            for (var i = 0; i < count; i++)
            {
                var item = itemCreator(i);
                item.gameObject.transform.SetParent(gridLayout.transform, false);
                items.Add(item.gameObject);
            }
        }
    }
}
