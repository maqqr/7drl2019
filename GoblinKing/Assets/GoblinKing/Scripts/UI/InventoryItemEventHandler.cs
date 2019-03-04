using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GoblinKing.UI
{
    public class InventoryItemEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public delegate void MouseEvent();

        public event MouseEvent MouseEnter;
        public event MouseEvent MouseExit;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (MouseEnter != null)
            {
                MouseEnter();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (MouseExit != null)
            {
                MouseExit();
            }
        }
    }
}