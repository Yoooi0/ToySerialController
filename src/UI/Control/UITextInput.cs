using UnityEngine;
using UnityEngine.UI;

namespace ToySerialController.UI
{
    public class UITextInput : UIHorizontalGroup
    {
        public JSONStorableString storable;

        public UITextInput(UIDynamic container, float height, string label, string paramName, string startingValue)
            : base(container, 510, height, new Vector2(10, 0), 2, null)
        {
            storable = new JSONStorableString(paramName, startingValue);
            UIManager.RegisterString(storable);

            var gridLayout = gameObject.GetComponent<GridLayoutGroup>();

            var labelTransform = new GameObject();
            labelTransform.transform.SetParent(gridLayout.transform, false);

            var textRect = labelTransform.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 0);
            textRect.anchoredPosition = new Vector2(5f, 5f);
            textRect.pivot = new Vector2(0, 0);
            textRect.sizeDelta = new Vector2(0, 30);

            var textText = labelTransform.AddComponent<Text>();
            textText.raycastTarget = false;
            textText.alignment = TextAnchor.MiddleCenter;
            textText.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            textText.color = Color.white;
            textText.text = label;
            textText.fontSize = 28;

            var textFieldTransform = GameObject.Instantiate<Transform>(UIManager.ConfigurableTextFieldPrefab);
            textFieldTransform.transform.SetParent(gridLayout.transform, false);

            var textField = textFieldTransform.GetComponent<UIDynamicTextField>();
            storable.dynamicText = textField;

            var layoutElement = textField.gameObject.GetComponent<LayoutElement>();
            layoutElement.minHeight = height;
            layoutElement.preferredHeight = height;

            var input = textField.gameObject.AddComponent<InputField>();
            input.textComponent = textField.UItext;
            input.lineType = InputField.LineType.SingleLine;
            storable.inputField = input;
        }
    }
}
