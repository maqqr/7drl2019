﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GoblinKing.UI
{
    public class PerkButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public delegate void MouseEvent(PointerEventData eventData);

        public event MouseEvent MouseEnter;
        public event MouseEvent MouseExit;
        public event MouseEvent MouseClick;

        public string PerkKey;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (MouseEnter != null)
            {
                MouseEnter(eventData);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (MouseExit != null)
            {
                MouseExit(eventData);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (MouseClick != null)
            {
                MouseClick(eventData);
            }
        }
    }
}