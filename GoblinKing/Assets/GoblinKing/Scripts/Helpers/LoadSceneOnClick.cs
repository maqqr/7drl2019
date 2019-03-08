using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Helpers
{
    public class LoadSceneOnClick : MonoBehaviour
    {
        public void LoadScene(string sceneName)
        {
            if (sceneName == "Quit")
            {
                Application.Quit();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }
    }
}
