using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Helpers
{
    public class DisplayCoinsHack : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<TMPro.TextMeshProUGUI>().text = string.Format("You stole {0} coins from the goblins while escaping.", Core.GameManager.CoinsStolen);
        }
    }
}