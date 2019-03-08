using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core
{
    public class MessageBuffer : MonoBehaviour
    {
        private const int NumberOfLines = 25;

        private List<TMPro.TextMeshProUGUI> textComponents = new List<TMPro.TextMeshProUGUI>();

        public void AddMessage(Color color, string message)
        {
            // Push all text up
            for (int i = textComponents.Count - 1; i > 0; i--)
            {
                textComponents[i].text = textComponents[i - 1].text;
                textComponents[i].color = textComponents[i - 1].color;
                textComponents[i].alpha = 1f - (i / (float)textComponents.Count);
            }

            textComponents[0].text = message;
            textComponents[0].color = color;
        }

        private void Awake()
        {
            var firstLine = transform.GetChild(0).gameObject;
            var firstLineRect = firstLine.GetComponent<RectTransform>();
            textComponents.Add(firstLine.GetComponent<TMPro.TextMeshProUGUI>());

            // Spawn other lines
            for (int i = 1; i < NumberOfLines; i++)
            {
                var newLine = GameObject.Instantiate(firstLine, Vector3.zero, firstLine.transform.rotation);
                newLine.transform.SetParent(transform);

                RectTransform newLineRect = newLine.GetComponent<RectTransform>();
                newLineRect.position = firstLineRect.position;
                newLineRect.anchoredPosition = firstLineRect.anchoredPosition + new Vector2(0, 20f * i);
                newLineRect.localScale = firstLineRect.localScale;
                textComponents.Add(newLine.GetComponent<TMPro.TextMeshProUGUI>());
            }

            // Set line settings
            for (int i = 0; i < textComponents.Count; i++)
            {
                var line = textComponents[i];
                line.text = "";
                line.alpha = 1f - (i / (float)NumberOfLines);
            }
        }

        private void Update()
        {
        }
    }
}