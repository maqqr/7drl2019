using UnityEditor;
using UnityEngine;
using GoblinKing.Core.Interaction;

namespace GoblinKing.Editor
{
    [CustomEditor(typeof(Interactable))]
    public class ForceInteraction : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (EditorApplication.isPlaying)
            {
                Interactable interactable = (Interactable)target;

                if (GUILayout.Button("Force Interaction"))
                {
                    var gm = GameObject.FindObjectOfType<GoblinKing.Core.GameManager>();

                    foreach (var i in interactable.Interactions)
                    {
                        i.Interact(gm);
                    }
                }
            }
        }
    }
}
